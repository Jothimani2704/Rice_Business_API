using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiceBusinessApp.Application.Services;

namespace RiceBusinessApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/customers/{customerId}")]
    public class LedgerController : ControllerBase
    {
        private readonly ILedgerService _ledgerService;

        public LedgerController(ILedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetCustomerTransactions(int customerId)
        {
            try
            {
                var transactions = await _ledgerService.GetCustomerTransactionsAsync(customerId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("account-summary")]
        public async Task<IActionResult> GetCustomerAccountSummary(int customerId)
        {
            try
            {
                var summary = await _ledgerService.GetCustomerAccountSummaryAsync(customerId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
