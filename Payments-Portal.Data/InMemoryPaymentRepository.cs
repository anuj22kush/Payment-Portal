using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Payments_Portal.Data
{
    public class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly List<Payment> _payments = new();
        private readonly SemaphoreSlim _lock = new(1, 1);

        public async Task<List<Payment>> GetAllAsync()
        {
            await _lock.WaitAsync();
            try
            {
                return _payments.ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            await _lock.WaitAsync();
            try
            {
                return _payments.FirstOrDefault(p => p.Id == id);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Payment?> GetByClientRequestIdAsync(Guid clientRequestId)
        {
            await _lock.WaitAsync();
            try
            {
                return _payments.FirstOrDefault(p => p.ClientRequestId == clientRequestId);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task AddAsync(Payment payment)
        {
            await _lock.WaitAsync();
            try
            {
                _payments.Add(payment);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UpdateAsync(Payment payment)
        {
            await _lock.WaitAsync();
            try
            {
                var index = _payments.FindIndex(p => p.Id == payment.Id);
                if (index >= 0)
                {
                    _payments[index] = payment;
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            await _lock.WaitAsync();
            try
            {
                _payments.RemoveAll(p => p.Id == id);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<int> GetTodayPaymentCountAsync()
        {
            await _lock.WaitAsync();
            try
            {
                var today = DateTime.UtcNow.Date;
                return _payments.Count(p => p.CreatedAt.Date == today);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
