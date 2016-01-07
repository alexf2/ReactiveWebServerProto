using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    sealed class AddUserHandler : HtmlBaseHandler, IHttpRequestHandler
    {
        readonly ILogger _logger;
        readonly IGuestBookDataProvider _gbProvider;

        public AddUserHandler(ILogger logger, IGuestBookDataProvider gbProvider)
        {
            _logger = logger;
            _gbProvider = gbProvider;

            CssLinks.Add(BootStrapCss);
            JsLinks.Add(JqueryJs);
            JsLinks.Add(BootStrapJs);                        
        }

        public string DisplayName => "AddUserHandler";

        public int Priority => 1;

        public HandlerPriorityClass PriorityClass => HandlerPriorityClass.Normal;

        protected override string Path => "Guestbook/Users";

        protected override string PageTitle => "New User";

        protected override IList<string> SupportedVerbs => PostVerb;

        protected override async Task RenderBody(IResponseContext rsp)
        {
            rsp.Write("<div class=\"container body-content\">");
            rsp.Write("<div class=\"clearfix\"></div>");

            var p = rsp.GetPostParameters();
            string login, dispName;
            p.TryGetValue("login", out login);
            p.TryGetValue("displayname", out dispName);

            if (string.IsNullOrEmpty(login))
            {
                rsp.Status = StatusCodes.BadRequest;
                rsp.Write("<p>User login should not be empty</p>");                
            }
            else
            {
                var ui = await _gbProvider.AddUser(login, dispName);
                rsp.Status = StatusCodes.Created;
                rsp.AddLink(Path + "/" + WebUtility.UrlDecode(login));
                rsp.Write($"<p>User '{ui.UserLogin}' created</p>");                
            }
            rsp.Write("<div>");
        }
    }
}
