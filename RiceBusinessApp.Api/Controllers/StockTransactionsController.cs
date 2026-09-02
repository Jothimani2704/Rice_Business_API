using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RiceBusinessApp.Application.DTOs.StockTransaction;
using RiceBusinessApp.Application.Services;

namespace RiceBusinessApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockTransactionsController : ControllerBase
    {
        private readonly IStockTransactionService _stockTransactionService;

        public StockTransactionsController(IStockTransactionService stockTransactionService)
        {
            _stockTransactionService = stockTransactionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockTransactionDto>>> GetStockTransactions()
        {
            var transactions = await _stockTransactionService.GetAllAsync();
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StockTransactionDto>> GetStockTransaction(int id)
        {
            try
            {
                var transaction = await _stockTransactionService.GetByIdAsync(id);
                return Ok(transaction);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<StockTransactionDto>>> GetStockTransactionsByProduct(int productId)
        {
            var transactions = await _stockTransactionService.GetByProductIdAsync(productId);
            return Ok(transactions);
        }

        [HttpGet("product/{productId}/summary")]
        public async Task<ActionResult<StockSummaryDto>> GetStockSummaryByProduct(int productId)
        {
            try
            {
                var summary = await _stockTransactionService.GetStockSummaryByProductIdAsync(productId);
                return Ok(summary);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<StockTransactionDto>> CreateStockTransaction(CreateStockTransactionDto createDto)
        {
            try
            {
                var transaction = await _stockTransactionService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetStockTransaction), new { id = transaction.Id }, transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StockTransactionDto>> UpdateStockTransaction(int id, UpdateStockTransactionDto updateDto)
        {
            try
            {
                var transaction = await _stockTransactionService.UpdateAsync(id, updateDto);
                return Ok(transaction);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
