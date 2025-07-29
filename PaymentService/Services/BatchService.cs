using PaymentService.Interfaces;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class BatchService : IBatchService
    {
        private readonly IBonusRepository _bonusRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPayQuickerService _payService;

        private static readonly HashSet<string> SuccessStatuses = new() { "TransactionStatusType_Complete" };
        private static readonly HashSet<string> PendingStatuses = new() { "TransactionStatusType_Pending", "TransactionStatusType_Scheduled", "TransactionStatusType_ReviewRequired" };
        //private static readonly HashSet<string> FailureStatuses = new() { "TransactionStatusType_UNDEFINED", "TransactionStatusType_Failed", "TransactionStatusType_Canceled", "TransactionStatusType_Expired" };

        public BatchService(IBonusRepository bonusRepository, IPayQuickerService paymentService, ICustomerRepository customerRepository, IConfiguration config)
        {
            _bonusRepository = bonusRepository;
            _payService = paymentService;
            _customerRepository = customerRepository;
        }

        public async Task ProcesseBatch(Batch batch, string callbackToken, string pqClientId, string pqClientSecret, string pqFundingAccountPublicId, PaymentEnvironment pqEnvironment)
        {
            var accessToken = await _payService.GetAccessTokenAsync(pqClientId, pqClientSecret, pqEnvironment);
            if (string.IsNullOrWhiteSpace(accessToken.Token))
            {
                foreach (var release in batch.Releases)
                {
                    release.Status = Status.Failure;
                    release.StatusReason = accessToken.FailReason;
                }

                await _bonusRepository.UpdateBatch(callbackToken, batch.Id, batch.Releases);
                return;
            }

            var processed = new List<ReleaseResult>();
            var updateInterval = TimeSpan.FromSeconds(5);
            var lastUpdateTime = DateTime.UtcNow;

            foreach (var release in batch.Releases)
            {
                try
                {
                    var accountingId = $"{release.NodeId}-{release.BatchId}-{release.DetailId}";
                    var customer = await _customerRepository.GetCustomer(callbackToken, release.NodeId);

                    var paymentRequest = BuildPaymentRequest(accountingId, release, customer.EmailAddress, pqFundingAccountPublicId);
                    var response = await _payService.SendPaymentsAsync(accessToken.Token, pqEnvironment, paymentRequest);

                    var aabb = DetermineStatus(response, accountingId);
                    release.Status = aabb.Item1;
                    release.StatusReason = aabb.Item2;
                }
                catch (Exception ex)
                {
                    release.Status = Status.Failure;
                    release.StatusReason = ex.Message;
                }

                processed.Add(release);

                // Check if it's time to flush
                if (DateTime.UtcNow - lastUpdateTime >= updateInterval)
                {
                    await _bonusRepository.UpdateBatch(callbackToken, batch.Id, processed.ToArray());
                    processed.Clear();
                    lastUpdateTime = DateTime.UtcNow;
                }
            }

            // Final flush if any remain
            if (processed.Count > 0)
            {
                await _bonusRepository.UpdateBatch(callbackToken, batch.Id, processed.ToArray());
            }
        }

        private SendPaymentRequest BuildPaymentRequest(string accountingId, ReleaseResult release, string email, string fundingAccountPublicId)
        {
            return new SendPaymentRequest
            {
                Payments = new List<SendPayment>
                {
                    new SendPayment
                    {
                        AccountingId = accountingId,
                        FundingAccountPublicId = fundingAccountPublicId,
                        IssuePlasticCard = false,
                        Monetary = new Monetary
                        {
                            Amount = release.Amount
                        },
                        UserCompanyAssignedUniqueKey = release.NodeId,
                        UserNotificationEmailAddress = email,
                        RecipientUserLanguageCode = "en-us",
                        //"memoComment": "Congratulations on your recent success!"
                    }
                }
            };
        }

        private (Status, string) DetermineStatus(List<SendPaymentsResult> responses, string accountingId)
        {
            var payment = responses.FirstOrDefault()?.Payments.FirstOrDefault(p => p.AccountingId == accountingId);
            if (payment == null) return (Status.Failure, "No payment response");

            if (SuccessStatuses.Contains(payment.TransactionStatusType)) return (Status.Success, "");
            if (PendingStatuses.Contains(payment.TransactionStatusType)) return (Status.Success, "");

            return (Status.Failure, payment.TransactionStatusType);
        }
    }
}
