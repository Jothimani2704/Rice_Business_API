using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Domain.Entities;

namespace RiceBusinessApp.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync(bool activeOnly = false);
        Task<IEnumerable<Product>> SearchAsync(string query);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task<bool> ExistsByNameAndBrandAsync(string productName, string brandName);
        Task<bool> IsReferencedAsync(int productId);
    }
}
