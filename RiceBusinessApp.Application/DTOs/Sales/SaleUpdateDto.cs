using System;
using System.Collections.Generic;

namespace RiceBusinessApp.Application.DTOs.Sales
{
    public class SaleUpdateDto
    {
        public List<SaleItemUpdateDto> SaleItems { get; set; } = new();
        public decimal PaidAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class SaleItemUpdateDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
    }
}
