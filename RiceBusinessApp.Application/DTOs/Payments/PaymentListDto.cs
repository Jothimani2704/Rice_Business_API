using System;
namespace RiceBusinessApp.Application.DTOs.Payments { public class PaymentListDto { public int Id { get; set; } public int CustomerId { get; set; } public string CustomerName { get; set; } = string.Empty; public decimal Amount { get; set; } public DateTime PaymentDate { get; set; } public string PaymentMode { get; set; } = string.Empty; } }
