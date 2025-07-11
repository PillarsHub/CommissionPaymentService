using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PaymentService.Interfaces;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class PayQuickerService : IPayQuickerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _liveIdentityUrl;
        private readonly string _liveBaseUrl;
        private readonly string _sandboxIdentityUrl;
        private readonly string _sandboxBaseUrl;

        public PayQuickerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            _liveIdentityUrl = Environment.GetEnvironmentVariable("LiveIdentityUrl") ?? string.Empty;
            _liveBaseUrl = Environment.GetEnvironmentVariable("LiveBaseUrl") ?? string.Empty;

            _sandboxIdentityUrl = Environment.GetEnvironmentVariable("SandboxIdentityUrl") ?? string.Empty;
            _sandboxBaseUrl = Environment.GetEnvironmentVariable("SandboxBaseUrl") ?? string.Empty;
        }

        public async Task<string?> GetAccessTokenAsync(string clientId, string clientSecret, PaymentEnvironment environment)
        {
            try
            {
                var identityUrl = environment == PaymentEnvironment.Live ? _liveIdentityUrl : _sandboxIdentityUrl;

                var client = _httpClientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Post, $"{identityUrl}/core/connect/token");
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "api useraccount_balance useraccount_debit useraccount_payment useraccount_invitation")
                });

                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to get token. Status: {response.StatusCode}, Content: {responseContent}");
                    return null;
                }

                var tokenResponse = JsonSerializer.Deserialize<AccessToken>(responseContent);
                return tokenResponse?.Token ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<List<SendPaymentsResult>> SendPaymentsAsync(string accessToken, PaymentEnvironment environment, SendPaymentRequest sendPaymentRequest)
        {
            try
            {
                var baseUrl = environment == PaymentEnvironment.Live ? _liveBaseUrl : _sandboxBaseUrl;
                var client = _httpClientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/v1/companies/accounts/payments");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("X-MyPayQuicker-Version", "01-15-2018");

                var json = JsonSerializer.Serialize(sendPaymentRequest);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to send payments. Status: {response.StatusCode}, Content: {responseContent}");
                    return new List<SendPaymentsResult>();
                }

                var results = JsonSerializer.Deserialize<List<SendPaymentsResult>>(responseContent);
                return results ?? new List<SendPaymentsResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<SendPaymentsResult>();
            }
        }
    }
}

