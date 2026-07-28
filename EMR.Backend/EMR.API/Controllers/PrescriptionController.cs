using EMR.Application.DTOs.Prescriptions;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }
            return Ok(prescription);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatientId(int patientId)
        {
            var prescriptions = await _prescriptionService.GetPrescriptionsByPatientAsync(patientId);
            return Ok(prescriptions);
        }

        [HttpGet("appointment/{appointmentId}")]
        public async Task<IActionResult> GetByAppointmentId(int appointmentId)
        {
            var prescription = await _prescriptionService.GetPrescriptionByAppointmentAsync(appointmentId);
            if (prescription == null)
            {
                return NotFound();
            }
            return Ok(prescription);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PrescriptionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdPrescription = await _prescriptionService.CreatePrescriptionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdPrescription.PrescriptionId }, createdPrescription);
        }
    }
}
