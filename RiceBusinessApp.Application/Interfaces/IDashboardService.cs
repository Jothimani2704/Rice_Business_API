using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Dashboard;
namespace RiceBusinessApp.Application.Interfaces {
public interface IDashboardService {
Task<DashboardSummaryDto> GetDashboardSummaryAsync();
} }
