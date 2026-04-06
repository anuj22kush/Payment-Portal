using System;

namespace Payments_Portal.Data
{
    public class Payment
    {
        public Guid Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid ClientRequestId { get; set; }
    }
}