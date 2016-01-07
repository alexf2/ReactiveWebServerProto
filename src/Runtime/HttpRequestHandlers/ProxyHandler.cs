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
    sealed class ProxyHandler : IHttpRequestHandler
    {
        readonly ILogger _logger;

        public ProxyHandler(ILogger logger)
        {
            _logger = logger;
        }

        public string DisplayName => "ProxyHandler";

        public int Priority => 2;

        public HandlerPriorityClass PriorityClass => HandlerPriorityClass.Normal;        
                
        public async Task<bool> HandleRequest(HttpListenerContext context)
        {
            if (!HtmlBaseHandler.IsRequestMatchHelper(context.Request, "Proxy", new List<string>(), HtmlBaseHandler.GetVerb))
                return false;

            var url = WebUtility.UrlDecode(context.Request.QueryString["url"]);
            if (string.IsNullOrEmpty(url))
            {
                await new TextResponse(context, "Empty url parameter").Execute();
                context.Response.StatusCode = (int)StatusCodes.Created;
            }
            else
            {
                context.Response.StatusCode = (int) StatusCodes.NoContent;
            }

            return true;
        }
    }
}
