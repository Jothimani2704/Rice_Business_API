using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;
using RiceBusinessApp.Infrastructure.Data;
namespace RiceBusinessApp.Infrastructure.Repositories { public class CustomerRepository : ICustomerRepository { private readonly AppDbContext _context; public CustomerRepository(AppDbContext context) { _context = context; } public async Task<Customer?> GetByIdAsync(int id) => await _context.Customers.FindAsync(id); public async Task<IEnumerable<Customer>> GetAllAsync() => await _context.Customers.ToListAsync(); public async Task<Customer> AddAsync(Customer customer) { _context.Customers.Add(customer); await _context.SaveChangesAsync(); return customer; } public async Task UpdateAsync(Customer customer) { _context.Customers.Update(customer); await _context.SaveChangesAsync(); } public async Task DeleteAsync(int id) { var customer = await GetByIdAsync(id); if (customer != null) { _context.Customers.Remove(customer); await _context.SaveChangesAsync(); } } } }
