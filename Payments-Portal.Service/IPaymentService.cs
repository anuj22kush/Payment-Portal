namespace Payments_Portal.Service
{
    /// <summary>
    /// Defines the contract for payment business operations.
    /// </summary>
    public interface IPaymentService
    {
        Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request);
        Task<List<PaymentDto>> GetAllPaymentsAsync();
        Task<PaymentDto?> UpdatePaymentAsync(Guid id, UpdatePaymentRequest request);
        Task<bool> DeletePaymentAsync(Guid id);
    }
}
