using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    public interface IHttpServerOptions
    {        
        IList<Endpoint> Endpoints { get; }
    }
}
