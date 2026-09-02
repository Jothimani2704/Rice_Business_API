using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Payments;

namespace RiceBusinessApp.Application.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreatePaymentAsync(PaymentCreateDto request);
        Task<PaymentResponseDto> GetPaymentByIdAsync(int id);
        Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync();
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByCustomerIdAsync(int customerId);
    }
}
