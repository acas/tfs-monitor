using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TfsMonitor.Api.Work
{
	public class WorkItem: ICloneable
	{
		public int WorkItemID { get; set; }
		public string Type {get; set;}
		public string Project { get; set; }
		public string Title { get; set; }
		public string Assignee { get; set; }
		public string State { get; set; }
		public double WorkRemaining { get; set; }


		/// <summary>
		/// If all CanRead properties on this WorkItem equal the corresponding properties on the comparison object, 
		/// they are considered equal. The properties' implemented Equals method will be used.
		/// </summary>
		/// <param name="comparison"></param>
		/// <returns></returns>
		public override bool Equals(object comparison)
		{
			PropertyInfo[] properties = this.GetType().GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
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
			WorkItem cloned = new WorkItem();
			PropertyInfo[] properties = this.GetType().GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
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
