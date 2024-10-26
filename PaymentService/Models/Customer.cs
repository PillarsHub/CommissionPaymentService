using System.Text.Json.Serialization;

namespace PaymentService.Models
{
    public class Customer
    {

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("emailAddress")]
        public string EmailAddress { get; set; }
    }
}
