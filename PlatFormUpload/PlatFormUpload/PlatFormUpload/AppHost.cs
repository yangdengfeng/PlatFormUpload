using Funq;
using ServiceStack;
using PlatFormUpload.ServiceInterface;
using ServiceStack.Api.Swagger;
using System;

namespace PlatFormUpload
{
    //VS.NET Template Info: https://servicestack.net/vs-templates/EmptyAspNet
    public class AppHost : AppHostBase
    {
        /// <summary>
        /// Base constructor requires a Name and Assembly where web service implementation is located
        /// </summary>
        public AppHost()
            : base("PlatFormUpload", typeof(SyncPKPMJCDataService).Assembly) { }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        public override void Configure(Container container)
        {
            //Config examples
            this.Plugins.Add(new PostmanFeature());
            this.Plugins.Add(new CorsFeature());
            Plugins.Add(new SwaggerFeature());
            Plugins.Add(new SessionFeature());
            Plugins.Add(new RequestLogsFeature
            {
                RequestLogger = new CsvRequestLogger(appendEvery: TimeSpan.FromSeconds(3))
            });
        }
    }
}