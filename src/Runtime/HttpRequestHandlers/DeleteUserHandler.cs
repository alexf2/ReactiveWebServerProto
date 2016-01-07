using System.Collections.Generic;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    sealed class DeleteUserHandler : HtmlBaseHandler, IHttpRequestHandler
    {
        readonly ILogger _logger;
        readonly IGuestBookDataProvider _gbProvider;

        public DeleteUserHandler(ILogger logger, IGuestBookDataProvider gbProvider)
        {
            _logger = logger;
            _gbProvider = gbProvider;

            CssLinks.Add(BootStrapCss);
            JsLinks.Add(BootStrapJs);            
        }

        public string DisplayName => "DeletetHandler";

        public int Priority => 1;

        public HandlerPriorityClass PriorityClass => HandlerPriorityClass.Normal;

        protected override string Path => "Guestbook/Users/?";

        protected override string PageTitle => "Delete user";

        protected override IList<string> SupportedVerbs => DeleteVerb;

        protected override async Task RenderBody(IResponseContext rsp)
        {
            rsp.Write("<div class=\"container body-content\">");
            rsp.Write("<div class=\"clearfix\"></div>");

            var count = await _gbProvider.RemoveUser(rsp.PathArgs[0]).ConfigureAwait(false);

            if (count > 0)
                rsp.Write($"<p>User '{HtmlResponse.HtmlEncode(rsp.PathArgs[0])}' deleted</p>");
            else
                rsp.Write($"<p>User '{HtmlResponse.HtmlEncode(rsp.PathArgs[0])}' not found</p>");

            rsp.Write("<div>");
        }
    }
}
