using PaymentService.Models;

namespace PaymentService.Interfaces
{
    public interface ICustomerRepository
    {
        public Task<Customer> GetCustomer(string nodeId);
    }
}
