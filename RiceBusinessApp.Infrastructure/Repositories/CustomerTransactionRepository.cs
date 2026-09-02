using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;
using RiceBusinessApp.Infrastructure.Data;
using System.Linq;
namespace RiceBusinessApp.Infrastructure.Repositories { public class CustomerTransactionRepository : ICustomerTransactionRepository { private readonly AppDbContext _context; public CustomerTransactionRepository(AppDbContext context) { _context = context; } public async Task<IEnumerable<CustomerTransaction>> GetByCustomerIdAsync(int customerId) => await _context.CustomerTransactions.Where(ct => ct.CustomerId == customerId).ToListAsync(); public async Task<CustomerTransaction> AddAsync(CustomerTransaction transaction) { _context.CustomerTransactions.Add(transaction); await _context.SaveChangesAsync(); return transaction; } } }
