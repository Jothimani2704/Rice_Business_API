namespace RiceBusinessApp.Application.DTOs.StockTransaction
{
    public class StockSummaryDto
    {
        public int ProductId { get; set; }
        public decimal TotalInward { get; set; }
        public decimal TotalOutward { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal StockValue { get; set; }
    }
}
