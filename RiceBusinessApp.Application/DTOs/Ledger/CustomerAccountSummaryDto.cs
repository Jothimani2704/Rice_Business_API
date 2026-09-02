using System;

namespace RiceBusinessApp.Application.DTOs.Ledger
{
    public class CustomerAccountSummaryDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal CurrentOutstandingBalance { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }
}
