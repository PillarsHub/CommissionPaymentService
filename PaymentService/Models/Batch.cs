using System;

namespace PaymentService.Models
{
    public class Batch
    {
        public string Id { get; set; } = string.Empty;
        public ReleaseResult[] Releases { get; set; } = Array.Empty<ReleaseResult>();
    }
}
