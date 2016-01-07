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
    public abstract class ResponseContextBase
    {        
        static readonly string ServerString = $"ReactiveWebServer/1.0 (AnywayAnyday; {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion})";

        readonly Dictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { {"Server", ServerString}, {"Cache-Control", "no-cache" } };
        readonly HttpListenerContext _ctx;
        long _contentSize;
        readonly IList<string> _pathArgs;

        public ResponseContextBase(HttpListenerContext ctx, IList<string> pathArgs, bool keepRspMode = false)
        {
            Status = StatusCodes.Ok;
            Encoding = Encoding.UTF8;
            _ctx = ctx;
            _contentSize = ctx.Response.ContentLength64 < 0 ? 0: ctx.Response.ContentLength64;
            if (!keepRspMode)
                ctx.Response.SendChunked = false;
            _pathArgs = pathArgs;
        }

        public abstract Task Execute();

        public StatusCodes Status { get; set; }

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
            _contentSize += arr.Length;
            if (!Response.SendChunked)
                _ctx.Response.ContentLength64 = _contentSize;
            _ctx.Response.OutputStream.Write(arr, 0, arr.Length);
        }        

        protected void AddHeaders()
        {
            if (!_headers.ContainsKey("Content-Type"))
                _headers["Content-Type"] = $"text/html";

            if (!_headers["Content-Type"].Contains("charset"))
                _headers["Content-Type"] = _headers["Content-Type"] + $"; charset={Encoding.BodyName}";

            //_headers["Content-Length"] = _contentSize.ToString(CultureInfo.InvariantCulture);
            //_response.ContentLength64 = _contentSize;
            //_response.ContentEncoding = Encoding;            

            foreach (var kv in _headers)
                _ctx.Response.AddHeader(kv.Key, kv.Value);
        }
    }
}
