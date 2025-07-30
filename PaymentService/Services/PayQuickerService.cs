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

        public async Task<AccessToken> GetAccessTokenAsync(string clientId, string clientSecret, PaymentEnvironment environment)
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
                    return new AccessToken { FailReason = $"Failed to get token. Status: {response.StatusCode}, Content: {responseContent}" };
                }

                var tokenResponse = JsonSerializer.Deserialize<AccessToken>(responseContent);
                return tokenResponse ?? new AccessToken { FailReason = "Invalid access token response"};
            }
            catch (Exception ex)
            {
                return new AccessToken { FailReason = ex.Message };
            }
        }

        public async Task<List<SendPaymentsResult>> SendPaymentsAsync(string accessToken, string accountingId, PaymentEnvironment environment, SendPaymentRequest sendPaymentRequest)
        {
            try
            {
                var baseUrl = environment == PaymentEnvironment.Live ? _liveBaseUrl : _sandboxBaseUrl;
                var client = _httpClientFactory.CreateClient();

                var url = $"{baseUrl}/api/v1/companies/accounts/payments";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("X-MyPayQuicker-Version", "01-15-2018");

                var json = JsonSerializer.Serialize(sendPaymentRequest);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return GenerateFailReason(accountingId, $"Failed to send payments. Url: {url} Request: {json} Status: {(int)response.StatusCode} Response: {responseContent}");
                }

                var results = JsonSerializer.Deserialize<List<SendPaymentsResult>>(responseContent);
                return results ?? new List<SendPaymentsResult>();
            }
            catch (Exception ex)
            {
                return GenerateFailReason(accountingId, ex.Message);
            }
        }

        private List<SendPaymentsResult> GenerateFailReason(string accountingId, string failReason)
        {
            var failedPayment = new SendPayment { AccountingId = accountingId, FailReason = failReason };

            var failedPayments = new List<SendPayment>();
            failedPayments.Add(failedPayment);

            var result = new SendPaymentsResult { Payments = failedPayments };
            var results = new List<SendPaymentsResult>();
            results.Add(result);

            return results;            
        }

    }
}

