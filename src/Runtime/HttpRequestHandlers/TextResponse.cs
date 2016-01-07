using System.Net;
using System.Threading.Tasks;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    public sealed class TextResponse: ResponseContextBase, IResponseContext
    {
        readonly string _msg;
        public TextResponse(HttpListenerContext ctx, string msg, bool keepRspMode = false) : base(ctx, null, keepRspMode)
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
