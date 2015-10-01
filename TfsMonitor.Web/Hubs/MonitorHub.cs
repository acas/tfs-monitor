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
	public abstract class MonitorHub<T> : Hub
	{
		public override Task OnConnected() {
			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled) {
			return base.OnDisconnected(stopCalled);
		}

		public abstract void Broadcast(T data);

		public void NotifyError(Exception ex) {
			Clients.All.notifyError(ex);
		}
	}


}