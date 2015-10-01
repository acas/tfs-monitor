using System;
using System.Configuration;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using TfsMonitor.Api.Build;
using System.Linq;

namespace TfsMonitor.Web.Hubs
{
	public class BuildMonitorHub : MonitorHub<List<Build>>
	{		
		protected static BuildMonitor Monitor = new BuildMonitor();

		private TfsMonitor.Api.Listener<List<Build>> listener = null;

		public override Task OnConnected() {
			//if the thread is already running, broadcast for the new client
			listener = new TfsMonitor.Api.Listener<List<Build>>(Broadcast, NotifyError);
			Monitor.RegisterListener(listener);
			if(Monitor.ListenerCount > 1) {
				Broadcast(Monitor.GetLast());
			}
			return base.OnConnected();
		}
		public override Task OnDisconnected(bool stopCalled) {
			Monitor.UnregisterListener(listener);
			return base.OnDisconnected(stopCalled);
		}

		public override void Broadcast(List<Build> data) {
			Clients.All.loadData(data);
		}
	}
}