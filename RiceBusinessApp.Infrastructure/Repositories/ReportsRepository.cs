using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiceBusinessApp.Application.DTOs.Reports;
using RiceBusinessApp.Application.DTOs.Sales;
using RiceBusinessApp.Application.DTOs.Payments;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Enums;
using RiceBusinessApp.Infrastructure.Data;

namespace RiceBusinessApp.Infrastructure.Repositories
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly AppDbContext _context;

        public ReportsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SalesReportResponseDto> GetSalesReportAsync(DateTime startDate, DateTime endDate, int? customerIdFilter, int? productIdFilter)
        {
            // Normalize endDate to be inclusive
            var actualEndDate = endDate.Date.AddDays(1);
            
            var query = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Where(s => s.SaleDate >= startDate.Date && s.SaleDate < actualEndDate);

            if (customerIdFilter.HasValue)
            {
                query = query.Where(s => s.CustomerId == customerIdFilter.Value);
            }

            if (productIdFilter.HasValue)
            {
                query = query.Where(s => s.SaleItems.Any(si => si.ProductId == productIdFilter.Value));
            }

            var sales = await query.OrderByDescending(s => s.SaleDate).ToListAsync();

            return new SalesReportResponseDto
            {
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                CustomerIdFilter = customerIdFilter,
                ProductIdFilter = productIdFilter,
                TotalSalesAmount = sales.Sum(s => s.TotalAmount),
                TotalQuantity = sales.Sum(s => s.SaleItems.Sum(si => si.Quantity)),
                NumberOfSales = sales.Count,
                Sales = sales.Select(s => new SaleResponseDto
                {
                    Id = s.Id,
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer.Name,
                    SaleDate = s.SaleDate,
                    TotalAmount = s.TotalAmount,
                    PaidAmount = s.PaidAmount,
                    PaymentMode = s.PaymentMode,
                    Notes = s.Notes,
                    SaleItems = s.SaleItems.Select(si => new SaleItemResponseDto
                    {
                        Id = si.Id,
                        ProductId = si.ProductId,
                        ProductName = si.Product.ProductName,
                        Quantity = si.Quantity,
                        Rate = si.Rate,
                        Amount = si.Amount
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<PaymentReportResponseDto> GetPaymentReportAsync(DateTime startDate, DateTime endDate, int? customerIdFilter, string paymentModeFilter)
        {
            var actualEndDate = endDate.Date.AddDays(1);
            
            var query = _context.Payments
                .Include(p => p.Customer)
                .Where(p => p.PaymentDate >= startDate.Date && p.PaymentDate < actualEndDate);

            if (customerIdFilter.HasValue)
            {
                query = query.Where(p => p.CustomerId == customerIdFilter.Value);
            }

            if (!string.IsNullOrEmpty(paymentModeFilter))
            {
                query = query.Where(p => p.PaymentMode == paymentModeFilter);
            }

            var payments = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();

            return new PaymentReportResponseDto
            {
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                CustomerIdFilter = customerIdFilter,
                PaymentModeFilter = paymentModeFilter,
                TotalCollection = payments.Sum(p => p.Amount),
                Payments = payments.Select(p => new PaymentResponseDto
                {
                    Id = p.Id,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.Name,
                    Amount = p.Amount,
                    PaymentMode = p.PaymentMode,
                    PaymentDate = p.PaymentDate,
                    ReferenceNumber = p.ReferenceNumber,
                    Notes = p.Notes
                }).ToList()
            };
        }

        public async Task<List<CustomerBalanceReportItemDto>> GetCustomerBalancesAsync()
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.MobileNumber,
                    c.CurrentBalance,
                    TotalSales = _context.Sales.Where(s => s.CustomerId == c.Id).Sum(s => (decimal?)s.TotalAmount) ?? 0,
                    TotalPayments = _context.Payments.Where(p => p.CustomerId == c.Id).Sum(p => (decimal?)p.Amount) ?? 0,
                    LastTransactionDate = _context.CustomerTransactions
                        .Where(ct => ct.CustomerId == c.Id)
                        .OrderByDescending(ct => ct.TransactionDate)
                        .Select(ct => (DateTime?)ct.TransactionDate)
                        .FirstOrDefault()
                })
                .OrderByDescending(c => c.CurrentBalance)
                .ToListAsync();

            return customers.Select(c => new CustomerBalanceReportItemDto
            {
                CustomerId = c.Id,
                CustomerName = c.Name,
                PhoneNumber = c.MobileNumber ?? string.Empty,
                TotalSales = c.TotalSales,
                TotalPayments = c.TotalPayments,
                OutstandingBalance = c.CurrentBalance,
                LastTransactionDate = c.LastTransactionDate
            }).ToList();
        }

        public async Task<List<StockReportItemDto>> GetStockReportAsync()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.BrandName,
                    p.CurrentStock,
                    p.MinimumStockLevel,
                    TotalStockIn = _context.StockTransactions
                        .Where(st => st.ProductId == p.Id && st.TransactionType == StockTransactionType.In)
                        .Sum(st => (decimal?)st.Quantity) ?? 0,
                    TotalStockOut = _context.StockTransactions
                        .Where(st => st.ProductId == p.Id && st.TransactionType == StockTransactionType.Out)
                        .Sum(st => (decimal?)st.Quantity) ?? 0
                })
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return products.Select(p => new StockReportItemDto
            {
                ProductId = p.Id,
                ProductName = p.ProductName,
                BrandName = p.BrandName,
                CurrentStock = p.CurrentStock,
                MinimumStockLevel = p.MinimumStockLevel,
                StockStatus = p.CurrentStock == 0 ? "OUT_OF_STOCK" : 
                              p.CurrentStock <= p.MinimumStockLevel ? "LOW_STOCK" : "NORMAL",
                TotalStockIn = p.TotalStockIn,
                TotalStockOut = p.TotalStockOut
            }).ToList();
        }

        public async Task<List<ProductSalesReportItemDto>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var actualEndDate = endDate.Date.AddDays(1);
            
            var salesItems = await _context.SaleItems
                .Include(si => si.Product)
                .Include(si => si.Sale)
                .Where(si => si.Sale.SaleDate >= startDate.Date && si.Sale.SaleDate < actualEndDate)
                .GroupBy(si => new { si.ProductId, si.Product.ProductName, si.Product.BrandName })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    BrandName = g.Key.BrandName,
                    QuantitySold = g.Sum(si => si.Quantity),
                    SalesAmount = g.Sum(si => si.Quantity * si.Rate),
                    CustomerIds = g.Select(si => si.Sale.CustomerId).Distinct()
                })
                .ToListAsync();

            return salesItems.Select(s => new ProductSalesReportItemDto
            {
                ProductId = s.ProductId,
                ProductName = s.ProductName,
                BrandName = s.BrandName,
                QuantitySold = s.QuantitySold,
                SalesAmount = s.SalesAmount,
                NumberOfCustomersPurchased = s.CustomerIds.Count()
            })
            .OrderByDescending(s => s.SalesAmount)
            .ToList();
        }
    }
}
