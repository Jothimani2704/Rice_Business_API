using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Domain.Entities;

namespace RiceBusinessApp.Application.Interfaces
{
    public interface IStockTransactionRepository
    {
        Task<StockTransaction> AddAsync(StockTransaction transaction);
        Task UpdateAsync(StockTransaction transaction);
        Task<StockTransaction?> GetByIdAsync(int id);
        Task<IEnumerable<StockTransaction>> GetAllAsync();
        Task<IEnumerable<StockTransaction>> GetHistoryByProductIdAsync(int productId);
    }
}
