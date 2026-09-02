using System;
using RiceBusinessApp.Domain.Enums;

namespace RiceBusinessApp.Application.DTOs.StockTransaction
{
    public class CreateStockTransactionDto
    {
        public int ProductId { get; set; }
        public StockTransactionType TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
    }
}
