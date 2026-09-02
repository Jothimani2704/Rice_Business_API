using System;
namespace RiceBusinessApp.Application.DTOs.Stock { public class StockTransactionDto { public int Id { get; set; } public int ProductId { get; set; } public string TransactionType { get; set; } = string.Empty; public decimal Quantity { get; set; } public DateTime TransactionDate { get; set; } public string? Remarks { get; set; } } }
