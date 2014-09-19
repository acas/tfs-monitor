using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsMonitor.Api.Work
{
	public class WorkItem
	{
		public int WorkItemID;
		public string Type;
		public string Project;
		public string Title;
		public string Assignee;
		public string State;
	}
}
