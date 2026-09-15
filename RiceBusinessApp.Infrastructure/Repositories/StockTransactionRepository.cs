using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;
using RiceBusinessApp.Infrastructure.Data;
using System.Linq;

namespace RiceBusinessApp.Infrastructure.Repositories 
{ 
    public class StockTransactionRepository : IStockTransactionRepository 
    { 
        private readonly AppDbContext _context; 
        
        public StockTransactionRepository(AppDbContext context) 
        { 
            _context = context; 
        } 
        
        public async Task<IEnumerable<StockTransaction>> GetAllAsync()
        {
            return await _context.StockTransactions
                .Include(st => st.Product)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<StockTransaction?> GetByIdAsync(int id)
        {
            return await _context.StockTransactions
                .Include(st => st.Product)
                .FirstOrDefaultAsync(st => st.Id == id);
        }

        public async Task<IEnumerable<StockTransaction>> GetHistoryByProductIdAsync(int productId) 
        {
            return await _context.StockTransactions
                .Where(st => st.ProductId == productId)
                .OrderByDescending(st => st.TransactionDate)
                .ThenByDescending(st => st.Id)
                .ToListAsync();
        }

        public async Task<StockTransaction> AddAsync(StockTransaction transaction) 
        { 
            _context.StockTransactions.Add(transaction); 
            await _context.SaveChangesAsync(); 
            return transaction; 
        } 
        
        public async Task UpdateAsync(StockTransaction transaction)
        {
            _context.StockTransactions.Update(transaction);
            await _context.SaveChangesAsync();
        }
    } 
}
