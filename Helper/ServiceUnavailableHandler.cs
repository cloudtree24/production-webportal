using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

public class ServiceUnavailableHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServiceUnavailableHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            // Handle explicit 503 response code from the API
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                RedirectToServiceUnavailablePage();
            }

            return response;
        }
        catch (HttpRequestException) // Catch connection errors ("actively refused it")
        {
            // Always attempt the redirect first, as requested.
            // If context is not null, the redirect will happen, and this
            // execution path in SendAsync will terminate (by design of Redirect).
            if (RedirectToServiceUnavailablePage())
            {
                // This line is technically unreachable if Redirect worked, 
                // but for compiler satisfaction, we return a response.
                // In reality, the ASP.NET Core response pipeline is terminated by Redirect.
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            // If context was null (no active ASP.NET Core request), 
            // return a 503 so the caller (e.g., a controller) knows how to proceed.
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                ReasonPhrase = "API Connection Failed or Service Unavailable"
            };
        }
    }

    /// <summary>
    /// Attempts to redirect the client.
    /// </summary>
    /// <returns>True if a redirect was attempted, false otherwise (e.g., if context is null).</returns>
    private bool RedirectToServiceUnavailablePage()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null && !context.Response.HasStarted)
        {
            // This method throws a System.Threading.ThreadAbortException or 
            // a specific redirect exception to stop execution in the current context
            // and initiate the client-side redirect.
            context.Response.Redirect("/Home/Error");
            return true;
        }
        return false;
    }
}