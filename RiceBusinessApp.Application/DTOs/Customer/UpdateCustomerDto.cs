using System.ComponentModel.DataAnnotations;

namespace RiceBusinessApp.Application.DTOs.Customer
{
    public class UpdateCustomerDto
    {
        [Required(ErrorMessage = "Customer Name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? MobileNumber { get; set; }

        public string? Address { get; set; }
    }
}
