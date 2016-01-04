using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Core.Logging;

namespace AnywayAnyday.ReactiveWebServer.ConsoleHost
{
    class Program
    {
        const int GenericException = -100;

        static ILogger _logger;

        static void Main(string[] args)
        {            
            try
            {
                ExecuteCompositionRoot(args);
            }
            catch (TaskCanceledException ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            catch (AggregateException ex)
            {
                ex.Handle(e =>
                {
                    if (e is TaskCanceledException)
                        System.Console.WriteLine(e.Message);
                    else
                        System.Console.WriteLine(ex);
                    return true;
                });
            }
            catch (Exception ex)
            {
                Environment.ExitCode = GenericException;
                System.Console.WriteLine(ex);
            }
        }

        static void ExecuteCompositionRoot(string[] args)
        {
            using (var container = ConfigureIoC())
            {
                _logger = container.Resolve<LoggersManager>().AppLogger;
                _logger.Info("IoC successflly configured");

                var execContext = container.Resolve<IExecutionContext>();
                execContext.StartReading(); //starting keyboard control
                _logger.Info("Execution context started");

                try
                {
                }
                finally
                {
                    _logger.Info("Final releasing");
                    container.Release(execContext);                 
                }
            }
        }

        static IWindsorContainer ConfigureIoC()
        {
            var container = new WindsorContainer();            
            container.Install(Configuration.FromAppConfig(), FromAssembly.InThisApplication());
            return container;
        }
    }
}