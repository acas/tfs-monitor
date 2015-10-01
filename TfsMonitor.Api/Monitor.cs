using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace TfsMonitor.Api
{
	/// <summary>
	/// Abstract class defining access to the TFS model
	/// TODO should this perhaps not be abstract? We could create it once and use DI to get it into the individual monitors
	/// </summary>
	public abstract class Monitor<T>
	{
		/// <summary>
		/// The TfsTeamProjectCollection object used to access the TFS server
		/// </summary>
		protected TfsTeamProjectCollection TeamProjectCollection;
		private string projectCollectionUrl;

		private Thread thread;

		public int MonitorRefreshIntervalMS = 10000;

		protected abstract void MonitorAction();

		private bool running = false;

		private List<Listener<T>> listeners = new List<Listener<T>>();

		/// <summary>
		/// Regular Expression determining which TFS Team Projects are considered by the monitors.
		/// </summary>
		protected System.Text.RegularExpressions.Regex ProjectRegex;
		/// <summary>
		/// Gets whether or not a ProjectRegex is specified. If it's not, all projects are used.
		/// </summary>
		protected bool ProjectRegexExists;

		public Monitor() {
			thread = new Thread(new ThreadStart(NoAction));

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

		private void NoAction() {
		
		}

		private bool Start() {
			lock(thread) {
				if(!running) {
					running = true;
					thread = new Thread(new ThreadStart(Run));
					thread.Start();
					return true;
				}
			}
			return false;
		}

		private void Run() {
			while(running) {
				MonitorAction();
				Thread.Sleep(MonitorRefreshIntervalMS);
			}
		}

		public void Stop() {
			lock(thread) {
				if(running) {
					try {
						thread.Abort();
					}
					finally {
						thread = null;
						running = false;
					}
				}
			}
		}

		public void NotifyAll(T data) {
			Listener<T>[] l;
			lock(listeners) {
				l = listeners.ToArray();
			}
			Parallel.ForEach(l, (t) => {
				t.Target.Invoke(data);
			});
		}

		public void NotifyError(Exception ex) {
			Listener<T>[] l;
			lock(listeners) {
				l = listeners.ToArray();
			}
			Parallel.ForEach(l, (t) => {
				t.Error.Invoke(ex);
			});
		}

		public bool RegisterListener(Listener<T> listener) {
			lock(listeners) {
				listeners.Add(listener);
				return Start();
			}
		}

		public void UnregisterListener(Listener<T> listener) {
			lock(listeners) {
				listeners.Remove(listener);
				if(ListenerCount == 0) {
					Stop();
				}
			}
		}

		public int ListenerCount {
			get {
				int result;
				lock(listeners) {
					result = listeners.Count;
				}
				return result;
			}
		}
	}

	public class Listener<T> {
		public Action<T> Target;
		public Action<Exception> Error;

		public Listener(Action<T> target, Action<Exception> error) {
			Target = target;
			Error = error;
		}

		public override bool Equals(object obj) {
			return Target.Equals(((Listener<T>)obj).Target) && Error.Equals(((Listener<T>)obj).Error);
		}
	}
}
