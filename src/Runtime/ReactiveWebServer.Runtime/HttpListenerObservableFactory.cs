using System;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.ReactiveWebServer.Runtime
{
    public sealed class HttpListenerObservableFactory: IHttpListenerObservableFactory
    {
        public IHttpListenerObservable Create(Action<IHttpListenerConfigurator> configuratorCallback, ILogger logger)
        {
            return new HttpListenerObservable(configuratorCallback, logger);
        }
    }
}
