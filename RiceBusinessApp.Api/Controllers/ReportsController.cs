using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiceBusinessApp.Application.Interfaces;

namespace RiceBusinessApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet("revenue-trends")]
        public async Task<IActionResult> GetRevenueTrends([FromQuery] string period = "daily")
        {
            try
            {
                var trends = await _reportsService.GetRevenueTrendsAsync(period);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetFinancialSummary([FromQuery] string period = "daily")
        {
            try
            {
                var summary = await _reportsService.GetFinancialAnalyticsSummaryAsync(period);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
