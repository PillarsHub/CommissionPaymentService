using Microsoft.Extensions.Caching.Memory;
using PaymentService.Interfaces;
using PaymentService.Models;
using RestSharp;
using RestSharp.Authenticators;


namespace PaymentService.Services
{
    public class PayQuickerService : IPayQuickerService
    {
        private IMemoryCache _cache { get; }
        private bool _isCacheEnabled { get; set; } = true;
        private string fundingAccountPublicId { get; } = "e72ca8d563b54f61ba0c929cfd854d64";
        private string _baseUrl { get; set; } = string.Empty;
        private string _clientId { get; set; } = string.Empty;
        private string _clientSecret { get; set; } = string.Empty;
        private string _bearerToken { get; set; } = string.Empty;
        private string _paymentsBaseUrl { get; set; } = string.Empty;

        public PayQuickerService(IConfiguration config, IMemoryCache cache)
        {
            _clientId = Environment.GetEnvironmentVariable("PayQuickerClientId") ?? String.Empty;
            _clientSecret = Environment.GetEnvironmentVariable("PayQuickerClientSecret") ?? String.Empty;
            _baseUrl = Environment.GetEnvironmentVariable("PayQuickerBaseUrl") ?? String.Empty;
            _paymentsBaseUrl = Environment.GetEnvironmentVariable("PayQuickerPaymentsBaseUrl") ?? String.Empty;
            _cache = cache;
            _isCacheEnabled = false;
        }

        public async Task<AccessToken?> GetAccessTokenAsync()
        {
            if (_isCacheEnabled && _cache.TryGetValue("AccessToken", out AccessToken? cachedToken))
            {
                return cachedToken;
            }

            var accessToken = await this.GenerateAccessTokenAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(55));
            _cache.Set("AccessToken", accessToken, cacheEntryOptions);

            return accessToken;
        }

        public async Task<string> GetBearerToken()
        {
            var accessToken = await GetAccessTokenAsync();
            return accessToken?.Token ?? string.Empty;
        }

        private async Task<AccessToken?> GenerateAccessTokenAsync()
        {
            try
            {
                var options = new RestClientOptions(_baseUrl)
                {
                    Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret)
                };
                var restClient = new RestClient(options);
                
                var request = new RestRequest("/core/connect/token")
                    .AddParameter("grant_type", "client_credentials")
                    .AddParameter("scope", "api useraccount_balance useraccount_debit useraccount_payment useraccount_invitation", false);
                var response = restClient.PostAsync<AccessToken>(request);
                return await response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new AccessToken();
            }
        }

        public async Task<List<SendPaymentsResult>> SendPaymentsAsync(SendPaymentRequest sendPaymentRequest)
        {
            try
            {
                var accessToken = await GetBearerToken();
                var options = new RestClientOptions("https://platform.mypayquicker.build")
                {
                    MaxTimeout = -1,
                    Authenticator = new JwtAuthenticator(accessToken)
                };
                var client = new RestClient(options);
                var request = new RestRequest("/api/v1/companies/accounts/payments").
                    AddHeader("X-MyPayQuicker-Version", "01-15-2018")
                    .AddJsonBody(sendPaymentRequest);
                
                var response = await client.PostAsync<List<SendPaymentsResult>>(request);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<SendPaymentsResult>();
            }
        }

        public async Task<List<Monetary>> GetBalanceForUserAsync(string userId)
        {
            try
            {
                var accessToken = await GetBearerToken();
                var options = new RestClientOptions("https://platform.mypayquicker.build")
                {
                    MaxTimeout = -1,
                    Authenticator = new JwtAuthenticator(accessToken)
                };
                var client = new RestClient(options);
                var request =
                    new RestRequest($"/api/v1/users/{userId}/accounts/action").AddHeader("X-MyPayQuicker-Version",
                        "01-15-2018");

                var response = await client.GetAsync<List<Monetary>>(request);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Monetary>();
            }
        }
    }
}
