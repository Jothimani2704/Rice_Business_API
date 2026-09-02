using System;
using System.Collections.Generic;

namespace RiceBusinessApp.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? MobileNumber { get; set; }
        public string? Address { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<CustomerTransaction> Transactions { get; set; } = new List<CustomerTransaction>();
    }
}
