using System.Net.Http.Headers;

namespace PhotoGallery.Bff.Api.Shared
{
    public class JwtForwardHandler(IHttpContextAccessor contextAccessor) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var context = contextAccessor.HttpContext;

            //// If use Cookie, uncomment the following code to read token from cookie.
            //var token = context?.Request.Cookies[""];

            string? token = null;

            // 1️. Try to get token from HttpContext.Items (set temporarily in login flow)
            if (context?.Items.ContainsKey("jwt_token") == true)
            {
                token = context.Items["jwt_token"]?.ToString();
            }

            // 2️. Fallback: read token from Authorization header if present
            if (string.IsNullOrEmpty(token))
            {
                // JWT from Authorization header
                token = context?.Request.Headers.Authorization.FirstOrDefault();
            }
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
                //request.Headers.Add("Authorization", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
