using System;
using System.Net;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    /// <summary>
    /// The contract to represent HttpListener as an observable service.
    /// </summary>
    public interface IHttpListenerObservable: IObservable<HttpListenerContext>, IDisposableService
    {        
    }
}
