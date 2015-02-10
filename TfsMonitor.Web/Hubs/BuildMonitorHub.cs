using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using TfsMonitor.Api.Build;
using System.Linq;

namespace TfsMonitor.Web.Hubs
{
	public class BuildMonitorHub : MonitorHub
	{
		
		public BuildMonitorHub() : base ()
		{
			monitor = new BuildMonitor();			
		}

		public override Task OnConnected()
		{
			//if the thread is already running, broadcast for the new client
			if (connections != 0)
			{				
				Broadcast(((BuildMonitor)monitor).GetBuildStatuses());
			}
			return base.OnConnected();
		}

	

		public override void Start(TfsMonitor.Api.Monitor monitor)
		{
			List<Build> lastCheck = new List<Build>();
			while (connections > 0)
			{
				try
				{
					List<Build> current = ((BuildMonitor)monitor).GetBuildStatuses();
					if (!Enumerable.SequenceEqual(lastCheck, current)) //it'll always fire the first time when the thread is started
					{
						Broadcast(current);
						lastCheck = current;
					}
					Thread.Sleep(2000);			
				}
				catch (Exception ex)
				{
					NotifyError(ex);
					lastCheck = new List<Build>(); //reset the lastCheck so that next time around, it'll try to broadcast if no errors are shown. This will clear the error 
					//message on the client
				}
			}
		}

		public override void Broadcast(object data)
		{
			Clients.All.loadData(data);
		}
	
	}


}