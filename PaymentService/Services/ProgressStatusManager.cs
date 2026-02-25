using PaymentService.Models;
using System.Collections.Concurrent;

namespace PaymentService.Services
{
    public class ProgressStatusManager
    {
        private readonly ConcurrentDictionary<long, Status> _progressStatuses = new ConcurrentDictionary<long, Status>();
        private readonly ConcurrentDictionary<long, ReleaseResult> _progressResults = new ConcurrentDictionary<long, ReleaseResult>();
        private readonly ConcurrentDictionary<string, (int,int)> _updateCounts = new ConcurrentDictionary<string, (int, int)>();
        private string _lastErrorMessage = string.Empty;

        public void UpdateUpdateCount(string key, (int,int) values)
        {
            _updateCounts.AddOrUpdate(key, values, (s, e) => values);
        }

        public List<(string,(int,int))> GetUpdateCount()
        {
            var aa = _updateCounts.Select(x => (x.Key, x.Value));
            return aa.ToList();
        }   

        public void SetLastErrorMessage(string message)
        {
            lock (_progressStatuses)
            {
                _lastErrorMessage = message;
            }
        }

        public string GetLastErrorMessage()
        {
            lock (_progressStatuses)
            {
                return _lastErrorMessage;
            }
        }

        public void UpdateProgressStatus(List<ReleaseResult>? payments)
        {
            foreach(var payment in payments ?? Enumerable.Empty<ReleaseResult>())
            {
                UpdateProgressStatus(payment);
            }
        }

        public void UpdateProgressStatus(ReleaseResult? payment)
        {
            if (payment == null) return;

            lock (_progressStatuses)
            {
                if (payment.Status == Status.Pending)
                {
                    _progressStatuses.AddOrUpdate(payment.DetailId, payment.Status, (s, e) => payment.Status);
                    _progressResults.AddOrUpdate(payment.DetailId, payment, (s, e) => payment);
                }
                else
                {
                    _progressStatuses.TryRemove(payment.DetailId, out _);
                    _progressResults.TryRemove(payment.DetailId, out _);
                }
            }
        }

        public List<(ReleaseResult, Status)> GetProgressStatus()
        {
            lock (_progressStatuses)
            {
                var results = new List<(ReleaseResult, Status)>();
                foreach (var kvp in _progressStatuses)
                {
                    if (_progressResults.TryGetValue(kvp.Key, out var result))
                    {
                        results.Add((result, kvp.Value));
                    }
                }
                return results;
            }
        }
    }
}
