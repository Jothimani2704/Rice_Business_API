using System;
using System.Collections.Generic;
namespace RiceBusinessApp.Application.DTOs.Sales { public class SaleCreateDto { public int CustomerId { get; set; } public DateTime? SaleDate { get; set; } public decimal PaidAmount { get; set; } public string PaymentMode { get; set; } = string.Empty; public string? Notes { get; set; } public List<SaleItemCreateDto> SaleItems { get; set; } = new(); } public class SaleItemCreateDto { public int ProductId { get; set; } public decimal Quantity { get; set; } public decimal Rate { get; set; } } }
