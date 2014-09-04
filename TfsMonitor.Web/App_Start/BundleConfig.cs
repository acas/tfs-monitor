using System.Web;
using System.Web.Optimization;

namespace TfsMonitor
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/js").IncludeDirectory("~/Views", "*.js", true));
			BundleTable.EnableOptimizations = true;
#if DEBUG
			BundleTable.EnableOptimizations = false;
#endif
		}
	}
}
