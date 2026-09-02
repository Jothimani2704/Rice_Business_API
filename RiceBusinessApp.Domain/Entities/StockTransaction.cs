using System;
using RiceBusinessApp.Domain.Enums;

namespace RiceBusinessApp.Domain.Entities
{
    public class StockTransaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        
        public StockTransactionType TransactionType { get; set; }
        public decimal Quantity { get; set; }
        
        public decimal PreviousStock { get; set; }
        public decimal NewStock { get; set; }
        
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        public string? ReferenceType { get; set; } // e.g., "SALE", "MANUAL_IN", "ADJUSTMENT"
        public int? ReferenceId { get; set; }
        
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
