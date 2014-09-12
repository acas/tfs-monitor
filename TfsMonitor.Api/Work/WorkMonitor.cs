using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Server;
using System.Collections;
using Microsoft.TeamFoundation;


namespace TfsMonitor.Api.Work
{
	public class WorkMonitor : Monitor
	{
		private WorkItemStore workItemStore;

		public WorkMonitor()
		{
			workItemStore = (WorkItemStore)TeamProjectCollection.GetService(typeof(WorkItemStore));
		}

		private void FindCurrentIteration(ICommonStructureService4 css, string uri, List<object> result)
		{

			NodeInfo[] structures = css.ListStructures(uri);
			NodeInfo iterations = structures.FirstOrDefault(n => n.StructureType.Equals("ProjectLifecycle"));

			System.Xml.XmlElement iterationsTree = css.GetNodesXml(new[] { iterations.Uri }, true);

			if (iterations != null)
			{
				string projectName = css.GetProject(uri).Name;
				result.Add(iterations);
				FindCurrentIteration(iterationsTree.ChildNodes[0], projectName, result);
			}
		}

		private static void FindCurrentIteration(System.Xml.XmlNode node, string projectName, List<object> result)
		{


			if (node != null)
			{
				string iterationPath = node.Attributes["Path"].Value;
				if (!string.IsNullOrEmpty(iterationPath))
				{
					// Attempt to read the start and end dates if they exist.
					string strStartDate = (node.Attributes["StartDate"] != null) ? node.Attributes["StartDate"].Value : null;
					string strEndDate = (node.Attributes["FinishDate"] != null) ? node.Attributes["FinishDate"].Value : null;

					DateTime startDate = DateTime.Now;
					DateTime endDate = DateTime.Now;

					if (!string.IsNullOrEmpty(strStartDate) && !string.IsNullOrEmpty(strEndDate))
					{
						bool datesValid = true;

						// Both dates should be valid.
						datesValid = DateTime.TryParse(strStartDate, out startDate) && DateTime.TryParse(strEndDate, out endDate);
						if (datesValid && startDate <= DateTime.Now && endDate >= DateTime.Now)
						{
							result.Add(new
							{
								Project = projectName,
								Path = iterationPath.Replace(string.Concat("\\", projectName, "\\Iteration"), projectName),
								StartDate = startDate,
								EndDate = endDate
							});
						}
						else
						{
							// Visit any child nodes (sub-iterations).
							if (node.FirstChild != null)
							{
								// The first child node is the <Children> tag, which we'll skip.
								for (int nChild = 0; nChild < node.ChildNodes[0].ChildNodes.Count; nChild++)
									FindCurrentIteration(node.ChildNodes[0].ChildNodes[nChild], projectName, result);
							}
						}

					}


				}

				
			}
		}

		/// <summary>
		/// Retrieves all work items for the current sprint in all projects
		/// </summary>
		public List<object> GetWorkItems()
		{

			var css = TeamProjectCollection.GetService<ICommonStructureService4>();
			var currentIterations = new List<object>();
			foreach (var project in workItemStore.Projects.Cast<Project>().Where(p => !ProjectRegexExists || ProjectRegex.Match(p.Name).Success))
			{
				FindCurrentIteration(css, project.Uri.ToString(), currentIterations);				
			}

			// Run a query.
			var result = new List<object>();
			WorkItemCollection queryResults = workItemStore.Query(
			   @"Select Id, Title, System.TeamProject, State, [Assigned To], IterationId
			     From WorkItems 
			     Where 
					[Work Item Type] in ('Bug' , 'Product Backlog Item') 
					and State <> 'Done' and State <> 'Closed' and State <> 'Removed'
					and System.TeamProject = 'WebPort'"
			   );

			//List<WorkItem> result = new List<WorkItem>();
			foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem workItem in queryResults)
			{
				result.Add(new WorkItem()
				{
					Title = workItem.Title,
					Project = workItem.Project.Name,
					WorkItemID = workItem.Id,
					State = workItem.State,
					Assignee = workItem.Fields["Assigned To"].Value.ToString()
				});
				
			}
			return result;
		}


		private static void AddIterationPath(Node node, List<object> result, string parentIterationName)
		{
			foreach (Node item in node.ChildNodes)
			{
				var path = parentIterationName + "\\" + item.Name;
				result.Add(path);
				if (item.HasChildNodes)
				{
					AddIterationPath(item, result, path);
				}
			}
		}

	}
}
