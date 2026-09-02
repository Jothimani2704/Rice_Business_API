using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Ledger;

namespace RiceBusinessApp.Application.Services
{
    public interface ILedgerService
    {
        Task<IEnumerable<CustomerTransactionDto>> GetCustomerTransactionsAsync(int customerId);
        Task<CustomerAccountSummaryDto> GetCustomerAccountSummaryAsync(int customerId);
    }
}
