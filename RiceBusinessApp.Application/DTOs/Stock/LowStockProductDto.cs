namespace RiceBusinessApp.Application.DTOs.Stock
{
    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal MinimumStockLevel { get; set; }
    }
}
