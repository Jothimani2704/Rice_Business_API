using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Stock;
using RiceBusinessApp.Application.DTOs.Product;
using RiceBusinessApp.Application.Interfaces;
namespace RiceBusinessApp.Application.Services { public class StockService : IStockService { public async Task<StockTransactionDto> AddStockInAsync(StockInDto dto) => new StockTransactionDto(); public async Task<StockTransactionDto> AdjustStockAsync(StockAdjustmentDto dto) => new StockTransactionDto(); public async Task<IEnumerable<StockTransactionDto>> GetStockHistoryAsync(int productId) => new List<StockTransactionDto>(); public async Task<IEnumerable<LowStockProductDto>> GetLowStockProductsAsync() => new List<LowStockProductDto>(); public async Task<IEnumerable<ProductDto>> GetAllStockAsync() => new List<ProductDto>(); } }
