using EMR.Application.DTOs.Doctors;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMR.API.Controllers;

[ApiController]
[Route("api/doctors")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _service;
    public DoctorsController(IDoctorService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DoctorQueryParams queryParams)
    {
        var result = await _service.GetAllAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveDoctors()
    {
        // Appointment form ke dropdown ke liye — sab active doctors, bina pagination
        var doctors = await _service.GetActiveDoctorsAsync();
        return Ok(doctors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _service.GetByIdAsync(id);
        if (doctor is null) return NotFound(new { message = "Doctor not found" });
        return Ok(doctor);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]   // Sirf Admin naya doctor add kar sakta hai
    public async Task<IActionResult> Create([FromBody] DoctorCreateDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.DoctorId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] DoctorUpdateDto dto)
    {
        if (id != dto.DoctorId) return BadRequest(new { message = "Doctor ID mismatch" });

        var result = await _service.UpdateAsync(dto);
        if (!result) return NotFound(new { message = "Doctor not found" });
        return Ok(new { message = "Doctor updated successfully" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound(new { message = "Doctor not found" });
        return Ok(new { message = "Doctor deleted successfully" });
    }
}