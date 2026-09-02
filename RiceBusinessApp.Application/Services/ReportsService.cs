using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Reports;
using RiceBusinessApp.Application.Interfaces;
namespace RiceBusinessApp.Application.Services { public class ReportsService : IReportsService { public async Task<SalesReportResponseDto> GetSalesReportAsync(DateTime startDate, DateTime endDate, int? customerIdFilter, int? productIdFilter) => new SalesReportResponseDto(); public async Task<PaymentReportResponseDto> GetPaymentReportAsync(DateTime startDate, DateTime endDate, int? customerIdFilter, string paymentModeFilter) => new PaymentReportResponseDto(); public async Task<List<CustomerBalanceReportItemDto>> GetCustomerBalancesAsync() => new List<CustomerBalanceReportItemDto>(); public async Task<List<StockReportItemDto>> GetStockReportAsync() => new List<StockReportItemDto>(); public async Task<List<ProductSalesReportItemDto>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate) => new List<ProductSalesReportItemDto>(); } }
