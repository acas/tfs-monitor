using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;


namespace TfsMonitor.Api.Build
{
	public class BuildMonitor
	{		
		
		private List<Build> BuildStatuses;
		
		private System.Text.RegularExpressions.Regex ProjectRegex;
		private bool ProjectRegexExists;

		private System.Text.RegularExpressions.Regex DefinitionRegex;		
		private bool DefinitionRegexExists;

		private IBuildServer BuildServer;
		private TfsTeamProjectCollection TeamProjectCollection;
		public BuildMonitor()
		{
			BuildStatuses = new List<Build>();

			ProjectRegex = new System.Text.RegularExpressions.Regex("");
			string projectRegexExpression = System.Configuration.ConfigurationManager.AppSettings["buildDefinitionProjectRegex"];
			ProjectRegexExists = projectRegexExpression != null;
			if (ProjectRegexExists)
			{
				ProjectRegex = new System.Text.RegularExpressions.Regex(projectRegexExpression);
			}

			DefinitionRegex = new System.Text.RegularExpressions.Regex("");
			string definitionRegexExpression = System.Configuration.ConfigurationManager.AppSettings["buildDefinitionRegex"];
			DefinitionRegexExists = definitionRegexExpression != null;
			if (DefinitionRegexExists)
			{
				DefinitionRegex = new System.Text.RegularExpressions.Regex(definitionRegexExpression);
			}

			string projectCollectionUrl = System.Configuration.ConfigurationManager.AppSettings["projectCollectionUrl"];
			TeamProjectCollection = new TfsTeamProjectCollection(new Uri(projectCollectionUrl));						
			BuildServer = (IBuildServer)TeamProjectCollection.GetService(typeof(IBuildServer));
			
		}

		private void UpdateBuildStatus()
		{
			
			TeamProject[] teamProjects = TeamProjectCollection.GetService<VersionControlServer>().GetAllTeamProjects(true);
			
			List<Uri> newUris = new List<Uri>();
					
			foreach (TeamProject proj in teamProjects.Where(p => !ProjectRegexExists || ProjectRegex.Match(p.Name).Success))
			{
				var defs = BuildServer.QueryBuildDefinitions(proj.Name);
				foreach (IBuildDefinition def in defs.Where(d => !DefinitionRegexExists  || DefinitionRegex.Match(d.Name).Success))
				{
					IBuildDetailSpec spec = BuildServer.CreateBuildDetailSpec(proj.Name, def.Name);
					spec.QueryOptions = QueryOptions.None;
					spec.QueryDeletedOption = QueryDeletedOption.ExcludeDeleted;
					spec.InformationTypes = new string[] { };
					spec.MaxBuildsPerDefinition = 1;
					spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;

					var builds = BuildServer.QueryBuilds(spec);
					if (builds.Builds.Length > 0)
					{
						var buildDetail = builds.Builds[0];
						if (!BuildStatuses.Exists(x => x.Completed && x.BuildUri == buildDetail.Uri))
						{
							newUris.Add(buildDetail.Uri);
						}
					}
				}

			}
			if (newUris.Count > 0)
			{
				var builds = BuildServer.QueryBuildsByUri(newUris.ToArray(), new string[] { }, QueryOptions.All);

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
			return result;
		}


	}
}
