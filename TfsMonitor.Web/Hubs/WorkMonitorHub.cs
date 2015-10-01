using System;
using System.Configuration;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using TfsMonitor.Api.Work;
using System.Linq;

namespace TfsMonitor.Web.Hubs
{
	public class WorkMonitorHub : MonitorHub<List<WorkItem>>
	{
		protected static WorkMonitor Monitor = new WorkMonitor();

		private TfsMonitor.Api.Listener<List<WorkItem>> listener = null;

		public override Task OnConnected() {
			//if the thread is already running, broadcast for the new client
			listener = new TfsMonitor.Api.Listener<List<WorkItem>>(Broadcast, NotifyError);
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

		public override void Broadcast(List<WorkItem> data) {
			Clients.Client(Context.ConnectionId).loadData(data);
		}
	}
}