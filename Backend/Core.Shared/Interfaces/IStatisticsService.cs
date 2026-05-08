using Core.Shared.Models;

namespace Core.Shared.Interfaces;

public interface IStatisticsService
{
    Task<DashboardStats> GetDashboardStatsAsync();
}
