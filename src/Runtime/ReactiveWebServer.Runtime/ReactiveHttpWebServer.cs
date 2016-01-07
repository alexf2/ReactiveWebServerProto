using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AnywayAnyday.HttpRequestHandlers.Runtime;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.ReactiveWebServer.Runtime
{
    public class ReactiveHttpWebServer : ServiceHostBase, IWebServer
    {
        readonly IHttpListenerObservable _listener;
        readonly ILogger _logger;
        IDisposable _listenerSubscription;
        List<IHttpRequestHandler> _handlers;

        public ReactiveHttpWebServer(
            IHttpListenerObservableFactory listenerFactory, 
            IEnumerable<IHttpRequestHandler>  handlers,
            IHttpServerOptions opt, 
            ILogger logger): base(logger)
        {

            if (handlers == null || !handlers.Any())
                throw new ArgumentException("No Http handlers specified for the WebServer");

            _logger = logger;
            _listener = listenerFactory.Create((configurator) =>
            {
                foreach (var ep in opt.Endpoints) 
                    configurator.AddPrefix(ep.ToString());

                _logger.Info($"Listens on: {string.Join(", ", opt.Endpoints.Select(p => p.ToString()))}");
            }, logger);

            _handlers = handlers.ToList();
            _handlers.Sort((x, y) => x.PriorityClass == y.PriorityClass
                ? x.Priority.CompareTo(y.Priority)
                : x.PriorityClass.CompareTo(y.PriorityClass)
            );
        }

        #region IWebServer
        public override string DisplayName => "WebServer";
        #endregion IWebServer

        protected override void InternalStart()
        {
            _listener.Start();
            _listenerSubscription = _listener.Subscribe(HandleRequest, HandleListenerError, HandleComplete);
        }

        protected override void InternalStop()
        {
            _listenerSubscription.Dispose();
            _listenerSubscription = null;
            _listener.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            _listener.Dispose();
        }

        #region Http request processing
        async void HandleRequest (HttpListenerContext ctx)
        {
            LogRequest(ctx); 
            foreach (var handler in _handlers)
            {
                try
                {
                    if (await handler.HandleRequest(ctx))
                    {
                        _logger.Info($"{handler.DisplayName} executed");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("At handling request: ", ex);
                    try
                    {
                        await (new TextResponse(ctx, "<br/>Error at request handling: " + GetunfoldedError(ex), true)
                        {
                            Status = StatusCodes.InternalServerError
                        }).Execute();
                    }
                    catch (Exception e)
                    {
                        _logger.Error("At reporting error: ", e);
                    }
                    break;
                }
            }

            try
            {                
                ctx.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                _logger.Error("At compliting request: ", ex);
            }

            _logger.Info("Request processed");
        }
        void HandleListenerError(Exception ex)
        {            
            _logger.Error("HttpListener error: ", ex);
        }
        void HandleComplete()
        {
            _logger.Info("WebServer: listening completted");
        }
        #endregion Http request processing

        void LogRequest(HttpListenerContext ctx)
        {
            _logger.Info($"Method: {ctx.Request.HttpMethod}; Content-type: {ctx.Request.ContentType}, Path: {ctx.Request.Url.LocalPath}, Params: {ToStr(ctx.Request.QueryString)}");
        }

        string ToStr(NameValueCollection coll)
        {
            var bld = new StringBuilder();
            foreach (var k in coll.AllKeys)
            {
                if (bld.Length > 0)
                    bld.Append(", ");
                bld.Append($"{k} = {coll[k]}");
            }
            return bld.ToString();
        }

        string GetunfoldedError(Exception ex, StringBuilder bld = null)
        {
            if (bld == null)
                bld = new StringBuilder();

            if (ex is AggregateException)
                (ex as AggregateException).Handle(ex2 =>
                {
                    if (bld.Length > 0)
                        bld.Append("&nbsp;\r\n");
                    bld.Append(ex2.Message);
                    return true;
                });
            else
            {
                bld.Append(ex.Message);
                if (ex.InnerException != null)
                    GetunfoldedError(ex.InnerException, bld);
            }

            return bld.ToString();
        }
    }
}
