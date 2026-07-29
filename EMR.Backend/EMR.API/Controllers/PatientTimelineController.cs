using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientTimelineController : ControllerBase
    {
        private readonly IPatientTimelineService _timelineService;

        public PatientTimelineController(IPatientTimelineService timelineService)
        {
            _timelineService = timelineService;
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetTimeline(int patientId)
        {
            try
            {
                var timeline = await _timelineService.GetTimelineByPatientIdAsync(patientId);
                return Ok(timeline);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the patient timeline.", Details = ex.Message });
            }
        }
    }
}
