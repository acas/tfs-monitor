using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;


namespace TfsMonitor.Api.Build
{
	public class BuildMonitor : Monitor<List<Build>>
	{		
		private List<Build> buildStatuses;
		private List<Build> lastCheck = new List<Build>();

		private System.Text.RegularExpressions.Regex DefinitionRegex;		
		private bool DefinitionRegexExists;

		private IBuildServer BuildServer;

		public BuildMonitor() : base()
		{
			buildStatuses = new List<Build>();

			
			DefinitionRegex = new System.Text.RegularExpressions.Regex("");
			string definitionRegexExpression = System.Configuration.ConfigurationManager.AppSettings["buildDefinitionRegex"];
			DefinitionRegexExists = definitionRegexExpression != null;
			if (DefinitionRegexExists) {
				DefinitionRegex = new System.Text.RegularExpressions.Regex(definitionRegexExpression);
			}
			
			BuildServer = (IBuildServer)TeamProjectCollection.GetService(typeof(IBuildServer));

			int configMonitorRefreshIntervalMS;
			if(int.TryParse(ConfigurationManager.AppSettings["BuildMonitorRefreshIntervalMS"], out configMonitorRefreshIntervalMS)) {
				MonitorRefreshIntervalMS = configMonitorRefreshIntervalMS;
			}
		}

		protected override void MonitorAction() {
			try {
				List<Build> current = GetBuildStatuses();
				if(!Enumerable.SequenceEqual(lastCheck, current)) {
					lastCheck = current;
					NotifyAll(current);
				}
			}
			catch (Exception ex)
			{
				NotifyError(ex);
				lastCheck = new List<Build>(); //reset the lastCheck so that next time around, it'll try to broadcast if no errors are shown. This will clear the error 
				//message on the client
			}
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
						if (!buildStatuses.Exists(x => x.Completed && x.BuildUri == buildDetail.Uri))
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
					buildStatuses.Remove(buildStatuses.Where(x => x.BuildDefinitionUri == buildDetail.BuildDefinitionUri).FirstOrDefault());
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
					buildStatuses.Add(build);
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
			result.AddRange(buildStatuses.Select(x => (Build)x.Clone()));		
			//result must come back in the same order every time so that the client can compare previous to current easily				
			return result.OrderBy(x => x.BuildDefinitionUri.ToString()).ToList();
		}

		public List<Build> GetLast() {
			return lastCheck;
		}
	}
}
