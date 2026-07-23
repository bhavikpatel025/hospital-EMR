using System.Threading.Tasks;
using EMR.Application.DTOs.Assessments;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMR.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class JointAssessmentsController : ControllerBase
    {
        private readonly IJointAssessmentService _service;

        public JointAssessmentsController(IJointAssessmentService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssessment([FromBody] JointAssessmentDto dto)
        {
            var result = await _service.CreateAssessmentAsync(dto);
            return Ok(result);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetAssessmentsByPatient(int patientId)
        {
            var results = await _service.GetAssessmentsByPatientAsync(patientId);
            return Ok(results);
        }

        [HttpGet("patient/{patientId}/latest")]
        public async Task<IActionResult> GetLatestAssessment(int patientId)
        {
            var result = await _service.GetLatestAssessmentByPatientAsync(patientId);
            if (result == null) return NotFound("No assessments found for this patient.");
            return Ok(result);
        }
    }
}
