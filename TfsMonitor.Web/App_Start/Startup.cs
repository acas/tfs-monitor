using Microsoft.Owin;
using Owin;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using Microsoft.AspNet.SignalR.Client;

[assembly: OwinStartup(typeof(SignalRChat.Startup))]
namespace SignalRChat
{
	public class SignalRContractResolver : IContractResolver
	{
		private readonly Assembly _assembly;
		private readonly IContractResolver _camelCaseContractResolver;
		private readonly IContractResolver _defaultContractSerializer;

		public SignalRContractResolver()
		{
			_defaultContractSerializer = new DefaultContractResolver();
			_camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
			_assembly = typeof(PersistentConnection).Assembly;
		}

		#region IContractResolver Members

		public JsonContract ResolveContract(Type type)
		{			
			if (type.Assembly.Equals(_assembly))
			{
				return _defaultContractSerializer.ResolveContract(type);
			}

			return _camelCaseContractResolver.ResolveContract(type);
		}

		#endregion
	}

	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			// Any connection or hub wire up and configuration should go here
			app.MapSignalR();

			var settings = new JsonSerializerSettings();
			settings.ContractResolver = new SignalRContractResolver();
			var serializer = JsonSerializer.Create(settings);
			GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
		}


	}
}