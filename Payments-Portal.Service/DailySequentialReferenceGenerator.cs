using Payments_Portal.Data;

namespace Payments_Portal.Service
{
    /// <summary>
    /// Generates sequential payment references in the format PAY-YYYYMMDD-####.
    /// Thread-safe: uses a semaphore to protect the static daily counter.
    /// </summary>
    public class DailySequentialReferenceGenerator : IReferenceGenerator
    {
        private static int _dailySequence = -1;
        private static DateTime _lastSequenceDate = DateTime.MinValue;
        private static readonly SemaphoreSlim _sequenceLock = new(1, 1);

        private readonly IPaymentRepository _repository;

        public DailySequentialReferenceGenerator(IPaymentRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> GenerateAsync()
        {
            await _sequenceLock.WaitAsync();
            try
            {
                var today = DateTime.UtcNow.Date;

                if (_dailySequence == -1 || today != _lastSequenceDate)
                {
                    _lastSequenceDate = today;
                    _dailySequence = await _repository.GetTodayPaymentCountAsync();
                }

                _dailySequence++;
                return $"PAY-{_lastSequenceDate:yyyyMMdd}-{_dailySequence:D4}";
            }
            finally
            {
                _sequenceLock.Release();
            }
        }
    }
}
