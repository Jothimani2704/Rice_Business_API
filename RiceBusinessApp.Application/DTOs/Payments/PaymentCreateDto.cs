using System;
namespace RiceBusinessApp.Application.DTOs.Payments { public class PaymentCreateDto { public int CustomerId { get; set; } public decimal Amount { get; set; } public DateTime? PaymentDate { get; set; } public string PaymentMode { get; set; } = string.Empty; public string? ReferenceNumber { get; set; } public string? Notes { get; set; } } }
