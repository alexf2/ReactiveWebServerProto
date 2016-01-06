using System;
using System.Net;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    public interface IHttpListenerObservable: IObservable<HttpListenerContext>, IDisposableService
    {        
    }
}
