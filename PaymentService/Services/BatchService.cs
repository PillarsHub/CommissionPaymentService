using Microsoft.VisualBasic;
using PaymentService.Interfaces;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class BatchService : IBatchService
    {
        private readonly IBonusRepository _bonusRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPayQuickerService _payService;
        private string _fundingAccountPublicId { get; set; }
        public BatchService(IBonusRepository bonusRepository, IPayQuickerService paymentService, ICustomerRepository customerRepository, IConfiguration config)
        {
            _bonusRepository = bonusRepository;
            _payService = paymentService;
            _fundingAccountPublicId = Environment.GetEnvironmentVariable("PayQuickerFundingAccountPublicId") ?? String.Empty;
            _customerRepository = customerRepository;
        }

        public async Task ProcesseBatch(Batch batch)
        {
            var successStatuses = new List<string>()
            {
                "TransactionStatusType_Complete",
            }; 
            var pendingStatuses = new List<string>()
            {
                "TransactionStatusType_Pending",
                "TransactionStatusType_Scheduled",
                "TransactionStatusType_ReviewRequired",
            }; var failureStatuses = new List<string>()
            {
                "TransactionStatusType_UNDEFINED",
                "TransactionStatusType_Failed",
                "TransactionStatusType_Canceled",
                "TransactionStatusType_Expired"
            };

            foreach (var release in batch.Releases)
            {
                var accountingId = $"{release.NodeId}-{release.BatchId}-{release.BonusId}";
                var customer = await _customerRepository.GetCustomer(release.NodeId);
                var req = new SendPaymentRequest()
                {
                    Payments = new List<SendPayment>()
                    {
                        new SendPayment()
                        {
                            AccountingId = accountingId,
                            FundingAccountPublicId = _fundingAccountPublicId,
                            IssuePlasticCard = false,
                            Monetary = new Monetary()
                            {
                                Amount = release.Amount
                            },
                            UserCompanyAssignedUniqueKey = release.NodeId,
                            UserNotificationEmailAddress = customer.EmailAddress,
                            RecipientUserLanguageCode = "en-us",
                        }
                    }
                };
                
                var result = await _payService.SendPaymentsAsync(req);
                if (result.First().Payments.Any() && result.First().Payments.Any(x=>x.AccountingId==accountingId && successStatuses.Contains(x.TransactionStatusType)))
                {
                    release.Status = Status.Success;
                }
                else if (result.First().Payments.Any() && result.First().Payments.Any(x =>
                             x.AccountingId == accountingId && pendingStatuses.Contains(x.TransactionStatusType)))
                {
                    release.Status = Status.Pending;
                }
                else
                {
                    release.Status = Status.Failure;
                }
            }
            //Process the bonuses and mark them released.
            var bonuses = batch.Releases.Select(x => { x.Status = Status.Success; return x; });
            await _bonusRepository.UpdateBatch(batch.Id, bonuses);
        }
    }
}
