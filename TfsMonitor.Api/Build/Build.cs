using System;

namespace TfsMonitor.Api.Build
{
	public class Build : ICloneable
	{

		private DateTime? _FinishedTime;
		private string _Status;

		public Uri BuildUri { get; set; }
		public Uri BuildDefinitionUri { get; set; }
		public string Project { get; set; }
		public string Definition { get; set; }

		public bool Completed { get; set; }
		/// <summary>
		/// GET will be null if Completed is false
		/// </summary>				
		public DateTime? FinishedTime
		{
			get { return (Completed ? _FinishedTime : null); }
			set { _FinishedTime = value; }
		}
		public DateTime StartTime { get; set; }
		/// <summary>
		/// For builds still running, reflects the current runtime when accessed. Clients should ideally perform their own calculations against StartTime
		/// </summary>
		public Nullable<TimeSpan> RunTime
		{
			get
			{
				return (Completed ? (Nullable<TimeSpan>)(_FinishedTime - StartTime) : null);
			}
		}

		public string Status
		{
			get { return _Status; }
			set
			{
				string status = null;
				switch (value)
				{
					case "Succeeded":
						status = "Succeeded";
						break;
					case "InProgress":
						status = "In Progress";
						break;
					case "Failed":
						status = "Failed";
						break;
					case "NotStarted":
						status = "Queued";
						break;
					default:
						status = value;
						break;
				}
				_Status = status;
			}
		}
		/// <summary>
		/// Reflects the Status field
		/// </summary>
		public bool Succeeded { get { return Status == "Succeeded"; } }
		/// <summary>
		/// Reflects the Status field
		/// </summary>
		public bool Failed { get { return Status == "Failed"; } }
		/// <summary>
		/// Reflects the Status field
		/// </summary>
		public bool InProgress { get { return Status == "In Progress"; } }

		public string Username { get; set; }

		public bool CIBuild { get; set; }
		public bool ScheduledBuild { get; set; }
		public bool ManualBuild { get; set; }

		public enum BuildTrigger { CIBuild, ScheduledBuild, ManualBuild }

		/// <summary>
		/// Overridden for debugging convenience
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Definition + " - " + Status;
		}

		/// <summary>
		/// If all CanRead properties on this Build equal the corresponding properties on the comparison object, 
		/// they are considered equal. The properties' implemented Equals method will be used.
		/// </summary>
		/// <param name="comparison"></param>
		/// <returns></returns>
		public override bool Equals(object comparison)
		{
			var properties = this.GetType().GetProperties();
			foreach (var propertyInfo in properties)
			{
				if (propertyInfo.CanRead)
				{
					if (!object.Equals(propertyInfo.GetValue(this), propertyInfo.GetValue(comparison)))
					{
						return false;
					}
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Returns copy of the Build object, only the CanWrite properties are set. It's not a deep copy - the reference types retain references.			
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			Build cloned = new Build();
			var properties = this.GetType().GetProperties();
			foreach (var propertyInfo in properties)
			{
				if (propertyInfo.CanWrite)
				{
					propertyInfo.SetValue(cloned, propertyInfo.GetValue(this));
				}
			}
			return cloned;
		}

	}

}
