using EMR.Application.DTOs.Dashboard;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EMR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("analytics")]
        public async Task<ActionResult<DashboardAnalyticsDto>> GetAnalytics()
        {
            var analytics = await _dashboardService.GetAnalyticsAsync();
            return Ok(analytics);
        }
    }
}
