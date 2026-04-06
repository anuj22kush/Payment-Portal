using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Payments_Portal.Data
{
    public class JsonPaymentRepository : IPaymentRepository
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public JsonPaymentRepository(string filePath)
        {
            _filePath = filePath;

            // Ensure directory exists
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Ensure file exists
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        private async Task<List<Payment>> ReadPaymentsAsync()
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Payment>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Payment>();
        }

        private async Task WritePaymentsAsync(List<Payment> payments)
        {
            var json = JsonSerializer.Serialize(payments, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            await _lock.WaitAsync();
            try
            {
                return await ReadPaymentsAsync();
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
                var payments = await ReadPaymentsAsync();
                return payments.FirstOrDefault(p => p.Id == id);
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
                var payments = await ReadPaymentsAsync();
                return payments.FirstOrDefault(p => p.ClientRequestId == clientRequestId);
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
                var payments = await ReadPaymentsAsync();
                payments.Add(payment);
                await WritePaymentsAsync(payments);
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
                var payments = await ReadPaymentsAsync();
                var index = payments.FindIndex(p => p.Id == payment.Id);
                if (index >= 0)
                {
                    payments[index] = payment;
                    await WritePaymentsAsync(payments);
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
                var payments = await ReadPaymentsAsync();
                payments.RemoveAll(p => p.Id == id);
                await WritePaymentsAsync(payments);
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
                var payments = await ReadPaymentsAsync();
                var today = DateTime.UtcNow.Date;
                return payments.Count(p => p.CreatedAt.Date == today);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
