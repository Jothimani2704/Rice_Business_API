using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Stock;
using RiceBusinessApp.Application.DTOs.Product;

namespace RiceBusinessApp.Application.Interfaces
{
    public interface IStockService
    {
        Task<StockTransactionDto> AddStockInAsync(StockInDto dto);
        Task<StockTransactionDto> AdjustStockAsync(StockAdjustmentDto dto);
        Task<IEnumerable<StockTransactionDto>> GetStockHistoryAsync(int productId);
        Task<IEnumerable<LowStockProductDto>> GetLowStockProductsAsync();
        Task<IEnumerable<ProductDto>> GetAllStockAsync();
    }
}
