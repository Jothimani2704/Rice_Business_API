namespace RiceBusinessApp.Application.DTOs.Customer { 
    public class CustomerListDto { 
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty; 
        public string? Phone { get; set; } 
        public decimal OutstandingBalance { get; set; } 
        public bool IsActive { get; set; }
        public string? LastActivity { get; set; }
    } 
}
