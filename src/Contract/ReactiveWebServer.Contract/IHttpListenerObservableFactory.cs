using System;
using Castle.Core.Logging;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    public interface IHttpListenerObservableFactory
    {
        IHttpListenerObservable Create(Action<IHttpListenerConfigurator> configuratorCallback, ILogger logger);
    }
}
