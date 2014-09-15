﻿using System;
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
		}

		private void FindCurrentIterations(ICommonStructureService4 css, string uri, List<Iteration> result)
		{

			NodeInfo[] structures = css.ListStructures(uri);
			NodeInfo iterations = structures.FirstOrDefault(n => n.StructureType.Equals("ProjectLifecycle"));

			System.Xml.XmlElement iterationsTree = css.GetNodesXml(new[] { iterations.Uri }, true);

			if (iterations != null)
			{
				string projectName = css.GetProject(uri).Name;
				FindCurrentIterations(iterationsTree.ChildNodes[0], projectName, result);
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
		private void FindCurrentIterations(System.Xml.XmlNode node, string projectName, List<Iteration> result)
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

					DateTime startDate = DateTime.Now;
					DateTime endDate = DateTime.Now;
					bool datesValid = true;

					// Both dates should be valid.
					datesValid = DateTime.TryParse(strStartDate, out startDate) && DateTime.TryParse(strEndDate, out endDate);

					if (!string.IsNullOrEmpty(strStartDate) && !string.IsNullOrEmpty(strEndDate)
						&& datesValid && startDate <= DateTime.Now && endDate >= DateTime.Now)
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
								FindCurrentIterations(node.ChildNodes[0].ChildNodes[child], projectName, result);
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
				FindCurrentIterations(css, project.Uri.ToString(), currentIterations);


				Dictionary<string, string> parameters = new Dictionary<string, string>();

				parameters.Add("Project", project.Name);
				string iterationPath = currentIterations.Where(i => i.Project == project.Name).SingleOrDefault().Path;
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
				}
			}

			return result;
		}

	}
}