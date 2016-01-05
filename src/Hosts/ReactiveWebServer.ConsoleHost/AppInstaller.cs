using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace AnywayAnyday.ReactiveWebServer.ConsoleHost
{
    public sealed class AppInstaller : IWindsorInstaller
    {        
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IGuestBookDataProvider>().ImplementedBy(Type.GetType(ConfigurationManager.AppSettings["data-provider"]))                                
                    .DependsOn( Dependency.OnAppSettingsValue("filePath", "storage-file"))
                    .DependsOn(Dependency.OnValue("connectionString", ConfigurationManager.ConnectionStrings["DefaultSqlStorage"].ConnectionString))
                    .DependsOn(Dependency.OnComponent(typeof(ILogger), LoggersManager.DalLoggerInst)),

                Component.For<IExecutionContext>().ImplementedBy<ConsoleAppExecutionContext>(),

                Component.For<LoggersManager>().DependsOn(Dependency.OnAppSettingsValue("level", "logging-level")),

                Component.For<ILogger>().UsingFactoryMethod((IKernel k) => k.Resolve<LoggersManager>().AppLogger).Named(LoggersManager.AppLoggerInst),
                Component.For<ILogger>().UsingFactoryMethod((IKernel k) => k.Resolve<LoggersManager>().WebServerLogger).Named(LoggersManager.WebServerLoggerInst),
                Component.For<ILogger>().UsingFactoryMethod((IKernel k) => k.Resolve<LoggersManager>().DalLogger).Named(LoggersManager.DalLoggerInst)
            );
        }
    }
}
