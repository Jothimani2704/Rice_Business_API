using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.StockTransaction;

namespace RiceBusinessApp.Application.Services
{
    public interface IStockTransactionService
    {
        Task<IEnumerable<StockTransactionDto>> GetAllAsync();
        Task<StockTransactionDto> GetByIdAsync(int id);
        Task<StockTransactionDto> CreateAsync(CreateStockTransactionDto createDto);
        Task<StockTransactionDto> UpdateAsync(int id, UpdateStockTransactionDto updateDto);
        Task<IEnumerable<StockTransactionDto>> GetByProductIdAsync(int productId);
        Task<StockSummaryDto> GetStockSummaryByProductIdAsync(int productId);
    }
}
