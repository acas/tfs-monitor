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
	public abstract class MonitorHub : Hub
	{
		protected TfsMonitor.Api.Monitor monitor;
		protected static int connections = 0;
		private Thread thread;

		public MonitorHub()
		{
			thread = new Thread(() => Start(monitor));
		}

		public override Task OnConnected()
		{
			connections++;
			if (connections == 1)
			{
				thread.Start();
			}
			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			connections--;
			return base.OnDisconnected(stopCalled);
		}

		public abstract void Start(TfsMonitor.Api.Monitor monitor);

		public abstract void Broadcast(object data);

		public void NotifyError(Exception ex)
		{
			Clients.All.notifyError(ex);
		}
	}


}