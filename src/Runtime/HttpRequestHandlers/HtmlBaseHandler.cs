using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        protected static readonly string[] GetVerb = new[] {"GET"};
        protected static readonly string[] PostVerb = new[] { "POST" };
        protected static readonly string[] DeleteVerb = new[] { "DELETE" };

        readonly Lazy<List<string>> _css = new Lazy<List<string>>(() => new List<string>());
        readonly Lazy<List<string>> _js = new Lazy<List<string>>(() => new List<string>());
        readonly List<string> _pathArgs = new List<string>();

        protected abstract Task RenderBody(IResponseContext rsp);
        protected abstract string Path { get; }
        protected abstract IList<string> SupportedVerbs { get; }

        protected virtual string PageTitle => string.Empty;

        protected List<string> CssLinks => _css.Value;

        protected List<string> JsLinks => _js.Value;        

        public async Task<bool> HandleRequest(HttpListenerContext context)
        {
            if (!RequestMatch(context.Request))
                return false;

            await new HtmlResponse(context, RenderBody, _css.IsValueCreated ? _css.Value:null, _js.IsValueCreated ? _js.Value : null, PageTitle, _pathArgs).Execute();

            return true;
        }

        protected virtual bool RequestMatch(HttpListenerRequest req)
        {
            if (!SupportedVerbs.Any(v => v.Equals(req.HttpMethod, StringComparison.OrdinalIgnoreCase)))
                return false;

            _pathArgs.Clear();
            var comps = Path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);            
            return comps.SequenceEqual(req.Url.Segments.Skip(1).Select(item => item.Replace("/", string.Empty)), new PathEqualityComparer(_pathArgs));
        }

        sealed class PathEqualityComparer : IEqualityComparer<string>
        {
            readonly IList<string> _pathArguments;
            //internal static PathEqualityComparer Def = new PathEqualityComparer();
            internal PathEqualityComparer(IList<string> pathArguments)
            {
                _pathArguments = pathArguments;
            }

            public bool Equals(string x, string y)
            {
                if (x == "?")
                {
                    _pathArguments.Add(y);
                    return true;
                }
                return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
