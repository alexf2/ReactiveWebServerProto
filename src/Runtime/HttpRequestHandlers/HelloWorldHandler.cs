using System.Net;
using System.Threading.Tasks;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    /// <summary>
    /// The fallback handler for all not handled URLs.
    /// Returns 200 Http code with "Hello world" body.    
    /// </summary>
    public sealed class HelloWorldHandler : IHttpRequestHandler
    {
        readonly ILogger _logger;

        public HelloWorldHandler(ILogger logger)
        {
            _logger = logger;
        }

        public string DisplayName => "HelloWorldHandler";

        public int Priority => 999;
        
        public HandlerPriorityClass PriorityClass => HandlerPriorityClass.Fallback;

        public async Task<bool> HandleRequest (HttpListenerContext context)
        {
            await new TextResponse(context, "Hello world!").Execute();
            return true;
        }
    }
}
