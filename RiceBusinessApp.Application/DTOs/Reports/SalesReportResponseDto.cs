using System;
using System.Collections.Generic;
using RiceBusinessApp.Application.DTOs.Sales;
namespace RiceBusinessApp.Application.DTOs.Reports { public class SalesReportResponseDto { public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public int? CustomerIdFilter { get; set; } public int? ProductIdFilter { get; set; } public decimal TotalSalesAmount { get; set; } public decimal TotalQuantity { get; set; } public int NumberOfSales { get; set; } public List<SaleResponseDto> Sales { get; set; } = new(); } }
