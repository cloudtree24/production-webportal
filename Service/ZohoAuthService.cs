using System.Text.Json;
using System.Text.Json.Serialization;
using HCMPortal.Helper;

namespace HCMPortal.Service
{
    public class ZohoAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ZohoAuthService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        /// <summary>
        /// Fetches credentials from config, populates the static Store, 
        /// and returns the one-time Grant Code.
        /// </summary>
        private string GetZohoCredentials()
        {
            // Set the static "Identity" of the app in the Store
            ZohoTokenStore.ClientId = _config["ZohoDesk:Client_ID"] ?? string.Empty;
            ZohoTokenStore.ClientSecret = _config["ZohoDesk:Client_Secret"] ?? string.Empty;

            // Load the permanent Refresh Token if it exists in appsettings
            var savedRefreshToken = _config["ZohoDesk:Refresh_Token"];
            if (!string.IsNullOrEmpty(savedRefreshToken))
            {
                ZohoTokenStore.RefreshToken = savedRefreshToken;
            }

            // Return the one-time Grant Code
            return ((!ZohoTokenStore.IsExpired)?_config["ZohoDesk:Generated_code"]: savedRefreshToken)??string.Empty;
        }

        public async Task GetInitialTokenAsync()
        {
            // This call populates Store.ClientId and Store.ClientSecret
            var grantCode = GetZohoCredentials();

            var parameters = new Dictionary<string, string>
            {
                { "client_id", ZohoTokenStore.ClientId },
                { "client_secret", ZohoTokenStore.ClientSecret },
                { "code", grantCode },
                { "grant_type", "authorization_code" }
            };

            await SendRequest(parameters);
        }

        public async Task RefreshAccessTokenAsync(string refreshToken)
        {
            string refreshCodeFromStore = "";
            // Ensure static credentials are loaded from config if the Store was reset (RAM cleared)
            if (string.IsNullOrEmpty(ZohoTokenStore.ClientId) || String.IsNullOrEmpty(refreshToken))
            {
                refreshCodeFromStore =GetZohoCredentials();
            }

            var parameters = new Dictionary<string, string>
            {
                { "client_id", ZohoTokenStore.ClientId },
                { "client_secret", ZohoTokenStore.ClientSecret },
                { "refresh_token", String.IsNullOrEmpty(refreshToken)?refreshCodeFromStore:refreshToken },
                { "grant_type", "refresh_token" }
            };

            await SendRequest(parameters);
        }

        private async Task SendRequest(Dictionary<string, string> parameters)
        {
            var requestContent = new FormUrlEncodedContent(parameters);

            // Note: India region accounts use .in
            var response = await _httpClient.PostAsync("https://accounts.zoho.in/oauth/v2/token", requestContent);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ZohoTokenResponse>(content);

                if (result != null && result.AccessToken!=null)
                {
                    ZohoTokenStore.AccessToken = result.AccessToken ?? string.Empty;

                    if (!string.IsNullOrEmpty(result.ApiDomain))
                        ZohoTokenStore.ApiDomain = result.ApiDomain;

                    // Set expiry time based on the seconds returned (usually 3600)
                    ZohoTokenStore.ExpiryTime = DateTime.Now.AddSeconds(result.ExpiresIn);

                    // Refresh token is usually only returned in the 'initial' call
                    if (!string.IsNullOrEmpty(result.RefreshToken))
                        ZohoTokenStore.RefreshToken = result.RefreshToken;
                }
            }
            else
            {
                // Optional: Log failure details
                var error = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"Zoho Auth Error: {error}");
            }
        }
    }

    public class ZohoTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("api_domain")]
        public string? ApiDomain { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}