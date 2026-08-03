using EMR.Application.DTOs.Appointments;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMR.API.Controllers;

[ApiController]
[Route("api/public")]
public class PublicApiController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorRepository _doctorRepository;

    public PublicApiController(IAppointmentService appointmentService, IDoctorRepository doctorRepository)
    {
        _appointmentService = appointmentService;
        _doctorRepository = doctorRepository;
    }

    [HttpPost("appointments/book")]
    [AllowAnonymous]
    public async Task<IActionResult> BookAppointment([FromBody] PublicAppointmentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _appointmentService.BookPublicAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctors")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveDoctors()
    {
        var doctors = await _doctorRepository.GetActiveDoctorsAsync();
        var result = doctors.Select(d => new
        {
            d.DoctorId,
            DoctorName = d.User.FullName,
            d.Specialization,
            d.Qualification
        });
        return Ok(result);
    }
}
