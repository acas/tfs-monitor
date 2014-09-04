using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace TfsMonitor
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			
			// Use camel case for JSON data.
			config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			//the default json data format is MicrosoftDateFormat, which is STOOOPIDDDDDD
			config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
			//remove the xml formatter, so that loading the data in status browser by default will favor json. This is just status convenience, if we ever need xml endpoints, we'll add it back
			config.Formatters.Remove(config.Formatters.XmlFormatter);
			// Web API routes
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
			//config.Services.Replace(typeof(System.Web.Http.Dispatcher.IHttpControllerActivator), new TfsMonitor.Controllers.ServiceActivator(config));
			
		}
	}
}
