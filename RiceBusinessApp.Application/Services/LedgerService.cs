using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Ledger;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Enums;

namespace RiceBusinessApp.Application.Services
{
    public class LedgerService : ILedgerService
    {
        private readonly ICustomerTransactionRepository _transactionRepository;
        private readonly ICustomerRepository _customerRepository;

        public LedgerService(ICustomerTransactionRepository transactionRepository, ICustomerRepository customerRepository)
        {
            _transactionRepository = transactionRepository;
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<CustomerTransactionDto>> GetCustomerTransactionsAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new Exception("Customer not found.");
            }

            var transactions = await _transactionRepository.GetByCustomerIdAsync(customerId);
            // Ensure chronological order for running balance calculation
            transactions = transactions.OrderBy(t => t.TransactionDate).ThenBy(t => t.CreatedDate).ToList();

            var result = new List<CustomerTransactionDto>();
            decimal runningBalance = customer.OpeningBalance;

            foreach (var tx in transactions)
            {
                decimal debit = 0;
                decimal credit = 0;

                if (tx.TransactionType == CustomerTransactionType.Sale)
                {
                    debit = tx.Amount;
                    runningBalance += debit;
                }
                else if (tx.TransactionType == CustomerTransactionType.Payment)
                {
                    credit = tx.Amount;
                    runningBalance -= credit;
                }

                result.Add(new CustomerTransactionDto
                {
                    Id = tx.Id,
                    CustomerId = tx.CustomerId,
                    TransactionDate = tx.TransactionDate,
                    TransactionType = tx.TransactionType.ToString(),
                    ReferenceType = tx.TransactionType.ToString().ToUpper(),
                    ReferenceId = tx.ReferenceId,
                    Description = tx.Notes,
                    Debit = debit,
                    Credit = credit,
                    RunningBalance = runningBalance,
                    CreatedDate = tx.CreatedDate
                });
            }

            // Return latest first
            return result.OrderByDescending(r => r.TransactionDate).ThenByDescending(r => r.CreatedDate);
        }

        public async Task<CustomerAccountSummaryDto> GetCustomerAccountSummaryAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new Exception("Customer not found.");
            }

            var transactions = await _transactionRepository.GetByCustomerIdAsync(customerId);

            decimal totalSales = transactions.Where(t => t.TransactionType == CustomerTransactionType.Sale).Sum(t => t.Amount);
            decimal totalPayments = transactions.Where(t => t.TransactionType == CustomerTransactionType.Payment).Sum(t => t.Amount);
            
            var lastTx = transactions.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedDate).FirstOrDefault();

            return new CustomerAccountSummaryDto
            {
                CustomerName = customer.Name,
                OpeningBalance = customer.OpeningBalance,
                TotalSales = totalSales,
                TotalPayments = totalPayments,
                CurrentOutstandingBalance = customer.CurrentBalance, 
                LastTransactionDate = lastTx?.TransactionDate
            };
        }
    }
}
