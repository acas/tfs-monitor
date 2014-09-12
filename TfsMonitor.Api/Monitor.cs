using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;


namespace TfsMonitor.Api
{
	public class Monitor
	{

		protected TfsTeamProjectCollection TeamProjectCollection;
		private string projectCollectionUrl = System.Configuration.ConfigurationManager.AppSettings["projectCollectionUrl"];

		protected System.Text.RegularExpressions.Regex ProjectRegex;
		protected bool ProjectRegexExists;

		public Monitor()
		{


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
