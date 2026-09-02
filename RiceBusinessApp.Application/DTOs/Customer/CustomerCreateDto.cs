namespace RiceBusinessApp.Application.DTOs.Customer { 
    public class CustomerCreateDto { 
        public string Name { get; set; } = string.Empty; 
        public string? Phone { get; set; } 
        public string? Address { get; set; } 
        public decimal OpeningBalance { get; set; } 
        public bool IsActive { get; set; } = true;
    } 
}
