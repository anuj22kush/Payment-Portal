using System.ComponentModel.DataAnnotations;

namespace Payments_Portal.Service
{
    public class UpdatePaymentRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [RegularExpression("^(USD|EUR|INR|GBP)$", ErrorMessage = "Currency must be USD, EUR, INR, or GBP")]
        public string Currency { get; set; } = string.Empty;
    }
}
