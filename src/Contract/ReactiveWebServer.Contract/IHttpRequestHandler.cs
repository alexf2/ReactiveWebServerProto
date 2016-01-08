using System.Net;
using System.Threading.Tasks;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    /// <summary>
    /// Web request handlers' priorities. We arrange the handlers by class, then by integer priority level.
    /// Usually we put more specific handlers before others to let them a chance to capture and handle a suitable request.
    /// </summary>
    public enum HandlerPriorityClass
    {
        Normal = 1,
        Fallback = 2,
        High = 0
    }

    /// <summary>
    /// The Http request handler contract.
    /// </summary>
    public interface IHttpRequestHandler
    {
        string DisplayName { get; }
        HandlerPriorityClass PriorityClass { get; }
        int Priority { get; }

        Task<bool> HandleRequest(HttpListenerContext context);
    }    
}
