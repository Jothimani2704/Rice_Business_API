using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Domain.Entities;
namespace RiceBusinessApp.Application.Interfaces {
public interface ISaleRepository {
Task<Sale?> GetByIdAsync(int id);
Task<IEnumerable<Sale>> GetAllAsync();
Task<Sale> AddAsync(Sale sale);
Task UpdateAsync(Sale sale);
Task<IEnumerable<Sale>> GetByCustomerIdAsync(int customerId);
Task<IEnumerable<Sale>> GetByProductIdAsync(int productId);
} }
