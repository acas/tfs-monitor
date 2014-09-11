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
		private List<Build> BuildStatuses = new List<Build>();
		private void UpdateBuildStatus()
		{
			string projectCollectionUrl = System.Configuration.ConfigurationManager.AppSettings["projectCollectionUrl"].ToString();			
			TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(projectCollectionUrl));						
			TeamProject[] teamProjects = tfs.GetService<VersionControlServer>().GetAllTeamProjects(true);
			IBuildServer buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));

			//todo perhaps we can use this and def.LastBuildUri together to speed up the process?
			//def.LastBuildUri only contains completed builds, but is much faster than the query
			//List<Uri> runningBuildDefinitions = new List<Uri>();
			//IQueuedBuildSpec qbSpec = buildServer.CreateBuildQueueSpec("*", "*");
			//var queuedBuilds = buildServer.QueryQueuedBuilds(qbSpec);
			//foreach (var build in queuedBuilds.QueuedBuilds)
			//{
			//	runningBuildDefinitions.Add(build.BuildDefinitionUri);
			//	Build b = new Build()
			//	{

			//	};
			//}

			List<Uri> newUris = new List<Uri>();
			foreach (TeamProject proj in teamProjects)
			{
				var defs = buildServer.QueryBuildDefinitions(proj.Name);
				foreach (IBuildDefinition def in defs)
				{
					IBuildDetailSpec spec = buildServer.CreateBuildDetailSpec(proj.Name, def.Name);
					spec.QueryOptions = QueryOptions.None;
					spec.QueryDeletedOption = QueryDeletedOption.ExcludeDeleted;
					spec.InformationTypes = new string[] { };
					spec.MaxBuildsPerDefinition = 1;
					spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;

					var builds = buildServer.QueryBuilds(spec);
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
				var builds = buildServer.QueryBuildsByUri(newUris.ToArray(), new string[] { }, QueryOptions.All);

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
