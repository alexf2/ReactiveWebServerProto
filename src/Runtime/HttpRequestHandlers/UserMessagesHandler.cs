﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    /// <summary>
    /// Web request handlerfor http://server_domain:port/Guestbook/Users/user_login
    /// Handles only GET verb. Last part of the URL is User Login.
    /// Lists all the messages for specified user. Accepts optional query string parameters 'page' and 'size'. 
    /// If these parameters are omitted or size = -1, returns all the data.
    /// </summary>
    sealed class UserMessagesHandler: HttpBaseHandler, IHttpRequestHandler
    {
        readonly ILogger _logger;
        readonly IGuestBookDataProvider _gbProvider;

        public UserMessagesHandler(ILogger logger, IGuestBookDataProvider gbProvider)
        {
            _logger = logger;
            _gbProvider = gbProvider;

            CssLinks.Add(BootStrapCss);
            JsLinks.Add(JqueryJs);
            JsLinks.Add(BootStrapJs);                        
        }

        public string DisplayName => "UserMessageHandler";

        public int Priority => 1;

        public HandlerPriorityClass PriorityClass => HandlerPriorityClass.Normal;

        protected override string Path => "Guestbook/Users/?";

        protected override string PageTitle => "User messages";

        protected override IList<string> SupportedVerbs => GetVerb;

        protected override async Task RenderBody (IResponseContext rsp)
        {
            rsp.Write("<div class=\"container body-content\">");
            rsp.Write("<div class=\"clearfix\"></div>");

            _logger.Info($"Returning user '{rsp.PathArgs[0]}' messages {rsp.Page}, {rsp.Size}");

            var page = await _gbProvider.GetUserMessages(rsp.PathArgs[0], rsp.Page, rsp.Size).ConfigureAwait(false);

            if (page.Items == null)
                rsp.Write($"<p>User '{HtmlResponse.HtmlEncode(rsp.PathArgs[0])}' not found</p>");
            else if (!page.Items.Any())
                rsp.Write("<p>No messages</p>");
            else
            {
                foreach (var m in page.Items)
                {
                    rsp.Write("<hr/><div class=\"row\">");
                    rsp.Write($"<div class=\"col-md-2\"><b>{m.Created.ToString("G")}</b></div>");
                    rsp.Write("</div>");

                    rsp.Write("<div class=\"row\">");
                    rsp.Write($"<div class=\"col-md-12\">{HtmlResponse.HtmlEncode(m.Text)}</div>");
                    rsp.Write("</div>");
                }
            }
            rsp.Write("<div>");
        }
    }
}
