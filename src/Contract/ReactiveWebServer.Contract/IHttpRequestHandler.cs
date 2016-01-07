using System.Net;
using System.Threading.Tasks;

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

        Task<bool> HandleRequest(HttpListenerContext context);
    }    
}
