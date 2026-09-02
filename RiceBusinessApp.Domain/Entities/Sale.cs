using System;
using System.Collections.Generic;

namespace RiceBusinessApp.Domain.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime SaleDate { get; set; }
        
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        
        public string PaymentMode { get; set; } = string.Empty;
        public string? Notes { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
