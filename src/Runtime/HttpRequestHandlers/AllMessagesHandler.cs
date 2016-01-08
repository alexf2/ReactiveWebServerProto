using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    /// <summary>
    /// Web request handlerfor http://server_domain:port/Guestbook
    /// Handles only GET verb. Doesn't have any parameters.
    /// Displayes all the content: users and messages.
    /// </summary>
    sealed class AllMessagesHandler : HttpBaseHandler, IHttpRequestHandler
    {
        readonly ILogger _logger;
        readonly IGuestBookDataProvider _gbProvider;

        public AllMessagesHandler(ILogger logger, IGuestBookDataProvider gbProvider)
        {
            _logger = logger;
            _gbProvider = gbProvider;

            CssLinks.Add(BootStrapCss);
            JsLinks.Add(JqueryJs);
            JsLinks.Add(BootStrapJs);                        
        }

        public string DisplayName => "AllMessagesHandler";

        public int Priority => 1;

        public HandlerPriorityClass PriorityClass => HandlerPriorityClass.Normal;

        protected override string Path => "Guestbook";

        protected override string PageTitle => "All the guestbook content";

        protected override IList<string> SupportedVerbs => GetVerb;

        protected override async Task RenderBody(IResponseContext rsp)
        {
            rsp.Write("<div class=\"container body-content\">");
            rsp.Write("<div class=\"clearfix\"></div>");

            var page = await _gbProvider.GetUsers(1, -1).ConfigureAwait(false);

            if (!page.Items.Any())
                rsp.Write("<p>No users found</p>");
            else
            {
                foreach (var u in page.Items)
                {
                    rsp.Write($"<hr/><div class=\"row\" data-id=\"{HtmlResponse.HtmlEncode(u.UserLogin)}\">");
                    rsp.Write($"<div class=\"col-md-1\"><b>{u.UserLogin}</b></div>");
                    rsp.Write($"<div class=\"col-md-4\">{HtmlResponse.HtmlEncode(u.DisplayName)}</div>");
                    rsp.Write("</div>");

                    var msgPage = await _gbProvider.GetUserMessages(u.UserLogin, 1, -1).ConfigureAwait(false);

                    foreach (var m in msgPage.Items)
                    {
                        rsp.Write("<div class=\"row\">");
                        rsp.Write("<div class=\"col-md-1\">&nbsp;</div>");
                        rsp.Write($"<div class=\"col-md-2\"><b>{m.Created.ToString("G")}</b></div>");
                        rsp.Write("</div>");

                        rsp.Write("<div class=\"row\">");
                        rsp.Write($"<div class=\"col-md-12\">{HtmlResponse.HtmlEncode(m.Text)}</div>");
                        rsp.Write("</div>");
                    }
                    rsp.Write("<div class=\"clearfix\"></div>");
                }
            }
            rsp.Write("<div>");
        }
    }
}
