using System.Net;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    public enum HandlerPriorityClass
    {
        Normal = 1,
        Fallback = 2,
        High = 0
    }
    public interface IHttpRequestHandler
    {
        string DisplayName { get; }
        HandlerPriorityClass PriorityClass { get; }
        int Priority { get; }
        bool HandleRequest(HttpListenerContext context);
    }    
}
