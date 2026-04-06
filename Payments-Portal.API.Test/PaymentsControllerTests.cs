using Microsoft.AspNetCore.Mvc;
using Payments_Portal.API.Controllers;
using Payments_Portal.Service;

namespace Payments_Portal.API.Test
{
    [TestClass]
    public class PaymentsControllerTests
    {
        private MockPaymentService _mockService = null!;
        private PaymentsController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockService = new MockPaymentService();
            _controller = new PaymentsController(_mockService);
        }

        #region GetPayments

        [TestMethod]
        public async Task GetPayments_ShouldReturnOkWithPaymentsList()
        {
            _mockService.Payments.Add(CreatePaymentDto("PAY-20260405-0001", 100m, "USD"));
            _mockService.Payments.Add(CreatePaymentDto("PAY-20260405-0002", 250m, "EUR"));

            var result = await _controller.GetPayments();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var payments = okResult.Value as List<PaymentDto>;
            Assert.IsNotNull(payments);
            Assert.AreEqual(2, payments.Count);
        }

        [TestMethod]
        public async Task GetPayments_WhenEmpty_ShouldReturnOkWithEmptyList()
        {
            var result = await _controller.GetPayments();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var payments = okResult.Value as List<PaymentDto>;
            Assert.IsNotNull(payments);
            Assert.AreEqual(0, payments.Count);
        }

        #endregion

        #region CreatePayment

        [TestMethod]
        public async Task CreatePayment_ValidRequest_ShouldReturnCreatedResult()
        {
            var request = new CreatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                ClientRequestId = Guid.NewGuid()
            };

            var result = await _controller.CreatePayment(request);

            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);

            var payment = createdResult.Value as PaymentDto;
            Assert.IsNotNull(payment);
            Assert.AreEqual(100.00m, payment.Amount);
            Assert.AreEqual("USD", payment.Currency);
        }

        [TestMethod]
        public async Task CreatePayment_InvalidModel_ShouldReturnBadRequest()
        {
            _controller.ModelState.AddModelError("Amount", "Amount must be greater than 0");

            var request = new CreatePaymentRequest
            {
                Amount = 0m,
                Currency = "USD",
                ClientRequestId = Guid.NewGuid()
            };

            var result = await _controller.CreatePayment(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        #endregion

        #region UpdatePayment

        [TestMethod]
        public async Task UpdatePayment_ExistingId_ShouldReturnOkWithUpdatedPayment()
        {
            var dto = CreatePaymentDto("PAY-20260405-0001", 100m, "USD");
            _mockService.Payments.Add(dto);

            var request = new UpdatePaymentRequest
            {
                Amount = 500.00m,
                Currency = "GBP"
            };

            var result = await _controller.UpdatePayment(dto.Id, request);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var payment = okResult.Value as PaymentDto;
            Assert.IsNotNull(payment);
            Assert.AreEqual(500.00m, payment.Amount);
            Assert.AreEqual("GBP", payment.Currency);
        }

        [TestMethod]
        public async Task UpdatePayment_NonExistingId_ShouldReturnNotFound()
        {
            var request = new UpdatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD"
            };

            var result = await _controller.UpdatePayment(Guid.NewGuid(), request);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task UpdatePayment_InvalidModel_ShouldReturnBadRequest()
        {
            _controller.ModelState.AddModelError("Amount", "Amount must be greater than 0");

            var request = new UpdatePaymentRequest
            {
                Amount = 0m,
                Currency = "USD"
            };

            var result = await _controller.UpdatePayment(Guid.NewGuid(), request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        #endregion

        #region DeletePayment

        [TestMethod]
        public async Task DeletePayment_ExistingId_ShouldReturnNoContent()
        {
            var dto = CreatePaymentDto("PAY-20260405-0001", 100m, "USD");
            _mockService.Payments.Add(dto);

            var result = await _controller.DeletePayment(dto.Id);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task DeletePayment_NonExistingId_ShouldReturnNotFound()
        {
            var result = await _controller.DeletePayment(Guid.NewGuid());

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        #endregion

        #region Helpers

        private static PaymentDto CreatePaymentDto(string reference, decimal amount, string currency)
        {
            return new PaymentDto
            {
                Id = Guid.NewGuid(),
                Reference = reference,
                Amount = amount,
                Currency = currency,
                CreatedAt = DateTime.UtcNow
            };
        }

        #endregion
    }

    /// <summary>
    /// A lightweight manual mock of IPaymentService for controller-level tests.
    /// </summary>
    internal class MockPaymentService : IPaymentService
    {
        public List<PaymentDto> Payments { get; } = new();

        public Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request)
        {
            var dto = new PaymentDto
            {
                Id = Guid.NewGuid(),
                Reference = $"PAY-{DateTime.UtcNow:yyyyMMdd}-0001",
                Amount = request.Amount,
                Currency = request.Currency,
                CreatedAt = DateTime.UtcNow
            };
            Payments.Add(dto);
            return Task.FromResult(dto);
        }

        public Task<List<PaymentDto>> GetAllPaymentsAsync()
        {
            return Task.FromResult(Payments.ToList());
        }

        public Task<PaymentDto?> UpdatePaymentAsync(Guid id, UpdatePaymentRequest request)
        {
            var existing = Payments.FirstOrDefault(p => p.Id == id);
            if (existing == null) return Task.FromResult<PaymentDto?>(null);

            existing.Amount = request.Amount;
            existing.Currency = request.Currency;
            return Task.FromResult<PaymentDto?>(existing);
        }

        public Task<bool> DeletePaymentAsync(Guid id)
        {
            var existing = Payments.FirstOrDefault(p => p.Id == id);
            if (existing == null) return Task.FromResult(false);

            Payments.Remove(existing);
            return Task.FromResult(true);
        }
    }
}
