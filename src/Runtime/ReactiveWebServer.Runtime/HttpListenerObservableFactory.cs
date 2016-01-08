using System;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.ReactiveWebServer.Runtime
{
    /// <summary>
    /// Provides an abstract factory for HttpListener observable
    /// </summary>
    public sealed class HttpListenerObservableFactory: IHttpListenerObservableFactory
    {
        public IHttpListenerObservable Create(Action<IHttpListenerConfigurator> configuratorCallback, ILogger logger)
        {
            return new HttpListenerObservable(configuratorCallback, logger);
        }
    }
}
