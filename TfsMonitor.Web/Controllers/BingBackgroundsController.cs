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
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US");
			WebResponse response = request.GetResponse();
			StreamReader sr = new StreamReader(response.GetResponseStream());
			JObject data = JObject.Parse(sr.ReadToEnd());

			request = (HttpWebRequest)WebRequest.Create("http://www.bing.com");
			request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.94 Safari/537.36";
			response = request.GetResponse();
			sr = new StreamReader(response.GetResponseStream());
			string bingHtml = sr.ReadToEnd();

			dynamic result = new JObject();
			result.imageUrl = data.Value<JArray>("images")[0].Value<string>("url");
			result.copyright = data.Value<JArray>("images")[0].Value<string>("copyright");

			int videoSourceStart = bingHtml.IndexOf("g_vid =", StringComparison.InvariantCultureIgnoreCase);
			if(videoSourceStart > 0) {
				videoSourceStart += 7;
				result.video = JArray.Parse(bingHtml.Substring(videoSourceStart, bingHtml.IndexOf("]];", videoSourceStart) - videoSourceStart + 2));
			}

			return result;
		}
	}
}
