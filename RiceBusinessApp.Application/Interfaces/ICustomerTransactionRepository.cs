using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Domain.Entities;
namespace RiceBusinessApp.Application.Interfaces {
public interface ICustomerTransactionRepository {
Task<IEnumerable<CustomerTransaction>> GetByCustomerIdAsync(int customerId);
Task<CustomerTransaction> AddAsync(CustomerTransaction transaction);
} }
