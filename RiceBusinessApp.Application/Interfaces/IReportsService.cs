using System.Collections.Generic;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Reports;

namespace RiceBusinessApp.Application.Interfaces
{
    public interface IReportsService
    {
        Task<List<RevenueTrendDto>> GetRevenueTrendsAsync(string period);
        Task<FinancialAnalyticsSummaryDto> GetFinancialAnalyticsSummaryAsync(string period);
    }
}
