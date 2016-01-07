using System.Net;
using System.Threading.Tasks;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    public sealed class TextResponse: ResponseBase
    {
        readonly string _msg;
        public TextResponse(HttpListenerResponse response, string msg) : base(response)
        {            
            _msg = msg;
        }

        public override async Task Execute()
        {            
            AddHeaders();
            Write(_msg);
        }
    }
}
