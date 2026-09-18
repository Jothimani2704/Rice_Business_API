using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Reports;
using RiceBusinessApp.Application.Interfaces;

namespace RiceBusinessApp.Application.Services
{
    public class ReportsService : IReportsService
    {
        private readonly IReportsRepository _repo;

        public ReportsService(IReportsRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<RevenueTrendDto>> GetRevenueTrendsAsync(string period)
        {
            return await _repo.GetRevenueTrendsAsync(period);
        }

        public async Task<FinancialAnalyticsSummaryDto> GetFinancialAnalyticsSummaryAsync(string period)
        {
            return await _repo.GetFinancialAnalyticsSummaryAsync(period);
        }
    }
}
