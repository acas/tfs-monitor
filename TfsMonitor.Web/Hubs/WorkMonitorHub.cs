using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using TfsMonitor.Api.Work;
using System.Linq;

namespace TfsMonitor.Web.Hubs
{
	public class WorkMonitorHub : MonitorHub
	{
		public WorkMonitorHub()
			: base()
		{
			monitor = new WorkMonitor();
		}

		public override Task OnConnected()
		{
			//if the thread is already running, broadcast for the new client
			if (connections != 0)
			{
				Broadcast(((WorkMonitor)monitor).GetWorkItems());
			}
			return base.OnConnected();
		}

		public override void Start(TfsMonitor.Api.Monitor monitor)
		{
			List<WorkItem> lastCheck = new List<WorkItem>();
			while (connections > 0)
			{
				try
				{
					List<WorkItem> current = ((WorkMonitor)monitor).GetWorkItems();					
					if (!Enumerable.SequenceEqual(lastCheck, current)) //it'll always fire the first time when the thread is started
					{
						Broadcast(current);
						lastCheck = current;
					}
					//interval is large because the query is slow. Speed up the query and then tighten the interval, perhaps. 
					//in any case it's ok for the work items to be a few seconds behind, because they don't change as frequently as 
					//builds do and timing is not as critical
					Thread.Sleep(10 * 1000); 
				}
				catch (Exception ex)
				{
					NotifyError(ex);
					lastCheck = new List<WorkItem>(); //reset the lastCheck so that next time around, it'll try to broadcast if no errors are shown. This will clear the error 
					//message on the client
				}
			}
		}

		public override void Broadcast(object data)
		{
			Clients.All.sendData(data);
		}
	}


}