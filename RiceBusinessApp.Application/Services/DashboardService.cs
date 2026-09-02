using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Dashboard;
using RiceBusinessApp.Application.Interfaces;
namespace RiceBusinessApp.Application.Services { public class DashboardService : IDashboardService { private readonly IDashboardRepository _repo; public DashboardService(IDashboardRepository repo) { _repo = repo; } public async Task<DashboardSummaryDto> GetDashboardSummaryAsync() => await _repo.GetDashboardSummaryAsync(); } }
