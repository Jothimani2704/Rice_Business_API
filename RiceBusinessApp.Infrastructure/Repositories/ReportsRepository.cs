using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiceBusinessApp.Application.DTOs.Reports;
using RiceBusinessApp.Application.Interfaces;
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

        public async Task<List<RevenueTrendDto>> GetRevenueTrendsAsync(string period)
        {
            period = string.IsNullOrWhiteSpace(period) ? "daily" : period.ToLower();
            var now = DateTime.Today;
            var trends = new List<RevenueTrendDto>();

            if (period == "weekly")
            {
                // Last 8 weeks
                for (int i = 7; i >= 0; i--)
                {
                    var weekStart = now.AddDays(-((int)now.DayOfWeek == 0 ? 6 : (int)now.DayOfWeek - 1)).AddDays(-i * 7);
                    var weekEnd = weekStart.AddDays(7);

                    var sales = await _context.Sales
                        .Where(s => s.SaleDate >= weekStart && s.SaleDate < weekEnd)
                        .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                        .ToListAsync();

                    var payments = await _context.Payments
                        .Where(p => p.PaymentDate >= weekStart && p.PaymentDate < weekEnd)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                    decimal salesAmt = sales.Sum(s => s.TotalAmount);
                    decimal profit = sales.SelectMany(s => s.SaleItems)
                        .Sum(si => (si.Rate - (si.Product != null ? si.Product.PurchasePrice : 0m)) * si.Quantity);

                    trends.Add(new RevenueTrendDto
                    {
                        Label = $"{weekStart:dd MMM}",
                        Date = weekStart,
                        SalesAmount = salesAmt,
                        PaymentAmount = payments,
                        ProfitAmount = profit,
                        SalesCount = sales.Count
                    });
                }
            }
            else if (period == "monthly")
            {
                // Last 6 months
                for (int i = 5; i >= 0; i--)
                {
                    var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                    var monthEnd = monthStart.AddMonths(1);

                    var sales = await _context.Sales
                        .Where(s => s.SaleDate >= monthStart && s.SaleDate < monthEnd)
                        .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                        .ToListAsync();

                    var payments = await _context.Payments
                        .Where(p => p.PaymentDate >= monthStart && p.PaymentDate < monthEnd)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                    decimal salesAmt = sales.Sum(s => s.TotalAmount);
                    decimal profit = sales.SelectMany(s => s.SaleItems)
                        .Sum(si => (si.Rate - (si.Product != null ? si.Product.PurchasePrice : 0m)) * si.Quantity);

                    trends.Add(new RevenueTrendDto
                    {
                        Label = monthStart.ToString("MMM yyyy"),
                        Date = monthStart,
                        SalesAmount = salesAmt,
                        PaymentAmount = payments,
                        ProfitAmount = profit,
                        SalesCount = sales.Count
                    });
                }
            }
            else
            {
                // Daily: Last 7 days
                for (int i = 6; i >= 0; i--)
                {
                    var day = now.AddDays(-i);
                    var nextDay = day.AddDays(1);

                    var sales = await _context.Sales
                        .Where(s => s.SaleDate >= day && s.SaleDate < nextDay)
                        .Include(s => s.SaleItems)
                        .ThenInclude(si => si.Product)
                        .ToListAsync();

                    var payments = await _context.Payments
                        .Where(p => p.PaymentDate >= day && p.PaymentDate < nextDay)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                    decimal salesAmt = sales.Sum(s => s.TotalAmount);
                    decimal profit = sales.SelectMany(s => s.SaleItems)
                        .Sum(si => (si.Rate - (si.Product != null ? si.Product.PurchasePrice : 0m)) * si.Quantity);

                    trends.Add(new RevenueTrendDto
                    {
                        Label = i == 0 ? "Today" : day.ToString("ddd dd"),
                        Date = day,
                        SalesAmount = salesAmt,
                        PaymentAmount = payments,
                        ProfitAmount = profit,
                        SalesCount = sales.Count
                    });
                }
            }

            return trends;
        }

        public async Task<FinancialAnalyticsSummaryDto> GetFinancialAnalyticsSummaryAsync(string period)
        {
            period = string.IsNullOrWhiteSpace(period) ? "daily" : period.ToLower();
            var trends = await GetRevenueTrendsAsync(period);

            var totalRevenue = trends.Sum(t => t.SalesAmount);
            var totalCollections = trends.Sum(t => t.PaymentAmount);
            var grossProfit = trends.Sum(t => t.ProfitAmount);
            var salesCount = trends.Sum(t => t.SalesCount);

            var totalOutstanding = await _context.Customers
                .Where(c => c.IsActive)
                .SumAsync(c => (decimal?)c.CurrentBalance) ?? 0m;

            double margin = totalRevenue > 0 ? (double)(grossProfit / totalRevenue * 100m) : 0.0;

            // Payment Mode Breakdown for the period
            DateTime startDate = trends.FirstOrDefault()?.Date ?? DateTime.Today.AddDays(-7);
            var paymentsInPeriod = await _context.Payments
                .Where(p => p.PaymentDate >= startDate)
                .ToListAsync();

            var paymentGroups = paymentsInPeriod
                .GroupBy(p => string.IsNullOrWhiteSpace(p.PaymentMode) ? "Cash" : p.PaymentMode)
                .Select(g => new
                {
                    Mode = g.Key,
                    Total = g.Sum(p => p.Amount),
                    Count = g.Count()
                })
                .ToList();

            decimal totalPaymentsInPeriod = paymentGroups.Sum(g => g.Total);
            var breakdown = paymentGroups.Select(g => new PaymentModeBreakdownDto
            {
                PaymentMode = g.Mode,
                TotalAmount = g.Total,
                TransactionCount = g.Count,
                Percentage = totalPaymentsInPeriod > 0 ? (double)(g.Total / totalPaymentsInPeriod * 100m) : 0.0
            }).OrderByDescending(b => b.TotalAmount).ToList();

            return new FinancialAnalyticsSummaryDto
            {
                Period = period,
                TotalRevenue = totalRevenue,
                TotalCollections = totalCollections,
                TotalOutstandingReceivables = totalOutstanding,
                GrossProfit = grossProfit,
                ProfitMarginPercentage = Math.Round(margin, 1),
                TotalSalesCount = salesCount,
                RevenueTrends = trends,
                PaymentBreakdown = breakdown
            };
        }
    }
}
