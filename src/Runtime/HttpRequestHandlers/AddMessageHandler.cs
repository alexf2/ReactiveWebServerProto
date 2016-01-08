using System.Collections.Generic;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    /// <summary>
    /// Web request handler for http://server_domain:port/Guestbook
    /// Handles only POST verb. Expects two query body parameters: login and msgtext.
    /// Creates new message for specified user.
    /// </summary>
    sealed class AddMessageHandler: HttpBaseHandler, IHttpRequestHandler
    {
        readonly ILogger _logger;
        readonly IGuestBookDataProvider _gbProvider;

        public AddMessageHandler(ILogger logger, IGuestBookDataProvider gbProvider)
        {
            _logger = logger;
            _gbProvider = gbProvider;

            CssLinks.Add(BootStrapCss);            
            JsLinks.Add(JqueryJs);
            JsLinks.Add(BootStrapJs);            
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

            _logger.Info($"Adding message: {login}, {text}");

            if (string.IsNullOrEmpty(login))
            {
                rsp.Status = StatusCodes.BadRequest;
                rsp.Write("<p>User login should not be empty</p>");                
            }
            else
            {
                var ui = await _gbProvider.AddMessage(login, text);
                rsp.Status = StatusCodes.Created;
                rsp.Write($"<p>User '{ui.UserLogin}' added message</p>");                
            }
            rsp.Write("<div>");
        }
    }
}
