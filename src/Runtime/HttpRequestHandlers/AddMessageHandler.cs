using System.Collections.Generic;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    sealed class AddMessageHandler: HtmlBaseHandler, IHttpRequestHandler
    {
        readonly ILogger _logger;
        readonly IGuestBookDataProvider _gbProvider;

        public AddMessageHandler(ILogger logger, IGuestBookDataProvider gbProvider)
        {
            _logger = logger;
            _gbProvider = gbProvider;

            CssLinks.Add(BootStrapCss);
            JsLinks.Add(BootStrapJs);
            JsLinks.Add(JqueryJs);
            JsLinks.Add(KnockoutJs);
        }

        public string DisplayName => "AddMessageHandler";

        public int Priority => 1;

        public HandlerPriorityClass PriorityClass => HandlerPriorityClass.Normal;

        protected override string Path => "Guestbook";

        protected override string PageTitle => "New Message";

        protected override IList<string> SupportedVerbs => PostVerb;

        protected override async Task RenderBody(IResponseContext rsp)
        {
            rsp.Write("<div class=\"container body-content\">");
            rsp.Write("<div class=\"clearfix\"></div>");

            var p = rsp.GetPostParameters();
            string login, text;
            p.TryGetValue("login", out login);
            p.TryGetValue("msgtext", out text);

            if (string.IsNullOrEmpty(login))
                rsp.Write("<p>User login should not be empty</p>");
            else
            {
                var ui = await _gbProvider.AddMessage(login, text);
                rsp.Write($"<p>User '{ui.UserLogin}' added message</p>");
            }
            rsp.Write("<div>");
        }
    }
}
