using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    /// <summary>
    /// Web request handlerfor http://server_domain:port/Proxy&url=xxx
    /// Handles only GET verb. Expects 'ur' query string parameter.
    /// Requests specified Web resurce by URL.
    /// </summary>
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
            if (!HttpBaseHandler.IsRequestMatchHelper(context.Request, "Proxy", new List<string>(), HttpBaseHandler.GetVerb))
                return false;

            var url = WebUtility.UrlDecode(context.Request.QueryString["url"]);
            if (string.IsNullOrEmpty(url))
            {
                context.Response.StatusCode = (int)StatusCodes.BadRequest;
                context.Response.StatusDescription = "Query string 'url' parameter should be not empty";
                await new TextResponse(context, "Query string 'url' parameter should be not empty").Execute().ConfigureAwait(false);
            }
            else
            {
                context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                context.Response.AddHeader("Access-Control-Request-Method", "*");
                context.Response.AddHeader("Access-Control-Allow-Methods", "OPTIONS, GET");
                context.Response.AddHeader("Access-Control-Allow-Headers", "*");                

                
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = context.Request.UserAgent;
                using (var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                {
                    if (response.StatusCode != HttpStatusCode.OK || response.ContentLength < 1)
                    {
                        context.Response.StatusCode = (int) response.StatusCode;
                        context.Response.StatusDescription = response.StatusCode.ToString();
                    }
                    else
                    {
                        PrepareContext(context.Response, response);
                        using (var responseStream = response.GetResponseStream())
                        {
                            context.Response.SendChunked = true;
                            responseStream.CopyToAsync(context.Response.OutputStream);
                        }
                    }
                }
            }

            return true;
        }

        void PrepareContext(HttpListenerResponse tragetRsp, HttpWebResponse sourceRsp)
        {
            tragetRsp.ContentEncoding = Encoding.GetEncoding(sourceRsp.CharacterSet);
            tragetRsp.ContentType = sourceRsp.ContentType;

            foreach (string k in sourceRsp.Headers)
                foreach (string v in sourceRsp.Headers.GetValues(k))
                {
                    if (string.Equals(k, "Content-Length", StringComparison.OrdinalIgnoreCase))
                        continue;

                    tragetRsp.AddHeader(k, v);
                }
        }
    }
}
