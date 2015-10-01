using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation;


namespace TfsMonitor.Api.Work
{
	public class WorkMonitor : Monitor<List<WorkItem>>
	{
		private WorkItemStore workItemStore;
		private List<WorkItem> lastCheck = new List<WorkItem>();

		private struct Iteration
		{
			public string Project;
			public string Path;
			public DateTime StartDate;
			public DateTime EndDate;
		}

		public WorkMonitor()
		{
			workItemStore = (WorkItemStore)TeamProjectCollection.GetService(typeof(WorkItemStore));

			int configMonitorRefreshIntervalMS;
			if(int.TryParse(ConfigurationManager.AppSettings["WorkMonitorRefreshIntervalMS"], out configMonitorRefreshIntervalMS)) {
				MonitorRefreshIntervalMS = configMonitorRefreshIntervalMS;
			}
		}

		private void FindCurrentIteration(ICommonStructureService4 css, string projectUri, List<Iteration> result)
		{

			NodeInfo[] structures = css.ListStructures(projectUri);
			NodeInfo iterations = structures.FirstOrDefault(n => n.StructureType.Equals("ProjectLifecycle"));

			System.Xml.XmlElement iterationsTree = css.GetNodesXml(new[] { iterations.Uri }, true);

			if (iterations != null)
			{
				string projectName = css.GetProject(projectUri).Name;
				FindCurrentIteration(iterationsTree.ChildNodes[0], projectName, result);
			}
		}

		/// <summary>
		/// Recursively searches the iteration XmlNode for an iteration in the specified project that 
		/// has a start date before today and an end date after today (inclusive). Stops after the first 
		/// iteration that passes those criteria is found. The iteration is added to the result list.
		/// </summary>
		/// <param name="node">The root node to look for iterations in</param>
		/// <param name="projectName">The project to search in</param>
		/// <param name="result">The list the found iteration is to be added to.</param>
		private void FindCurrentIteration(System.Xml.XmlNode node, string projectName, List<Iteration> result)
		{
			//check if the project is already in the result, if it is return

			if (node != null)
			{
				string iterationPath = node.Attributes["Path"].Value;
				if (!string.IsNullOrEmpty(iterationPath))
				{
					// Attempt to read the start and end dates if they exist.
					string strStartDate = (node.Attributes["StartDate"] != null) ? node.Attributes["StartDate"].Value : null;
					string strEndDate = (node.Attributes["FinishDate"] != null) ? node.Attributes["FinishDate"].Value : null;

					DateTime startDate = DateTime.MinValue;
					DateTime endDate = DateTime.MinValue;
					bool datesValid;

					// Both dates should be valid.
					datesValid = DateTime.TryParse(strStartDate, out startDate) && DateTime.TryParse(strEndDate, out endDate);

					if (!string.IsNullOrEmpty(strStartDate) && !string.IsNullOrEmpty(strEndDate)
						&& datesValid && startDate <= DateTime.Now && endDate.AddDays(1) >= DateTime.Now) //endDate is midnight of the last day of the sprint. add 1 day to include the final day
					{
						result.Add(new Iteration()
						{
							Project = projectName,
							Path = iterationPath.Replace(string.Concat("\\", projectName, "\\Iteration"), projectName),
							StartDate = startDate,
							EndDate = endDate
						});
					}
					else
					{
						if (node.FirstChild != null)
						{
							for (int child = 0; child < node.ChildNodes[0].ChildNodes.Count; child++)
							{
								FindCurrentIteration(node.ChildNodes[0].ChildNodes[child], projectName, result);
							}

						}
					}




				}


			}
		}

		/// <summary>
		/// Retrieves all work items for the current sprint in all projects
		/// </summary>
		public List<WorkItem> GetWorkItems()
		{

			ICommonStructureService4 css = TeamProjectCollection.GetService<ICommonStructureService4>();
			List<Iteration> currentIterations = new List<Iteration>();

			List<WorkItem> result = new List<WorkItem>();
			foreach (Project project in workItemStore.Projects.Cast<Project>().Where(p => !ProjectRegexExists || ProjectRegex.IsMatch(p.Name)))
			{
				try
				{
					FindCurrentIteration(css, project.Uri.ToString(), currentIterations);

					Dictionary<string, string> parameters = new Dictionary<string, string>();

					parameters.Add("Project", project.Name);
					Iteration iteration;
					try
					{
						iteration = currentIterations.Where(i => i.Project == project.Name).SingleOrDefault();
					}
					catch (System.InvalidOperationException iox)
					{
						throw new Exception(string.Format("There cannot be more than one current iteration on the {0} project.", project.Name), iox);
					}
					string iterationPath = iteration.Path;
					if (iterationPath != null)
					{
						parameters.Add("IterationPath", iterationPath);

						WorkItemCollection queryResults = workItemStore.Query(
						   @"Select Id, Title, System.TeamProject, State, [Assigned To]
						From WorkItems 
						Where 
						[Work Item Type] in ('Bug' , 'Product Backlog Item') 
						and State <> 'Done' and State <> 'Closed' and State <> 'Removed'
						and System.TeamProject = @Project and System.IterationPath = @IterationPath",
								parameters
						   );

						foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem workItem in queryResults)
						{
							//TODO this is really slow. ALl the queries should be combined into one, with dataprocessing on the
							//performed here to figure all this stuff out
							Dictionary<Activity, double> workRemaining = new Dictionary<Activity, double>();
							foreach (WorkItemLink link in workItem.WorkItemLinks)
							{
								if (link.LinkTypeEnd.Name == "Child")
								{
									var item = workItemStore.GetWorkItem(link.TargetId);
									if (item.Type.Name == "Task")
									{
										var value = item.Fields["Remaining Work"].Value;
										if (value != null)
										{
											//if activity is empty, set to None
											Activity activity = Activity.None;
											Enum.TryParse<Activity>(item.Fields["Activity"].Value.ToString(), true, out activity);

											if (!workRemaining.ContainsKey(activity))
											{
												workRemaining.Add(activity, 0);
											}
											workRemaining[activity] += double.Parse(value.ToString());
										}
									}
								}
							}
							result.Add(new WorkItem()
							{
								Title = workItem.Title,
								Type = workItem.Type.Name,
								Project = workItem.Project.Name,
								WorkItemID = workItem.Id,
								State = workItem.State,
								Assignee = workItem.Fields["Assigned To"].Value.ToString(),
								DueDate = iteration.EndDate,
								Iteration = iterationPath.Split(' ', '\\').Last(), //the last word or last component in path
								WorkRemaining = workRemaining
							});


						}

					}
				}
				catch (Exception ex)
				{
					//swallow security exceptions - if the current user doesn't have access to the requested project, just 
					//don't show it. Continue to show other projects the user has access to.
					if (!(ex is System.Security.SecurityException))
					{
						throw;
					}

				}
			}

			return result;

		}

		protected override void MonitorAction() {
			try {
				List<WorkItem> current = GetWorkItems();
				if(!Enumerable.SequenceEqual(lastCheck, current)) {
					lastCheck = current;
					NotifyAll(current);
				}
			}
			catch(Exception ex) {
				NotifyError(ex);
				lastCheck = new List<WorkItem>(); //reset the lastCheck so that next time around, it'll try to broadcast if no errors are shown. This will clear the error 
				//message on the client
			}
		}

		public List<WorkItem> GetLast() {
			return lastCheck;
		}
	}
}
