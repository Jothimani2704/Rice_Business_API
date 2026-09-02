using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Payments;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;
using RiceBusinessApp.Domain.Enums;

namespace RiceBusinessApp.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerTransactionRepository _customerTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ICustomerRepository customerRepository,
            ICustomerTransactionRepository customerTransactionRepository,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _customerRepository = customerRepository;
            _customerTransactionRepository = customerTransactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(PaymentCreateDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Validate Customer exists and is active
                var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
                if (customer == null || !customer.IsActive)
                    throw new Exception("Invalid or inactive customer.");

                // 2. Validate Payment Amount
                if (request.Amount <= 0)
                    throw new Exception("Amount must be greater than zero.");
                
                // Do not allow negative payments or zero payments. Handled by <= 0 check.
                
                // 3. Balance verification
                // Do not allow payment greater than outstanding balance unless explicitly supported (rule: no advance payment)
                if (request.Amount > customer.CurrentBalance)
                    throw new Exception($"Payment amount ({request.Amount}) cannot exceed the outstanding balance ({customer.CurrentBalance}).");

                var paymentDate = request.PaymentDate ?? DateTime.UtcNow;

                var payment = new Payment
                {
                    CustomerId = customer.Id,
                    Amount = request.Amount,
                    PaymentMode = request.PaymentMode,
                    PaymentDate = paymentDate,
                    ReferenceNumber = request.ReferenceNumber,
                    Notes = request.Notes,
                    PreviousBalance = customer.CurrentBalance,
                    NewBalance = customer.CurrentBalance - request.Amount
                };

                // Create Payment
                await _paymentRepository.AddAsync(payment);

                // Update Customer Balance
                customer.CurrentBalance -= request.Amount; // Reduce debt
                await _customerRepository.UpdateAsync(customer);

                // Create CustomerTransaction representing the payment
                var customerTx = new CustomerTransaction
                {
                    CustomerId = customer.Id,
                    TransactionDate = paymentDate,
                    TransactionType = CustomerTransactionType.Payment,
                    Amount = request.Amount,
                    ReferenceId = payment.Id,
                    Notes = string.IsNullOrEmpty(payment.Notes) 
                            ? $"Payment #{payment.Id} ({payment.PaymentMode})" 
                            : payment.Notes
                };
                await _customerTransactionRepository.AddAsync(customerTx);

                await _unitOfWork.CommitTransactionAsync();

                return await GetPaymentByIdAsync(payment.Id);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<PaymentResponseDto> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null) throw new Exception("Payment not found.");
            return MapToResponseDto(payment);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return payments.OrderByDescending(p => p.Id).Select(MapToResponseDto);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByCustomerIdAsync(int customerId)
        {
            var payments = await _paymentRepository.GetByCustomerIdAsync(customerId);
            return payments.Select(MapToResponseDto);
        }

        private PaymentResponseDto MapToResponseDto(Payment payment)
        {
            return new PaymentResponseDto
            {
                Id = payment.Id,
                CustomerId = payment.CustomerId,
                CustomerName = payment.Customer?.Name ?? string.Empty,
                Amount = payment.Amount,
                PreviousBalance = payment.PreviousBalance,
                NewBalance = payment.NewBalance,
                PaymentMode = payment.PaymentMode,
                PaymentDate = payment.PaymentDate,
                ReferenceNumber = payment.ReferenceNumber,
                Notes = payment.Notes,
                CreatedDate = payment.CreatedDate,
                UpdatedDate = payment.UpdatedDate
            };
        }
    }
}
