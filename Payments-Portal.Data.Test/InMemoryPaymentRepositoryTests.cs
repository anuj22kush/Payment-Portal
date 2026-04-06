using Payments_Portal.Data;

namespace Payments_Portal.Data.Test
{
    [TestClass]
    public class InMemoryPaymentRepositoryTests
    {
        private InMemoryPaymentRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            _repository = new InMemoryPaymentRepository();
        }

        [TestMethod]
        public async Task AddAsync_ShouldAddPayment()
        {
            var payment = CreateTestPayment();

            await _repository.AddAsync(payment);

            var all = await _repository.GetAllAsync();
            Assert.AreEqual(1, all.Count);
            Assert.AreEqual(payment.Id, all[0].Id);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            var all = await _repository.GetAllAsync();
            Assert.AreEqual(0, all.Count);
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingId_ShouldReturnPayment()
        {
            var payment = CreateTestPayment();
            await _repository.AddAsync(payment);

            var result = await _repository.GetByIdAsync(payment.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(payment.Id, result.Id);
            Assert.AreEqual(payment.Amount, result.Amount);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
        {
            var result = await _repository.GetByIdAsync(Guid.NewGuid());
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetByClientRequestIdAsync_ExistingId_ShouldReturnPayment()
        {
            var payment = CreateTestPayment();
            await _repository.AddAsync(payment);

            var result = await _repository.GetByClientRequestIdAsync(payment.ClientRequestId);

            Assert.IsNotNull(result);
            Assert.AreEqual(payment.ClientRequestId, result.ClientRequestId);
        }

        [TestMethod]
        public async Task GetByClientRequestIdAsync_NonExistingId_ShouldReturnNull()
        {
            var result = await _repository.GetByClientRequestIdAsync(Guid.NewGuid());
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldModifyPayment()
        {
            var payment = CreateTestPayment();
            await _repository.AddAsync(payment);

            payment.Amount = 999.99m;
            payment.Currency = "GBP";
            await _repository.UpdateAsync(payment);

            var updated = await _repository.GetByIdAsync(payment.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual(999.99m, updated.Amount);
            Assert.AreEqual("GBP", updated.Currency);
        }

        [TestMethod]
        public async Task DeleteAsync_ExistingId_ShouldRemovePayment()
        {
            var payment = CreateTestPayment();
            await _repository.AddAsync(payment);

            await _repository.DeleteAsync(payment.Id);

            var all = await _repository.GetAllAsync();
            Assert.AreEqual(0, all.Count);
        }

        [TestMethod]
        public async Task DeleteAsync_NonExistingId_ShouldNotThrow()
        {
            await _repository.DeleteAsync(Guid.NewGuid());
            var all = await _repository.GetAllAsync();
            Assert.AreEqual(0, all.Count);
        }

        [TestMethod]
        public async Task GetTodayPaymentCountAsync_ShouldCountOnlyTodaysPayments()
        {
            var todayPayment = CreateTestPayment();
            todayPayment.CreatedAt = DateTime.UtcNow;

            var yesterdayPayment = CreateTestPayment();
            yesterdayPayment.CreatedAt = DateTime.UtcNow.AddDays(-1);

            await _repository.AddAsync(todayPayment);
            await _repository.AddAsync(yesterdayPayment);

            var count = await _repository.GetTodayPaymentCountAsync();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task AddMultiplePayments_ShouldReturnAll()
        {
            var p1 = CreateTestPayment();
            var p2 = CreateTestPayment();
            var p3 = CreateTestPayment();

            await _repository.AddAsync(p1);
            await _repository.AddAsync(p2);
            await _repository.AddAsync(p3);

            var all = await _repository.GetAllAsync();
            Assert.AreEqual(3, all.Count);
        }

        private static Payment CreateTestPayment()
        {
            return new Payment
            {
                Id = Guid.NewGuid(),
                ClientRequestId = Guid.NewGuid(),
                Amount = 100.00m,
                Currency = "USD",
                Reference = "PAY-20260405-0001",
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
