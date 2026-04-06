using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Payments_Portal.Data
{
    /// <summary>
    /// Abstraction for payment data access operations.
    /// </summary>
    public interface IPaymentRepository
    {
        Task<List<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(Guid id);
        Task<Payment?> GetByClientRequestIdAsync(Guid clientRequestId);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(Guid id);
        Task<int> GetTodayPaymentCountAsync();
    }
}
