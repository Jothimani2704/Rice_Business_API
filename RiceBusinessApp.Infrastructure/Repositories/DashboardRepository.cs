using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiceBusinessApp.Application.DTOs.Dashboard;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Infrastructure.Data;

namespace RiceBusinessApp.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var activeCustomers = await _context.Customers
                .Where(c => c.IsActive)
                .ToListAsync();

            var todaysSales = await _context.Sales
                .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow)
                .Select(s => new { s.TotalAmount, TotalQuantity = s.SaleItems.Sum(si => si.Quantity) })
                .ToListAsync();

            var todaysPayments = await _context.Payments
                .Where(p => p.PaymentDate >= today && p.PaymentDate < tomorrow)
                .SumAsync(p => p.Amount);

            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            var lowStockProducts = products
                .Where(p => p.CurrentStock <= p.MinimumStockLevel)
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.Id,
                    ProductName = p.ProductName,
                    BrandName = p.BrandName,
                    CurrentStock = p.CurrentStock,
                    MinimumStockLevel = p.MinimumStockLevel,
                    StockStatus = p.CurrentStock == 0 ? "OUT_OF_STOCK" : "LOW_STOCK"
                })
                .ToList();

            var recentSales = await _context.Sales
                .Include(s => s.Customer)
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .Select(s => new RecentActivityDto
                {
                    Type = "SALE",
                    Id = s.Id,
                    CustomerName = s.Customer.Name,
                    Date = s.SaleDate,
                    Amount = s.TotalAmount,
                    Description = $"Sale of {s.SaleItems.Sum(si => si.Quantity)} items"
                })
                .ToListAsync();

            var recentPayments = await _context.Payments
                .Include(p => p.Customer)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new RecentActivityDto
                {
                    Type = "PAYMENT",
                    Id = p.Id,
                    CustomerName = p.Customer.Name,
                    Date = p.PaymentDate,
                    Amount = p.Amount,
                    Description = $"Payment via {p.PaymentMode}"
                })
                .ToListAsync();

            var recentActivity = recentSales.Concat(recentPayments)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToList();

            // Calculate top selling products (last 30 days)
            var thirtyDaysAgo = today.AddDays(-30);
            var topProducts = await _context.SaleItems
                .Include(si => si.Sale)
                .Include(si => si.Product)
                .Where(si => si.Sale.SaleDate >= thirtyDaysAgo)
                .GroupBy(si => new { si.ProductId, si.Product.ProductName, si.Product.BrandName })
                .Select(g => new TopSellingProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    BrandName = g.Key.BrandName,
                    TotalQuantitySold = g.Sum(si => si.Quantity),
                    TotalSalesAmount = g.Sum(si => si.Quantity * si.Rate)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(5)
                .ToListAsync();

            return new DashboardSummaryDto
            {
                TotalCustomers = await _context.Customers.CountAsync(),
                ActiveCustomers = activeCustomers.Count,
                TotalOutstandingBalance = activeCustomers.Sum(c => c.CurrentBalance),
                TodaysSalesAmount = todaysSales.Sum(s => s.TotalAmount),
                TodaysSalesQuantity = todaysSales.Sum(s => s.TotalQuantity),
                TodaysPaymentCollection = todaysPayments,
                TotalProducts = products.Count,
                TotalAvailableStock = products.Sum(p => p.CurrentStock),
                LowStockCount = lowStockProducts.Count,
                LowStockProducts = lowStockProducts,
                RecentActivity = recentActivity,
                TopSellingProducts = topProducts,
                // Optional: mapping to actual recent sale/payment models if UI needs them explicitly
            };
        }
    }
}
