using Payments_Portal.Data;
using Payments_Portal.Service;

namespace Payments_Portal.Service.Test
{
    [TestClass]
    public class PaymentServiceTests
    {
        private InMemoryPaymentRepository _repository = null!;
        private PaymentService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _repository = new InMemoryPaymentRepository();
            var referenceGenerator = new DailySequentialReferenceGenerator(_repository);
            var mapper = new PaymentMapper();
            var validator = new PaymentValidator();
            _service = new PaymentService(_repository, referenceGenerator, mapper, validator);
        }

        #region CreatePaymentAsync

        [TestMethod]
        public async Task CreatePaymentAsync_ShouldCreatePaymentWithGeneratedReference()
        {
            var request = new CreatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                ClientRequestId = Guid.NewGuid()
            };

            var result = await _service.CreatePaymentAsync(request);

            Assert.IsNotNull(result);
            Assert.AreEqual(100.00m, result.Amount);
            Assert.AreEqual("USD", result.Currency);
            Assert.IsTrue(result.Reference.StartsWith("PAY-"));
            Assert.AreNotEqual(Guid.Empty, result.Id);
        }

        [TestMethod]
        public async Task CreatePaymentAsync_ReferenceFormat_ShouldMatchPattern()
        {
            var request = new CreatePaymentRequest
            {
                Amount = 50.00m,
                Currency = "EUR",
                ClientRequestId = Guid.NewGuid()
            };

            var result = await _service.CreatePaymentAsync(request);

            // Reference format: PAY-YYYYMMDD-####
            Assert.IsTrue(result.Reference.StartsWith("PAY-"));
            var parts = result.Reference.Split('-');
            Assert.AreEqual(3, parts.Length);
            Assert.AreEqual("PAY", parts[0]);
            Assert.AreEqual(8, parts[1].Length); // YYYYMMDD
            Assert.AreEqual(4, parts[2].Length); // #### (sequential)
        }

        [TestMethod]
        public async Task CreatePaymentAsync_DuplicateClientRequestId_ShouldReturnExistingPayment()
        {
            var clientRequestId = Guid.NewGuid();

            var request1 = new CreatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                ClientRequestId = clientRequestId
            };

            var request2 = new CreatePaymentRequest
            {
                Amount = 200.00m,
                Currency = "EUR",
                ClientRequestId = clientRequestId
            };

            var result1 = await _service.CreatePaymentAsync(request1);
            var result2 = await _service.CreatePaymentAsync(request2);

            // Same clientRequestId → returns the same original record
            Assert.AreEqual(result1.Id, result2.Id);
            Assert.AreEqual(result1.Reference, result2.Reference);
            Assert.AreEqual(100.00m, result2.Amount); // Original amount, not 200
            Assert.AreEqual("USD", result2.Currency); // Original currency, not EUR
        }

        [TestMethod]
        public async Task CreatePaymentAsync_DifferentClientRequestIds_ShouldCreateDifferentPayments()
        {
            var request1 = new CreatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                ClientRequestId = Guid.NewGuid()
            };

            var request2 = new CreatePaymentRequest
            {
                Amount = 250.00m,
                Currency = "EUR",
                ClientRequestId = Guid.NewGuid()
            };

            var result1 = await _service.CreatePaymentAsync(request1);
            var result2 = await _service.CreatePaymentAsync(request2);

            Assert.AreNotEqual(result1.Id, result2.Id);
            Assert.AreNotEqual(result1.Reference, result2.Reference);
        }

        [TestMethod]
        public async Task CreatePaymentAsync_SequentialReferences_ShouldIncrement()
        {
            // Use a dedicated service+repo to isolate from other tests.
            // Note: The static counter in DailySequentialReferenceGenerator is shared,
            // so we only verify that two consecutive calls produce incrementing sequences.
            var isolatedRepo = new InMemoryPaymentRepository();
            var isolatedRefGen = new DailySequentialReferenceGenerator(isolatedRepo);
            var isolatedService = new PaymentService(isolatedRepo, isolatedRefGen, new PaymentMapper(), new PaymentValidator());

            var result1 = await isolatedService.CreatePaymentAsync(new CreatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                ClientRequestId = Guid.NewGuid()
            });

            var result2 = await isolatedService.CreatePaymentAsync(new CreatePaymentRequest
            {
                Amount = 200.00m,
                Currency = "EUR",
                ClientRequestId = Guid.NewGuid()
            });

            // Extract the sequential part — second should be greater than first
            var seq1 = int.Parse(result1.Reference.Split('-')[2]);
            var seq2 = int.Parse(result2.Reference.Split('-')[2]);

            Assert.IsTrue(seq2 > seq1, $"Second sequence ({seq2}) should be greater than first ({seq1})");
        }

        #endregion

        #region GetAllPaymentsAsync

        [TestMethod]
        public async Task GetAllPaymentsAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            var result = await _service.GetAllPaymentsAsync();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllPaymentsAsync_ShouldReturnAllPayments()
        {
            await _service.CreatePaymentAsync(new CreatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                ClientRequestId = Guid.NewGuid()
            });
            await _service.CreatePaymentAsync(new CreatePaymentRequest
            {
                Amount = 200.00m,
                Currency = "EUR",
                ClientRequestId = Guid.NewGuid()
            });

            var result = await _service.GetAllPaymentsAsync();

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetAllPaymentsAsync_ShouldReturnOrderedByCreatedAtDescending()
        {
            var first = await _service.CreatePaymentAsync(new CreatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                ClientRequestId = Guid.NewGuid()
            });

            // Small delay to ensure different timestamps
            await Task.Delay(50);

            var second = await _service.CreatePaymentAsync(new CreatePaymentRequest
            {
                Amount = 200.00m,
                Currency = "EUR",
                ClientRequestId = Guid.NewGuid()
            });

            var result = await _service.GetAllPaymentsAsync();

            Assert.AreEqual(second.Id, result[0].Id);
            Assert.AreEqual(first.Id, result[1].Id);
        }

        #endregion

        #region UpdatePaymentAsync

        [TestMethod]
        public async Task UpdatePaymentAsync_ExistingId_ShouldUpdatePayment()
        {
            var created = await _service.CreatePaymentAsync(new CreatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                ClientRequestId = Guid.NewGuid()
            });

            var updateRequest = new UpdatePaymentRequest
            {
                Amount = 500.00m,
                Currency = "GBP"
            };

            var result = await _service.UpdatePaymentAsync(created.Id, updateRequest);

            Assert.IsNotNull(result);
            Assert.AreEqual(500.00m, result.Amount);
            Assert.AreEqual("GBP", result.Currency);
            Assert.AreEqual(created.Reference, result.Reference); // Reference should not change
        }

        [TestMethod]
        public async Task UpdatePaymentAsync_NonExistingId_ShouldReturnNull()
        {
            var result = await _service.UpdatePaymentAsync(Guid.NewGuid(), new UpdatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD"
            });

            Assert.IsNull(result);
        }

        #endregion

        #region DeletePaymentAsync

        [TestMethod]
        public async Task DeletePaymentAsync_ExistingId_ShouldReturnTrueAndRemove()
        {
            var created = await _service.CreatePaymentAsync(new CreatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                ClientRequestId = Guid.NewGuid()
            });

            var result = await _service.DeletePaymentAsync(created.Id);

            Assert.IsTrue(result);

            var all = await _service.GetAllPaymentsAsync();
            Assert.AreEqual(0, all.Count);
        }

        [TestMethod]
        public async Task DeletePaymentAsync_NonExistingId_ShouldReturnFalse()
        {
            var result = await _service.DeletePaymentAsync(Guid.NewGuid());
            Assert.IsFalse(result);
        }

        #endregion
    }
}
