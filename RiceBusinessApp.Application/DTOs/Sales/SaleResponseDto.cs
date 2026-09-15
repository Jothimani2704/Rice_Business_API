using System;
using System.Collections.Generic;

namespace RiceBusinessApp.Application.DTOs.Sales
{
    public class SaleResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public decimal CustomerCurrentBalance { get; set; }
        public decimal PreviousBalance { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<SaleItemResponseDto> SaleItems { get; set; } = new();
    }

    public class SaleItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal BagSize { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}
