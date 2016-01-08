using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    /// <summary>
    /// Provides base for response contexts. Main points are collecting headers, response settings and buffering data.
    /// Response specific Html is generated in overriden InternalExecute. This implementation completes the request by writing in out to the response.
    /// </summary>
    public abstract class ResponseContextBase
    {        
        static readonly string ServerString = $"ReactiveWebServer/1.0 (AnywayAnyday; {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion})";

        readonly Dictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { {"Server", ServerString}, {"Cache-Control", "no-cache" } };
        readonly HttpListenerContext _ctx;

        readonly IList<string> _pathArgs;
        MemoryStream _buffer = new MemoryStream();

        protected ResponseContextBase(HttpListenerContext ctx, IList<string> pathArgs)
        {
            _ctx = ctx;
            ctx.Response.SendChunked = false;
            Encoding = Encoding.UTF8;                        
            _pathArgs = pathArgs;
        }

        public async Task Execute()
        {
            await InternalExecute();
            if (_buffer.Length > 0)
            {
                _ctx.Response.ContentLength64 = _buffer.Length;
                _ctx.Response.ContentEncoding = Encoding;
                _buffer.Seek(0, SeekOrigin.Begin);
                await _buffer.CopyToAsync(_ctx.Response.OutputStream);
                _buffer.Dispose();
                _buffer = null;
            }
        }

        protected abstract Task InternalExecute();        

        public StatusCodes Status {
            get { return (StatusCodes)_ctx.Response.StatusCode;  }
            set
            {
                _ctx.Response.StatusCode = (int)value;
                _ctx.Response.StatusDescription = value.ToString();
            }
        }

        public Encoding Encoding { get; set; }

        public IReadOnlyDictionary<string, string> Headers => new ReadOnlyDictionary<string, string>(_headers);

        public void AddOrUpdateHeader(string key, string value)
        {
            _headers[ key ] = value;
        }

        public HttpListenerResponse Response => _ctx.Response;
        public HttpListenerRequest Request => _ctx.Request;
        public IList<string> PathArgs => _pathArgs; 

        public int Page
        {
            get
            {
                int p;
                int.TryParse(_ctx.Request.QueryString["page"], out p);
                return p < 1 ? 1:p;
            }
        }
        public int Size
        {
            get
            {
                int s;
                int.TryParse(_ctx.Request.QueryString["size"], out s);
                return s == 0 ? -1:s;
            }
        }

        public string GetQueryParameter(string name)
        {
            return _ctx.Request.QueryString[ name ];
        }

        public IDictionary<string, string> GetPostParameters()
        {            
            var res = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var content = string.Empty;
            if (_ctx.Request.HasEntityBody)
            {
                using (var postBody = _ctx.Request.InputStream)
                    content = new StreamReader(postBody, _ctx.Request.ContentEncoding).ReadToEnd();
            }

            foreach (string kv in content.Split('&'))
            {
                string[] kvPair = kv.Split('=');
                if (kvPair.Length == 1)
                    res.Add(WebUtility.UrlDecode(kvPair[0]), string.Empty);
                else if (kvPair.Length == 2)
                    res.Add(WebUtility.UrlDecode(kvPair[0]), WebUtility.UrlDecode(kvPair[1]));
            }            

            return res;
        }

        public void Write(string str)
        {
            var arr = Encoding.GetBytes(str);
            _buffer.Write(arr, 0, arr.Length);
        }

        public void AddLink(string relativePath)
        {
            var b = new UriBuilder(_ctx.Request.Url.Scheme, _ctx.Request.Url.Host, _ctx.Request.Url.Port, relativePath);            
            _headers["Location"] = b.ToString();
        }

        protected void AddHeaders()
        {
            if (!_headers.ContainsKey("Content-Type"))
                _headers["Content-Type"] = $"text/html";

            if (!_headers["Content-Type"].Contains("charset"))
                _headers["Content-Type"] = _headers["Content-Type"] + $"; charset={Encoding.BodyName}";

            foreach (var kv in _headers)
            {
                if (string.Equals(kv.Key, "Content-Length", StringComparison.OrdinalIgnoreCase))
                    continue;
                _ctx.Response.AddHeader(kv.Key, kv.Value);
            }
        }

    }
}
