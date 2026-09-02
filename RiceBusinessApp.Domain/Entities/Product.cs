using System;
using System.Collections.Generic;

namespace RiceBusinessApp.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal BagSize { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MinimumStockLevel { get; set; }
        public decimal CurrentStock { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}
