using System;
using RiceBusinessApp.Domain.Enums;

namespace RiceBusinessApp.Application.DTOs.StockTransaction
{
    public class StockTransactionDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        
        public StockTransactionType TransactionType { get; set; }
        public decimal Quantity { get; set; }
        
        public decimal PreviousStock { get; set; }
        public decimal NewStock { get; set; }
        
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
        
        public DateTime CreatedDate { get; set; }
    }
}
