using EMR.Application.DTOs.Dashboard;
using EMR.Application.Interfaces;
using System.Threading.Tasks;

namespace EMR.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repository;

        public DashboardService(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<DashboardAnalyticsDto> GetAnalyticsAsync()
        {
            return await _repository.GetAnalyticsAsync();
        }
    }
}
