using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
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

        public bool HandleRequest (HttpListenerContext context)
        {
            new TextResponse(context.Response, "Hello world!").Execute();
            return true;
        }
    }
}
