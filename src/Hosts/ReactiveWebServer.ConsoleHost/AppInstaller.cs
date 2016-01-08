using System;
using System.Configuration;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.HttpRequestHandlers.Runtime;
using AnywayAnyday.ReactiveWebServer.Contract;
using AnywayAnyday.ReactiveWebServer.Runtime;
using Castle.Core.Logging;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace AnywayAnyday.ReactiveWebServer.ConsoleHost
{
    /// <summary>
    /// Castle Windsor installer. Used to configure type mappings of IoC at the composittion root.
    /// </summary>
    public sealed class AppInstaller : IWindsorInstaller
    {        
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IGuestBookDataProvider>().ImplementedBy(Type.GetType(ConfigurationManager.AppSettings["data-provider"]))                                
                    .DependsOn(Dependency.OnAppSettingsValue("filePath", "storage-file"))
                    .DependsOn(Dependency.OnValue("connectionString", ConfigurationManager.ConnectionStrings["DefaultSqlStorage"].ConnectionString))
                    .DynamicParameters((kernel, dic) =>
                    {
                        dic["logger"] = kernel.Resolve<LoggersManager>().DalLogger;
                    }),
                    
                Component.For<IExecutionContext>().ImplementedBy<ConsoleAppExecutionContext>(),

                Component.For<LoggersManager>().DependsOn(Dependency.OnAppSettingsValue("level", "logging-level")),

                Component.For<ILogger>().UsingFactoryMethod((IKernel k) => k.Resolve<LoggersManager>().AppLogger).Named(LoggersManager.AppLoggerInst),
                Component.For<ILogger>().UsingFactoryMethod((IKernel k) => k.Resolve<LoggersManager>().WebServerLogger).Named(LoggersManager.WebServerLoggerInst),
                Component.For<ILogger>().UsingFactoryMethod((IKernel k) => k.Resolve<LoggersManager>().DalLogger).Named(LoggersManager.DalLoggerInst),


                Component.For<IHttpListenerObservableFactory>().ImplementedBy<HttpListenerObservableFactory>(),

                Component.For<IHttpServerOptions>().Instance(new HttpServerOptions() { Endpoints = new [] { ConstructEndpoint() }}),

                Classes.FromAssemblyContaining<ServiceHostBase>()
                    .BasedOn<ServiceHostBase>()
                        .WithService.FirstInterface().Configure((r) => r.DynamicParameters((kernel, dic) =>
                        {
                            dic["logger"] = kernel.Resolve<LoggersManager>().WebServerLogger;
                        })),

                Classes.FromAssemblyContaining<HelloWorldHandler>()
                    .IncludeNonPublicTypes()
                    .BasedOn<IHttpRequestHandler>()                    
                    .WithService.FirstInterface().Configure((r) => r.DynamicParameters((kernel, dic) =>
                    {
                        dic["logger"] = kernel.Resolve<LoggersManager>().HandlerLogger;
                    })).LifestyleTransient()
            );
        }

        static Endpoint ConstructEndpoint()
        {
            var portRaw = ConfigurationManager.AppSettings["port"];
            int p;
            if (!int.TryParse(portRaw, out p))
                throw new ArgumentException($"Can't parse app.config appsettings Port: '{portRaw}'");

            var host = ConfigurationManager.AppSettings["host"]?.Trim()?.ToLower();
            switch (host)
            {
                case "*":
                    return Endpoint.AllHttpWeak(p);

                case "+":
                    return Endpoint.AllHttpStrong(p);

                case "localhost":
                    return Endpoint.HttpLocal(p);

                default:
                    return new Endpoint() {HostName = host, Port = p, Protocol = "http"};
            }
        }
    }
}
