using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using TfsMonitor.Api.Build;
using System.Linq;
namespace SignalRChat
{
	public class BuildMonitorHub : Hub
	{
		private BuildMonitor buildMonitor = new BuildMonitor();
		private static int connections = 0;
		private Thread thread;


		public BuildMonitorHub()
		{
			thread = new Thread(() => Start(buildMonitor));
		}



		public override Task OnConnected()
		{
			connections++;
			if (connections == 1)
			{
				thread.Start();
			}
			else
			{	//if the thread is already running, broadcast for the new client
				Broadcast(buildMonitor.GetBuildStatuses());
			}



			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			connections--;
			return base.OnDisconnected(stopCalled);
		}

		public void Start(BuildMonitor monitor)
		{
			List<Build> lastCheck = new List<Build>();
			while (connections > 0)
			{
				try
				{					
					List<Build> current = buildMonitor.GetBuildStatuses();
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

		public void Broadcast(List<Build> data)
		{
			Clients.All.sendData(data);
		}

		public void NotifyError(Exception ex)
		{
			Clients.All.notifyError(ex);
		}
	}


}