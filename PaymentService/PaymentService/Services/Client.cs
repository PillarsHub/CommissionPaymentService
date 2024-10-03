using PaymentService.Inerfaces;
using PaymentService.Services.Exceptions;

namespace PaymentService.Services
{
    public class Client : IClient
    {
        private readonly HttpClient _client;
        private readonly string _commissionRootUrl;

        public Client(IConfiguration configuration, HttpClient client)
        {
            _client = client;
            _commissionRootUrl = configuration.GetValue<string>("ApiUrl");
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

        public async Task<T> Get<T>(string url)
        {
            var result = await _client.GetAsync(GetRootUrl() + url);
            return await ProcessResult<T>(result);
        }

        public async Task<T> Put<T, R>(string url, R query)
        {
            var result = await _client.PutAsJsonAsync(GetRootUrl() + url, query);
            return await ProcessResult<T>(result);
        }
    }
}
