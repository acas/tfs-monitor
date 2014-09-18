using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;


namespace TfsMonitor.Api.Build
{
	public class BuildMonitor:Monitor
	{		
		
		private List<Build> BuildStatuses;
		
		private System.Text.RegularExpressions.Regex DefinitionRegex;		
		private bool DefinitionRegexExists;

		private IBuildServer BuildServer;

		public BuildMonitor()
		{
			BuildStatuses = new List<Build>();

			
			DefinitionRegex = new System.Text.RegularExpressions.Regex("");
			string definitionRegexExpression = System.Configuration.ConfigurationManager.AppSettings["buildDefinitionRegex"];
			DefinitionRegexExists = definitionRegexExpression != null;
			if (DefinitionRegexExists)
			{
				DefinitionRegex = new System.Text.RegularExpressions.Regex(definitionRegexExpression);
			}
			
			BuildServer = (IBuildServer)TeamProjectCollection.GetService(typeof(IBuildServer));
			
		}

		private void UpdateBuildStatus()
		{
			
			TeamProject[] teamProjects = TeamProjectCollection.GetService<VersionControlServer>().GetAllTeamProjects(true);
			
			List<Uri> newUris = new List<Uri>();

			foreach (TeamProject project in teamProjects.Where(p => !ProjectRegexExists || ProjectRegex.Match(p.Name).Success))
			{
				IBuildDefinition[] definitions = BuildServer.QueryBuildDefinitions(project.Name);
				foreach (IBuildDefinition definition in definitions.Where(d => !DefinitionRegexExists || DefinitionRegex.Match(d.Name).Success))
				{
					IBuildDetailSpec spec = BuildServer.CreateBuildDetailSpec(project.Name, definition.Name);
					spec.QueryOptions = QueryOptions.None;
					spec.QueryDeletedOption = QueryDeletedOption.ExcludeDeleted;
					spec.InformationTypes = new string[] { };
					spec.MaxBuildsPerDefinition = 1;
					spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;

					IBuildQueryResult builds = BuildServer.QueryBuilds(spec);
					if (builds.Builds.Length > 0)
					{
						IBuildDetail buildDetail = builds.Builds[0];
						if (!BuildStatuses.Exists(x => x.Completed && x.BuildUri == buildDetail.Uri))
						{
							newUris.Add(buildDetail.Uri);
						}
					}
				}

			}
			if (newUris.Count > 0)
			{
				IBuildDetail[] builds = BuildServer.QueryBuildsByUri(newUris.ToArray(), new string[] { }, QueryOptions.All);

				foreach (IBuildDetail buildDetail in builds)
				{
					BuildStatuses.Remove(BuildStatuses.Where(x => x.BuildDefinitionUri == buildDetail.BuildDefinitionUri).FirstOrDefault());
					Build build = new Build()
					{
						BuildUri = buildDetail.Uri,
						BuildDefinitionUri = buildDetail.BuildDefinitionUri,
						Definition = buildDetail.BuildDefinition.Name,
						Project = buildDetail.TeamProject.ToString(),
						Completed = buildDetail.BuildFinished,
						StartTime = buildDetail.StartTime,
						FinishedTime = buildDetail.FinishTime,
						Status = buildDetail.Status.ToString(),
						Username = buildDetail.RequestedBy

					};
					BuildStatuses.Add(build);
				}
			}			
		}

		/// <summary>
		/// Returns an updated copy of the list of builds (not status reference to the original - 
		/// this facilitates keeping the returned list around long enough to compare it to the list
		/// when it's returned some time in the future)
		/// </summary>
		/// <returns></returns>
		public List<Build> GetBuildStatuses()
		{
			UpdateBuildStatus();

			List<Build> result = new List<Build>();
			result.AddRange(BuildStatuses.Select(x => (Build)x.Clone()));		
			//result must come back in the same order every time so that the client can compare previous to current easily				
			return result.OrderBy(x => x.BuildDefinitionUri.ToString()).ToList();
		}


	}
}
