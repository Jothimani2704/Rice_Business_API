using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Domain.Entities;
namespace RiceBusinessApp.Application.Interfaces {
public interface ICustomerRepository {
Task<Customer?> GetByIdAsync(int id);
Task<IEnumerable<Customer>> GetAllAsync();
Task<Customer> AddAsync(Customer customer);
Task UpdateAsync(Customer customer);
Task DeleteAsync(int id);
} }
