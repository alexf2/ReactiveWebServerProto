using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.HttpRequestHandlers.Runtime
{
    /// <summary>
    /// Web request handlerfor http://server_domain:port/Guestbook/Users
    /// Handles only GET verb. Doesn't expect any parameters.
    /// Lists all the users.    
    /// If these parameters are omitted or size = -1, returns all the data.
    /// </summary>
    sealed class UserListHandler : HttpBaseHandler, IHttpRequestHandler
    {
        readonly ILogger _logger;
        readonly IGuestBookDataProvider _gbProvider;

        public UserListHandler(ILogger logger, IGuestBookDataProvider gbProvider)
        {
            _logger = logger;
            _gbProvider = gbProvider;

            CssLinks.Add(BootStrapCss);
            JsLinks.Add(JqueryJs);
            JsLinks.Add(BootStrapJs);                        
        }

        public string DisplayName => "UserListHandler";        

        public int Priority => 1;

        public HandlerPriorityClass PriorityClass => HandlerPriorityClass.Normal;

        protected override string Path => "Guestbook/Users";

        protected override string PageTitle => "Users list";

        protected override IList<string> SupportedVerbs => GetVerb;

        protected override async Task RenderBody (IResponseContext rsp)
        {
            rsp.Write("<div class=\"container body-content\">");
            rsp.Write("<div class=\"clearfix\"></div>");                        

            var page = await _gbProvider.GetUsers(rsp.Page, rsp.Size).ConfigureAwait(false);

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
                }
            }
            rsp.Write("<div>");
        }        
    }
}
