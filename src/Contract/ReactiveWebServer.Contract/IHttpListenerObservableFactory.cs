using System;
using Castle.Core.Logging;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    /// <summary>
    /// Http listener abstract factory contract.
    /// </summary>
    public interface IHttpListenerObservableFactory
    {
        IHttpListenerObservable Create(Action<IHttpListenerConfigurator> configuratorCallback, ILogger logger);
    }
}
