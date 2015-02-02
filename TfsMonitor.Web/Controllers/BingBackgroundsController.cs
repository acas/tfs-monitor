using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace TfsMonitor.Controllers
{
    public class BingBackgroundsController : ApiController
    {
		public JObject Get() {
			WebRequest request = WebRequest.Create("http://www.bing.com/HPImageArchive.aspx?format=js&idx=1&n=1&mkt=en-US");
			WebResponse response = request.GetResponse();
			StreamReader sr = new StreamReader(response.GetResponseStream());
			return JObject.Parse(sr.ReadToEnd());
		}
	}
}
