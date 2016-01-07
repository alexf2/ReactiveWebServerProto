using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
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

    interface IResponseContext
    {
        Task Execute();

        StatusCodes Status { get; set; }
        Encoding Encoding { get; set; }
        IReadOnlyDictionary<string, string> Headers { get; }
        void AddOrUpdateHeader(string key, string value);

        void Write(string str);

        int Page { get; }
        int Size { get; }
        HttpListenerResponse Response { get; }
        HttpListenerRequest Request { get; }
        string GetQueryParameter(string name);
        IList<string> PathArgs { get; }
    }
}
