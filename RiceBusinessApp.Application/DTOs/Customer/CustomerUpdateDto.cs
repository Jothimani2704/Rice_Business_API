namespace RiceBusinessApp.Application.DTOs.Customer { 
    public class CustomerUpdateDto { 
        public string Name { get; set; } = string.Empty; 
        public string? Phone { get; set; } 
        public string? Address { get; set; } 
        public bool IsActive { get; set; }
    } 
}
