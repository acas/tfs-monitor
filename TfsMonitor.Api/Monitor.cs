using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;


namespace TfsMonitor.Api
{
	/// <summary>
	/// Abstract class defining access to the TFS model
	/// TODO should this perhaps not be abstract? We could create it once and use DI to get it into the individual monitors
	/// </summary>
	public abstract class Monitor
	{
		/// <summary>
		/// The TfsTeamProjectCollection object used to access the TFS server
		/// </summary>
		protected TfsTeamProjectCollection TeamProjectCollection;
		private string projectCollectionUrl;

		/// <summary>
		/// Regular Expression determining which TFS Team Projects are considered by the monitors.
		/// </summary>
		protected System.Text.RegularExpressions.Regex ProjectRegex;
		/// <summary>
		/// Gets whether or not a ProjectRegex is specified. If it's not, all projects are used.
		/// </summary>
		protected bool ProjectRegexExists;

		public Monitor()
		{
			projectCollectionUrl = System.Configuration.ConfigurationManager.AppSettings["projectCollectionUrl"];

			ProjectRegex = new System.Text.RegularExpressions.Regex("");
			string projectRegexExpression = System.Configuration.ConfigurationManager.AppSettings["projectRegex"];
			ProjectRegexExists = projectRegexExpression != null;
			if (ProjectRegexExists)
			{
				ProjectRegex = new System.Text.RegularExpressions.Regex(projectRegexExpression);
			}


			TeamProjectCollection = new TfsTeamProjectCollection(new Uri(projectCollectionUrl));
		}
	}
}
