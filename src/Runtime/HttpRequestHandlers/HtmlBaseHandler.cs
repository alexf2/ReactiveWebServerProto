using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    public abstract class HtmlBaseHandler
    {
        protected const string BootStrapCss = "http://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css";
        protected const string BootStrapJs = "http://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/js/bootstrap.min.js";
        protected const string JqueryJs = "http://code.jquery.com/jquery-2.1.4.min.js";
        protected const string KnockoutJs = "http://cdnjs.cloudflare.com/ajax/libs/knockout/3.3.0/knockout-min.js";

        readonly Lazy<List<string>> _css = new Lazy<List<string>>(() => new List<string>());
        readonly Lazy<List<string>> _js = new Lazy<List<string>>(() => new List<string>());

        protected abstract Task RenderBody(ResponseBase rsp);
        protected abstract string Path { get; }

        protected virtual string PageTitle => string.Empty;

        protected List<string> CssLinks => _css.Value;

        protected List<string> JsLinks => _js.Value;

        public async Task<bool> HandleRequest(HttpListenerContext context)
        {
            if (!PathMatch(context.Request))
                return false;

            await new HtmlResponse(context.Response, RenderBody, _css.IsValueCreated ? _css.Value:null, _js.IsValueCreated ? _js.Value : null, PageTitle).Execute();

            return true;
        }

        protected virtual bool PathMatch(HttpListenerRequest req)
        {
            var comps = Path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            return comps.SequenceEqual(req.Url.Segments.Skip(1).Select(item => item.Replace("/", string.Empty)), StringComparer.OrdinalIgnoreCase);
        }

    }
}
