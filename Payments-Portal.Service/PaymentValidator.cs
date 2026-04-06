namespace Payments_Portal.Service
{
    /// <summary>
    /// Validates payment requests beyond attribute-level validation.
    /// </summary>
    public interface IPaymentValidator
    {
        (bool IsValid, string? ErrorMessage) ValidateCreate(CreatePaymentRequest request);
        (bool IsValid, string? ErrorMessage) ValidateUpdate(UpdatePaymentRequest request);
    }

    public class PaymentValidator : IPaymentValidator
    {
        private static readonly HashSet<string> AllowedCurrencies = new(StringComparer.OrdinalIgnoreCase)
        {
            "USD", "EUR", "INR", "GBP"
        };

        public (bool IsValid, string? ErrorMessage) ValidateCreate(CreatePaymentRequest request)
        {
            if (request.Amount <= 0)
                return (false, "Amount must be greater than 0.");

            if (!AllowedCurrencies.Contains(request.Currency))
                return (false, $"Currency must be one of: {string.Join(", ", AllowedCurrencies)}.");

            if (request.ClientRequestId == Guid.Empty)
                return (false, "ClientRequestId is required.");

            return (true, null);
        }

        public (bool IsValid, string? ErrorMessage) ValidateUpdate(UpdatePaymentRequest request)
        {
            if (request.Amount <= 0)
                return (false, "Amount must be greater than 0.");

            if (!AllowedCurrencies.Contains(request.Currency))
                return (false, $"Currency must be one of: {string.Join(", ", AllowedCurrencies)}.");

            return (true, null);
        }
    }
}
