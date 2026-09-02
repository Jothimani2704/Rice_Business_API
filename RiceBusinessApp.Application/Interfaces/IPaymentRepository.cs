using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Domain.Entities;
namespace RiceBusinessApp.Application.Interfaces {
public interface IPaymentRepository {
Task<Payment?> GetByIdAsync(int id);
Task<IEnumerable<Payment>> GetAllAsync();
Task<Payment> AddAsync(Payment payment);
Task<IEnumerable<Payment>> GetByCustomerIdAsync(int customerId);
} }
