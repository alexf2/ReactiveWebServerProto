﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    /// <summary>
    /// Web request handlerfor http://server_domain:port/Guestbook
    /// Handles only DELETE verb. Doesn't have any parameters.
    /// Removes all the content. Deletes users. Messages are deleted automatically by means the cascading FK.
    /// </summary>
    sealed class ClearHandler: HttpBaseHandler, IHttpRequestHandler
    {
        readonly ILogger _logger;
        readonly IGuestBookDataProvider _gbProvider;

        public ClearHandler(ILogger logger, IGuestBookDataProvider gbProvider)
        {
            _logger = logger;
            _gbProvider = gbProvider;
            
            JsLinks.Add(BootStrapJs);
            CssLinks.Add(BootStrapCss);
        }

        public string DisplayName => "ClearHandler";

        public int Priority => 1;

        public HandlerPriorityClass PriorityClass => HandlerPriorityClass.Normal;

        protected override string Path => "Guestbook";

        protected override string PageTitle => "Clear the book";

        protected override IList<string> SupportedVerbs => DeleteVerb;

        protected override async Task RenderBody(IResponseContext rsp)
        {
            rsp.Write("<div class=\"container body-content\">");
            rsp.Write("<div class=\"clearfix\"></div>");

            _logger.Info("Clearing the Guest Book");

            await _gbProvider.Clear().ConfigureAwait(false);
            rsp.Write($"<p>The book cleared</p>");            

            rsp.Write("<div>");
        }
    }
}
