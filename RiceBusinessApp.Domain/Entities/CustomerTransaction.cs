using System;
using RiceBusinessApp.Domain.Enums;

namespace RiceBusinessApp.Domain.Entities
{
    public class CustomerTransaction
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime TransactionDate { get; set; }
        
        public CustomerTransactionType TransactionType { get; set; }
        
        public decimal Amount { get; set; }
        
        public int? ReferenceId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
