using System.Collections.Generic;
using AnywayAnyday.ReactiveWebServer.Contract;

namespace AnywayAnyday.ReactiveWebServer.Runtime
{
    /// <summary>
    /// Provides some settings for WebServer
    /// </summary>
    public class HttpServerOptions : IHttpServerOptions
    {
        public IList<Endpoint> Endpoints { get; set; }        
    }
}
