using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
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
                Component.For<IGuestBookDataProvider>().ImplementedBy(Type.GetType(ConfigurationManager.AppSettings["data-provider"])),
                Component.For<IExecutionContext>().ImplementedBy<ConsoleAppExecutionContext>(),
                Component.For<LoggersManager>().DependsOn(Dependency.OnAppSettingsValue("level", "logging-level"))
            );
        }
    }
}
