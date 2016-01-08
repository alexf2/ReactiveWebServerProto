using System.Collections.Generic;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    /// <summary>
    /// Server settings.
    /// </summary>
    public interface IHttpServerOptions
    {        
        IList<Endpoint> Endpoints { get; }
    }
}
