using System;

namespace RiceBusinessApp.Application.DTOs.Ledger
{
    public class CustomerTransactionDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime TransactionDate { get; set; }
        
        public string TransactionType { get; set; } = string.Empty;
        public string ReferenceType { get; set; } = string.Empty;
        public int? ReferenceId { get; set; }
        public string? Description { get; set; }
        
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
        
        public DateTime CreatedDate { get; set; }
    }
}
