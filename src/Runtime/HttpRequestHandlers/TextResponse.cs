﻿using System.Net;
using System.Threading.Tasks;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    /// <summary>
    /// Represents a simple text response.
    /// </summary>
    public sealed class TextResponse: ResponseContextBase, IResponseContext
    {
        readonly string _msg;
        public TextResponse(HttpListenerContext ctx, string msg) : base(ctx, null)
        {            
            _msg = msg;
        }

        protected override async Task InternalExecute()
        {                        
            Write(_msg);
            AddHeaders();
        }
    }
}
