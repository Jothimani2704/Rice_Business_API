using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Reports;

namespace RiceBusinessApp.Application.Interfaces
{
    public interface IReportsRepository
    {
        Task<SalesReportResponseDto> GetSalesReportAsync(DateTime startDate, DateTime endDate, int? customerIdFilter, int? productIdFilter);
        Task<PaymentReportResponseDto> GetPaymentReportAsync(DateTime startDate, DateTime endDate, int? customerIdFilter, string paymentModeFilter);
        Task<List<CustomerBalanceReportItemDto>> GetCustomerBalancesAsync();
        Task<List<StockReportItemDto>> GetStockReportAsync();
        Task<List<ProductSalesReportItemDto>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate);
    }
}
