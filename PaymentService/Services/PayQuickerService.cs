using PaymentService.Interfaces;
using PaymentService.Models;
using RestSharp;
using RestSharp.Authenticators;


namespace PaymentService.Services
{
    public class PayQuickerService : IPayQuickerService
    {
        private string _IdentityUrl { get; set; } = string.Empty;
        private string _LiveBaseUrl { get; set; } = string.Empty;
        private string _SandboxBaseUrl { get; set; } = string.Empty;

        public PayQuickerService()
        {
            _IdentityUrl = Environment.GetEnvironmentVariable("IdentityUrl") ?? string.Empty;
            _LiveBaseUrl = Environment.GetEnvironmentVariable("LiveBaseUrl") ?? string.Empty;
            _SandboxBaseUrl = Environment.GetEnvironmentVariable("SandboxBaseUrl") ?? string.Empty;
        }

        public async Task<string?> GetAccessTokenAsync(string _clientId, string _clientSecret)
        {
            try
            {
                var options = new RestClientOptions(_IdentityUrl)
                {
                    Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret)
                };
                var restClient = new RestClient(options);

                var request = new RestRequest("/core/connect/token")
                    .AddParameter("grant_type", "client_credentials")
                    .AddParameter("scope", "api useraccount_balance useraccount_debit useraccount_payment useraccount_invitation", false);
                var token = await restClient.PostAsync<AccessToken>(request);
                return token?.Token ?? string.Empty;
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
                var baseUrl = environment == PaymentEnvironment.Live ? _LiveBaseUrl : _SandboxBaseUrl;
                var options = new RestClientOptions(baseUrl)
                {
                    Timeout = TimeSpan.FromSeconds(15),
                    Authenticator = new JwtAuthenticator(accessToken)
                };
                var client = new RestClient(options);
                var request = new RestRequest("/api/v1/companies/accounts/payments").
                    AddHeader("X-MyPayQuicker-Version", "01-15-2018")
                    .AddJsonBody(sendPaymentRequest);
                
                var response = await client.PostAsync<List<SendPaymentsResult>>(request);
                return response ?? new List<SendPaymentsResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<SendPaymentsResult>();
            }
        }
    }
}
