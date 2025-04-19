namespace PaymentService.Interfaces
{
    public interface IClient
    {
        Task<T> Get<T>(string url, string callBackToken);
        Task<T> Put<T, R>(string url, R query, string callBackToken);
    }
}
