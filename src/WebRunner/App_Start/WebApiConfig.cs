using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace WebRunner
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            ConfigureJsonSerializerSettings(GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);

            var settings = new JsonSerializerSettings();
            ConfigureJsonSerializerSettings(settings);
            settings.ContractResolver = new SignalRContractResolver();
            var serializer = JsonSerializer.Create(settings);
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
        }

        private static void ConfigureJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
        }

        private class SignalRContractResolver : IContractResolver
        {

            private readonly System.Reflection.Assembly assembly;
            private readonly IContractResolver camelCaseContractResolver;
            private readonly IContractResolver defaultContractSerializer;

            public SignalRContractResolver()
            {
                defaultContractSerializer = new DefaultContractResolver();
                camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
                assembly = typeof(Microsoft.AspNet.SignalR.Infrastructure.Connection).Assembly;
            }

            public JsonContract ResolveContract(Type type)
            {
                if (type.Assembly.Equals(assembly))
                {
                    return defaultContractSerializer.ResolveContract(type);

                }

                return camelCaseContractResolver.ResolveContract(type);
            }

        }
    }
}
