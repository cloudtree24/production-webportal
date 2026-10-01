using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HCMPortal.Middleware
{
    public class SessionTimeoutMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTimeoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            // skip login/logout & static files
            if (path != null &&(path.Contains("/customermanagement/login") ||path.Contains("/customermanagement/logout") ||path.StartsWith("/css") ||path.StartsWith("/js") ||path.StartsWith("/images") || path.StartsWith("/lib")))
            {
                await _next(context);
                return;
            }
            // SESSION CHECK
            var userId = context.Session.GetInt32("UserId");
            if (userId == null && context.User.Identity?.IsAuthenticated == true)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Session.Clear();
                context.Response.Cookies.Delete(".AspNetCore.Cookies");
                context.Response.Redirect("/CustomerManagement/Login");
                return;
            }
            await _next(context);
        }
    }
}
