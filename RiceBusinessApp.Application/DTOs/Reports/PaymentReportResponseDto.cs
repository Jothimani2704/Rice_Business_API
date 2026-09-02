using System;
using System.Collections.Generic;
using RiceBusinessApp.Application.DTOs.Payments;
namespace RiceBusinessApp.Application.DTOs.Reports { public class PaymentReportResponseDto { public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public int? CustomerIdFilter { get; set; } public string? PaymentModeFilter { get; set; } public decimal TotalCollection { get; set; } public List<PaymentResponseDto> Payments { get; set; } = new(); } }
