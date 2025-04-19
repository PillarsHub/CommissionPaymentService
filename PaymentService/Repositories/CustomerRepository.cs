using PaymentService.Interfaces;
using PaymentService.Models;

namespace PaymentService.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IClient _client;

        public CustomerRepository(IClient client)
        {
            _client = client;
        }

        public async Task<Customer> GetCustomer(string token, string nodeId)
        {
            return await _client.Get<Customer>($"/api/v1/Customers/{nodeId}", token);
        }
    }
}
