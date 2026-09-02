using System;

namespace RiceBusinessApp.Application.DTOs.StockTransaction
{
    public class UpdateStockTransactionDto
    {
        public decimal Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
    }
}
