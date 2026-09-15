using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;
using RiceBusinessApp.Infrastructure.Data;
using System.Linq;

namespace RiceBusinessApp.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByIdAsync(int id)
            => await _context.Payments.Include(p => p.Customer).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Payment>> GetAllAsync()
            => await _context.Payments.Include(p => p.Customer).OrderByDescending(p => p.PaymentDate).ToListAsync();

        public async Task<Payment> AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<IEnumerable<Payment>> GetByCustomerIdAsync(int customerId)
            => await _context.Payments.Include(p => p.Customer).Where(p => p.CustomerId == customerId).OrderByDescending(p => p.PaymentDate).ToListAsync();
    }
}
