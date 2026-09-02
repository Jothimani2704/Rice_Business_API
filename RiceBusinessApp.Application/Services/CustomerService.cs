using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Customer;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;

namespace RiceBusinessApp.Application.Services { 
    public class CustomerService : ICustomerService { 
        private readonly ICustomerRepository _repo;

        public CustomerService(ICustomerRepository repo) {
            _repo = repo;
        }

        public async Task<CustomerResponseDto?> GetCustomerByIdAsync(int id) {
            var customer = await _repo.GetByIdAsync(id);
            if (customer == null) return null;
            return new CustomerResponseDto {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.MobileNumber,
                Address = customer.Address,
                OpeningBalance = customer.OpeningBalance,
                CurrentBalance = customer.CurrentBalance,
                IsActive = customer.IsActive
            };
        }
        
        public async Task<IEnumerable<CustomerListDto>> GetAllCustomersAsync() {
            var customers = await _repo.GetAllAsync();
            return customers.OrderByDescending(c => c.Id).Select(c => new CustomerListDto {
                Id = c.Id,
                Name = c.Name,
                Phone = c.MobileNumber,
                OutstandingBalance = c.CurrentBalance,
                IsActive = c.IsActive,
                LastActivity = "Last activity: Unknown" // Mocked string for now
            }).ToList();
        }
        
        public async Task<CustomerResponseDto> CreateCustomerAsync(CustomerCreateDto dto) {
            var customer = new Customer {
                Name = dto.Name,
                MobileNumber = dto.Phone,
                Address = dto.Address,
                OpeningBalance = dto.OpeningBalance,
                CurrentBalance = dto.OpeningBalance,
                IsActive = dto.IsActive
            };
            
            var created = await _repo.AddAsync(customer);
            
            return new CustomerResponseDto {
                Id = created.Id,
                Name = created.Name,
                Phone = created.MobileNumber,
                Address = created.Address,
                OpeningBalance = created.OpeningBalance,
                CurrentBalance = created.CurrentBalance,
                IsActive = created.IsActive
            };
        }
        
        public async Task<CustomerResponseDto> UpdateCustomerAsync(int id, CustomerUpdateDto dto) {
            var customer = await _repo.GetByIdAsync(id);
            if (customer == null) return null;

            customer.Name = dto.Name;
            customer.MobileNumber = dto.Phone;
            customer.Address = dto.Address;
            customer.IsActive = dto.IsActive;

            await _repo.UpdateAsync(customer);

            return new CustomerResponseDto {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.MobileNumber,
                Address = customer.Address,
                OpeningBalance = customer.OpeningBalance,
                CurrentBalance = customer.CurrentBalance,
                IsActive = customer.IsActive
            };
        }
    } 
}
