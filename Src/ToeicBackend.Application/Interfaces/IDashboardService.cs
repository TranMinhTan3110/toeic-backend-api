using System.Threading.Tasks;
using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardStatsAsync();
}
