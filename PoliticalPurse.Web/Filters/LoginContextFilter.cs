using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PoliticalPurse.Web.Filters
{
    public class LoginContextFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public async Task<AuthenticationContext> GetAuthenticationContextFromHttpContext(HttpContext context)
        {
            var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result == null || !result.Succeeded)
            {
                return null;
            }
            
            return new AuthenticationContext
            {
                UserId = long.Parse(result.Principal.FindFirst(c => c.Type == ClaimTypes.Sid).Value),
                UserName = result.Principal.FindFirst(c => c.Type == ClaimTypes.Name).Value,
                UserEmail = result.Principal.FindFirst(c => c.Type == ClaimTypes.Email).Value,
            };
        }

        public async void OnActionExecuting(ActionExecutingContext context)
        {
            var authContext = await GetAuthenticationContextFromHttpContext(context.HttpContext);

            var controller = context.Controller as Controller;
            if (controller == null) return;

            controller.ViewData["SignedIn"] = authContext != null;

            if (authContext != null)
            {
                SetOrganisationScopeViewData(controller, authContext);
            }
        }

        private void SetOrganisationScopeViewData(Controller controller, AuthenticationContext authContext)
        {
            controller.ViewData["UserId"] = authContext.UserId;
            controller.ViewData["UserName"] = authContext.UserName;
            controller.ViewData["UserEmail"] = authContext.UserEmail;
        }
    }

    public class AuthenticationContext
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
    }
}