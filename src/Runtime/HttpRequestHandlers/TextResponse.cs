using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

        public override void Execute()
        {            
            AddHeaders();
            Write(_msg);
        }
    }
}
