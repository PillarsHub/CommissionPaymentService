using PaymentService.Interfaces;
using PaymentService.Services.Exceptions;
using System.Net.Http.Headers;

namespace PaymentService.Services
{
    public class Client : IClient
    {
        private readonly HttpClient _client;
        private readonly string _commissionRootUrl;

        public Client(HttpClient client)
        {
            _client = client;
            _commissionRootUrl = Environment.GetEnvironmentVariable("PillarsApiUrl") ?? string.Empty;
        }

        private string GetRootUrl() 
        {
            return _commissionRootUrl;
        }

        private async Task<T> ProcessResult<T>(HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new UniqueKeyException(content);
            }

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK ||
                responseMessage.StatusCode == System.Net.HttpStatusCode.Created ||
                responseMessage.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                if (string.IsNullOrWhiteSpace(content)) return default;
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return System.Text.Json.JsonSerializer.Deserialize<T>(content, options);
            }

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return default(T);
            }

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new NotFoundException(content);
            }

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new BadRequestException(content);
            }

            throw new System.Exception(content);
        }

        public async Task<T> Get<T>(string url, string callBackToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GetRootUrl() + url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", callBackToken);

            var result = await _client.SendAsync(request);
            return await ProcessResult<T>(result);
        }

        public async Task<T> Put<T, R>(string url, R query, string callBackToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, GetRootUrl() + url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", callBackToken);
            request.Content = JsonContent.Create(query);

            var result = await _client.SendAsync(request);
            return await ProcessResult<T>(result);
        }

    }
}
