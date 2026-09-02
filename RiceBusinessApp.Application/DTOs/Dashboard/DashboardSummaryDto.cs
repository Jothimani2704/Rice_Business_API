using System;
using System.Collections.Generic;
using RiceBusinessApp.Application.DTOs.Sales;
using RiceBusinessApp.Application.DTOs.Payments;

namespace RiceBusinessApp.Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
        public decimal TodaysSalesAmount { get; set; }
        public decimal TodaysSalesQuantity { get; set; }
        public decimal TodaysPaymentCollection { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalAvailableStock { get; set; }
        public int LowStockCount { get; set; }
        
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
        public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
        
        public List<SaleResponseDto> RecentSales { get; set; } = new();
        public List<PaymentResponseDto> RecentPayments { get; set; } = new();
    }

    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal MinimumStockLevel { get; set; }
        public string StockStatus { get; set; } = string.Empty; // NORMAL, LOW_STOCK, OUT_OF_STOCK
    }

    public class RecentActivityDto
    {
        public string Type { get; set; } = string.Empty; // "SALE" or "PAYMENT"
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class TopSellingProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public decimal TotalQuantitySold { get; set; }
        public decimal TotalSalesAmount { get; set; }
    }
}
