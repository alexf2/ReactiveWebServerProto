using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    /// <summary>
    /// Represents a base for implemnting complex Html responses.
    /// Subclass provides a callback to the constructor, in 'execute' parameter. This callback renders some HTML content, specific for the response.
    /// </summary>
    sealed class HtmlResponse: ResponseContextBase, IResponseContext
    {
        readonly Func<IResponseContext, Task> _execute;
        readonly IList<string> _css, _js;
        readonly string _title;        

        public HtmlResponse(HttpListenerContext ctx, Func<IResponseContext, Task> execute, IList<string> css, IList<string> js, string title, IList<string> pathArgs) : base(ctx, pathArgs)
        {
            _execute = execute;
            _css = css;
            _js = js;
            _title = title;            
        }        

        protected override async Task InternalExecute()
        {
            RenderDocumentType(Response);
            StartDoc(Response);
            RenderHeader(Response);
            StartBody(Response);

            await _execute(this);

            EndBody(Response);
            EndDoc(Response);

            AddHeaders();
        }

        void RenderDocumentType(HttpListenerResponse rsp)
        {
            Write("<!DOCTYPE html> ");
        }
        void StartDoc(HttpListenerResponse rsp)
        {
            Write("<html>");
        }
        void RenderHeader(HttpListenerResponse rsp)
        {
            var bld = new StringBuilder();

            if (!string.IsNullOrEmpty(_title))
                bld.AppendFormat("<title>{0}</title>", HtmlEncode(_title));

            if (_css != null)
                foreach (var url in _css)
                    bld.AppendFormat("<link href='{0}' rel='stylesheet' />", url);

            if (_js != null)
                foreach (var url in _js)
                    bld.AppendFormat("<script src='{0}'></script>", url);

            Write($"<head>" +
                $"<meta charset='{Encoding.BodyName}'>" +
                "<meta name='viewport' content='width = device-width, initial-scale = 1.0'>" +
                $"{bld}" +
                $"</head>"
            );
        }
        void StartBody(HttpListenerResponse rsp)
        {
            Write("<body>");
        }
        void EndBody(HttpListenerResponse rsp)
        {
            Write("</body>");
        }
        void EndDoc(HttpListenerResponse rsp)
        {
            Write("</html>");
        }

        public static string HtmlEncode(string text)
        {
            if (text == null)
                return null;

            StringBuilder sb = new StringBuilder(text.Length);

            int len = text.Length;
            for (int i = 0; i < len; i++)
            {
                switch (text[i])
                {

                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case '"':
                        sb.Append("&quot;");
                        break;
                    case '&':
                        sb.Append("&amp;");
                        break;
                    default:
                        if (text[i] > 159)
                        {
                            // decimal numeric entity
                            sb.Append("&#");
                            sb.Append(((int)text[i]).ToString(CultureInfo.InvariantCulture));
                            sb.Append(";");
                        }
                        else
                            sb.Append(text[i]);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}

