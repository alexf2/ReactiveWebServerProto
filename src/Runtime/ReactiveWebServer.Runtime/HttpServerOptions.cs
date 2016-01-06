using System.Collections.Generic;
using AnywayAnyday.ReactiveWebServer.Contract;

namespace AnywayAnyday.ReactiveWebServer.Runtime
{
    public class HttpServerOptions : IHttpServerOptions
    {
        public IList<Endpoint> Endpoints { get; set; }        
    }
}
