using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    abstract class HtmlBaseHandler
    {
        protected const string BootStrapCss = "http://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css";
        protected const string BootStrapJs = "http://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/js/bootstrap.min.js";
        protected const string JqueryJs = "http://code.jquery.com/jquery-2.1.4.min.js";
        protected const string KnockoutJs = "http://cdnjs.cloudflare.com/ajax/libs/knockout/3.3.0/knockout-min.js";

        internal static readonly string[] GetVerb = new[] {"GET"};
        internal static readonly string[] PostVerb = new[] { "POST" };
        internal static readonly string[] DeleteVerb = new[] { "DELETE" };

        readonly Lazy<List<string>> _css = new Lazy<List<string>>(() => new List<string>());
        readonly Lazy<List<string>> _js = new Lazy<List<string>>(() => new List<string>());        

        protected abstract Task RenderBody(IResponseContext rsp);
        protected abstract string Path { get; }
        protected abstract IList<string> SupportedVerbs { get; }

        protected virtual string PageTitle => string.Empty;

        protected List<string> CssLinks => _css.Value;

        protected List<string> JsLinks => _js.Value;        

        public async Task<bool> HandleRequest(HttpListenerContext context)
        {
            List<string> pathArgs = new List<string>();
            if (!RequestMatch(context.Request, pathArgs))
                return false;

            await new HtmlResponse(context, RenderBody, _css.IsValueCreated ? _css.Value:null, _js.IsValueCreated ? _js.Value : null, PageTitle, pathArgs).Execute();

            return true;
        }

        #region Request matching
        protected virtual bool RequestMatch(HttpListenerRequest req, IList<string> pathArgs)
        {            
            return IsRequestMatchHelper(req, Path, pathArgs, SupportedVerbs);
        }

        internal static bool IsRequestMatchHelper(HttpListenerRequest req, string handlerPath, IList<string> pathArgs, IList<string> supportedVerbs)
        {
            if (!supportedVerbs.Any(v => v.Equals(req.HttpMethod, StringComparison.OrdinalIgnoreCase)))
                return false;

            pathArgs.Clear();
            var comps = handlerPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return comps.SequenceEqual(req.Url.Segments.Skip(1).Select(item => item.Replace("/", string.Empty)), new PathEqualityComparer(pathArgs));
        }

        sealed class PathEqualityComparer : IEqualityComparer<string>
        {
            readonly IList<string> _pathArguments;
            
            internal PathEqualityComparer(IList<string> pathArguments)
            {
                _pathArguments = pathArguments;
            }

            public bool Equals(string x, string y)
            {
                if (x == "?")
                {
                    _pathArguments.Add(WebUtility.UrlDecode(y));
                    return true;
                }
                return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }
        #endregion Request matching
    }
}
