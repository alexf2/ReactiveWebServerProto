using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    public abstract class ResponseBase
    {
        public enum StatusCodes
        {
            Ok = 200,
            Created = 201,
            NoContent = 204,
            NotModified = 304,
            BadRequest = 400,
            NotFound = 404,
            MethodNotAllowed = 405,
            InternalServerError = 500
        }

        static readonly string ServerString = $"ReactiveWebServer/1.0 (AnywayAnyday; {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion})";

        readonly Dictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { {"Server", ServerString}, {"Cache-Control", "no-cache" } };
        readonly HttpListenerResponse _response;
        long _contentSize;
        
        public ResponseBase(HttpListenerResponse response)
        {
            Status = StatusCodes.Ok;
            Encoding = Encoding.UTF8;
            _response = response;
            _contentSize = response.ContentLength64 < 0 ? 0: response.ContentLength64;
            response.SendChunked = false;
        }

        public abstract Task Execute();

        public StatusCodes Status { get; set; }

        public Encoding Encoding { get; set; }

        public IReadOnlyDictionary<string, string> Headers => new ReadOnlyDictionary<string, string>(_headers);

        public void AddOrUpdateHeader(string key, string value)
        {
            _headers[ key ] = value;
        }

        protected HttpListenerResponse Response => _response;

        public void Write(string str)
        {
            var arr = Encoding.GetBytes(str);
            _contentSize += arr.Length;
            if (!Response.SendChunked)
                _response.ContentLength64 = _contentSize;
            _response.OutputStream.Write(arr, 0, arr.Length);
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
                _response.AddHeader(kv.Key, kv.Value);
        }
    }
}
