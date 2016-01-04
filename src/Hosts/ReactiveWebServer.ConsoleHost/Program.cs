using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnywayAnyday.DataProviders.GuestBookXmlProvider;
using AnywayAnyday.GuestBook.Contract;
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
            using (var container = ConfigureIoC())
            {
                _logger = container.Resolve<LoggersManager>().AppLogger;

                try
                {
                    ExecuteCompositionRoot(args, container);
                }
                catch (TaskCanceledException ex)
                {                    
                    _logger.Info(ex.Message);
                }
                catch (AggregateException ex)
                {
                    ex.Handle(e =>
                    {
                        if (e is TaskCanceledException)
                            _logger.Info(e.Message);
                        else
                            _logger.Error("At application Main (agg):", e);
                        return true;
                    });
                }
                catch (Exception ex)
                {
                    Environment.ExitCode = GenericException;
                    _logger.Error("At application Main:", ex);
                }
            }

            Console.WriteLine("Press a key>");
            Console.ReadLine();     
        }

        static void ExecuteCompositionRoot (string[] args, IWindsorContainer container)
        {            
            _logger.Info("IoC successflly configured");

            var execContext = container.Resolve<IExecutionContext>();
            execContext.StartReading(); //starting keyboard control
            _logger.Info("Execution context started");

            try
            {
                var stg = container.Resolve<IGuestBookDataProvider>();

                Task.WaitAll(new Task[]
                {
                    stg.AddUser("user1", "Aleksey Fedorov"),
                    stg.AddUser("user2", "Pavel Potapov"),
                    stg.AddMessage("user2", "Test msg 1"),
                    stg.AddMessage("user2", "Test msg 2"),
                    stg.AddMessage("user2", "Test msg 3"),
                    stg.AddMessage("user1", "Test msg 11"),
                    stg.AddMessage("user1", "Test msg 21")
                });                
            }
            finally
            {
                _logger.Info("Final releasing");
                container.Release(execContext);                 
            }         
        }

        static IWindsorContainer ConfigureIoC()
        {
            var container = new WindsorContainer();            
            container.Install(Configuration.FromAppConfig(), FromAssembly.This(), FromAssembly.Containing<GuestBookXmlProvider>());
            return container;
        }
    }
}