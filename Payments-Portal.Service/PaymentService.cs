using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payments_Portal.Data;

namespace Payments_Portal.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly IReferenceGenerator _referenceGenerator;
        private readonly IPaymentMapper _mapper;
        private readonly IPaymentValidator _validator;

        public PaymentService(
            IPaymentRepository repository,
            IReferenceGenerator referenceGenerator,
            IPaymentMapper mapper,
            IPaymentValidator validator)
        {
            _repository = repository;
            _referenceGenerator = referenceGenerator;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request)
        {
            var validation = _validator.ValidateCreate(request);
            if (!validation.IsValid)
            {
                throw new ArgumentException(validation.ErrorMessage);
            }

            // Prevent duplicates: same clientRequestId returns existing payment
            var existingPayment = await _repository.GetByClientRequestIdAsync(request.ClientRequestId);
            if (existingPayment != null)
            {
                return _mapper.ToDto(existingPayment);
            }

            string reference = await _referenceGenerator.GenerateAsync();

            var newPayment = new Payment
            {
                Id = Guid.NewGuid(),
                ClientRequestId = request.ClientRequestId,
                Amount = request.Amount,
                Currency = request.Currency,
                Reference = reference,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(newPayment);

            return _mapper.ToDto(newPayment);
        }

        public async Task<List<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _repository.GetAllAsync();
            return payments.OrderByDescending(p => p.CreatedAt).Select(_mapper.ToDto).ToList();
        }

        public async Task<PaymentDto?> UpdatePaymentAsync(Guid id, UpdatePaymentRequest request)
        {
            var validation = _validator.ValidateUpdate(request);
            if (!validation.IsValid)
            {
                throw new ArgumentException(validation.ErrorMessage);
            }

            var payment = await _repository.GetByIdAsync(id);
            if (payment == null)
            {
                return null;
            }

            payment.Amount = request.Amount;
            payment.Currency = request.Currency;

            await _repository.UpdateAsync(payment);
            return _mapper.ToDto(payment);
        }

        public async Task<bool> DeletePaymentAsync(Guid id)
        {
            var payment = await _repository.GetByIdAsync(id);
            if (payment == null)
            {
                return false;
            }

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}