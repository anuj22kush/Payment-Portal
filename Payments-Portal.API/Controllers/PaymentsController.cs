using Microsoft.AspNetCore.Mvc;
using Payments_Portal.Service;

namespace Payments_Portal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var payment = await _paymentService.CreatePaymentAsync(request);
            return CreatedAtAction(nameof(GetPayments), new { id = payment.Id }, payment);
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] UpdatePaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var payment = await _paymentService.UpdatePaymentAsync(id, request);
            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(Guid id)
        {
            var result = await _paymentService.DeletePaymentAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}