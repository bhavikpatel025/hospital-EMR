using EMR.Application.DTOs.Appointments;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMR.API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;
    public AppointmentsController(IAppointmentService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AppointmentQueryParams queryParams)
    {
        var result = await _service.GetAllAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _service.GetByIdAsync(id);
        if (appointment is null) return NotFound(new { message = "Appointment not found" });
        return Ok(appointment);
    }

    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendarEvents([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? doctorId)
    {
        var events = await _service.GetCalendarEventsAsync(from, to, doctorId);
        return Ok(events);
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodayAppointments()
    {
        var appointments = await _service.GetTodayAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingAppointments()
    {
        var appointments = await _service.GetUpcomingAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.AppointmentId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AppointmentUpdateDto dto)
    {
        if (id != dto.AppointmentId) return BadRequest(new { message = "Appointment ID mismatch" });

        try
        {
            var result = await _service.UpdateAsync(dto);
            if (!result) return NotFound(new { message = "Appointment not found" });
            return Ok(new { message = "Appointment updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPatch("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, [FromBody] AppointmentRescheduleDto dto)
    {
        if (id != dto.AppointmentId) return BadRequest(new { message = "Appointment ID mismatch" });

        try
        {
            var result = await _service.RescheduleAsync(dto);
            if (!result) return NotFound(new { message = "Appointment not found" });
            return Ok(new { message = "Appointment rescheduled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AppointmentStatusUpdateDto dto)
    {
        if (id != dto.AppointmentId) return BadRequest(new { message = "Appointment ID mismatch" });

        var result = await _service.UpdateStatusAsync(dto);
        if (!result) return NotFound(new { message = "Appointment not found" });
        return Ok(new { message = "Status updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound(new { message = "Appointment not found" });
        return Ok(new { message = "Appointment deleted successfully" });
    }
}