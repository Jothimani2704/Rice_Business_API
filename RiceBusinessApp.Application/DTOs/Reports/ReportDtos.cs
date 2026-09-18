using System;
using System.Collections.Generic;

namespace RiceBusinessApp.Application.DTOs.Reports
{
    public class RevenueTrendDto
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal ProfitAmount { get; set; }
        public int SalesCount { get; set; }
    }

    public class PaymentModeBreakdownDto
    {
        public string PaymentMode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public double Percentage { get; set; }
    }

    public class FinancialAnalyticsSummaryDto
    {
        public string Period { get; set; } = "daily";
        public decimal TotalRevenue { get; set; }
        public decimal TotalCollections { get; set; }
        public decimal TotalOutstandingReceivables { get; set; }
        public decimal GrossProfit { get; set; }
        public double ProfitMarginPercentage { get; set; }
        public int TotalSalesCount { get; set; }
        public List<RevenueTrendDto> RevenueTrends { get; set; } = new List<RevenueTrendDto>();
        public List<PaymentModeBreakdownDto> PaymentBreakdown { get; set; } = new List<PaymentModeBreakdownDto>();
    }
}
