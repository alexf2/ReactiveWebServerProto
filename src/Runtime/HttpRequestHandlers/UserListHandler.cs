using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    public sealed class UserListHandler : HtmlBaseHandler, IHttpRequestHandler
    {
        readonly ILogger _logger;
        readonly IGuestBookDataProvider _gbProvider;

        public UserListHandler(ILogger logger, IGuestBookDataProvider gbProvider)
        {
            _logger = logger;
            _gbProvider = gbProvider;

            CssLinks.Add(BootStrapCss);
            JsLinks.Add(BootStrapJs);
            JsLinks.Add(JqueryJs);
            JsLinks.Add(KnockoutJs);
        }

        public string DisplayName => "UserListHandler";        

        public int Priority => 1;

        public HandlerPriorityClass PriorityClass => HandlerPriorityClass.Normal;

        protected override string Path => "Guestbook/Users";

        protected override string PageTitle => "Users list";

        protected override async Task RenderBody(ResponseBase rsp)
        {
            rsp.Write("<div class=\"container body-content\">");
            rsp.Write("<div class=\"clearfix\"></div>");

            var page = await _gbProvider.GetUsers(1, -1);
            if (page.Items == null)
                rsp.Write("<p>No users found</p>");
            else
            {                
                foreach (var u in page.Items)
                {
                    rsp.Write("<hr/><div class=\"row\" data-id=\"{{u.UserLogin}}\">");
                        rsp.Write($"<div class=\"col-md-1\"><b>{u.UserLogin}</b></div>");
                        rsp.Write($"<div class=\"col-md-4\">{u.DisplayName}</div>");
                    rsp.Write("</div>");
                }
            }
            rsp.Write("<div>");
        }        
    }
}
