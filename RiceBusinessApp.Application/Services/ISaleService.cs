using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Sales;

namespace RiceBusinessApp.Application.Services
{
    public interface ISaleService
    {
        Task<SaleResponseDto> CreateSaleAsync(SaleCreateDto request);
        Task<SaleResponseDto> GetSaleByIdAsync(int id);
        Task<IEnumerable<SaleListDto>> GetAllSalesAsync();
        Task<IEnumerable<SaleListDto>> GetSalesByCustomerIdAsync(int customerId);
        Task<IEnumerable<SaleListDto>> GetSalesByProductIdAsync(int productId);
        Task<SaleResponseDto> UpdateSaleAsync(int id, SaleUpdateDto request);
    }
}
