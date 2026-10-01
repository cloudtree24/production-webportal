//namespace HCMPortal.Middleware
//{
//    public class NoCacheMiddleware
//    {
//        private readonly RequestDelegate _next;

//        public NoCacheMiddleware(RequestDelegate next)
//        {
//            _next = next;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            var path = context.Request.Path.Value?.ToLower();

//            // Only apply to non-static requests
//            if (!path.StartsWith("/css") &&
//                !path.StartsWith("/js") &&
//                !path.StartsWith("/images") &&
//                !path.StartsWith("/lib"))
//            {
//                context.Response.Headers["Cache-Control"] =
//                    "no-cache, no-store, must-revalidate, max-age=0, private";
//                context.Response.Headers["Pragma"] = "no-cache";
//                context.Response.Headers["Expires"] = "0";
//            }

//            await _next(context);
//        }


//    }
//}
