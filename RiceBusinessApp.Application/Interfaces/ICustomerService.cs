using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Customer;
namespace RiceBusinessApp.Application.Interfaces {
public interface ICustomerService {
Task<CustomerResponseDto?> GetCustomerByIdAsync(int id);
Task<IEnumerable<CustomerListDto>> GetAllCustomersAsync();
Task<CustomerResponseDto> CreateCustomerAsync(CustomerCreateDto dto);
Task<CustomerResponseDto> UpdateCustomerAsync(int id, CustomerUpdateDto dto);
} }
