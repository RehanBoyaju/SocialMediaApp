
using System.Net.Http.Headers;

namespace BlazorChatWasm.Services.Handler
{
    public class AuthenticatedHttpClientHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthenticatedHttpClientHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var customAuthenticationStateProvider = _serviceProvider.GetRequiredService<CustomAuthenticationStateProvider>();
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var tokenRefreshed = await customAuthenticationStateProvider.TryRefreshTokenAsync();
                if (tokenRefreshed)
                {
                    // Retry original request with new token 
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", customAuthenticationStateProvider.localStorage.GetItem<string>("accessToken"));
                    return await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }
    }
}
