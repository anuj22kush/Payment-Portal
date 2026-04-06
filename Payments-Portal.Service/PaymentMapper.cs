using Payments_Portal.Data;

namespace Payments_Portal.Service
{
    /// <summary>
    /// Maps between Payment entities and PaymentDto objects.
    /// </summary>
    public interface IPaymentMapper
    {
        PaymentDto ToDto(Payment payment);
    }

    public class PaymentMapper : IPaymentMapper
    {
        public PaymentDto ToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                Reference = payment.Reference,
                Amount = payment.Amount,
                Currency = payment.Currency,
                CreatedAt = payment.CreatedAt
            };
        }
    }
}
