using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;
using RiceBusinessApp.Infrastructure.Data;

namespace RiceBusinessApp.Infrastructure.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly AppDbContext _context;

        public SaleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Sale> AddAsync(Sale sale)
        {
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return sale;
        }

        public async Task UpdateAsync(Sale sale)
        {
            _context.Sales.Update(sale);
            await _context.SaveChangesAsync();
        }

        public async Task<Sale?> GetByIdAsync(int id)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Sale>> GetAllAsync()
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sale>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sale>> GetByProductIdAsync(int productId)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.SaleItems.Any(si => si.ProductId == productId))
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.CreatedDate)
                .ToListAsync();
        }
    }
}
