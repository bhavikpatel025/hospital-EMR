using EMR.Application.DTOs.Dashboard;
using System.Threading.Tasks;

namespace EMR.Application.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardAnalyticsDto> GetAnalyticsAsync();
    }
}
