using EMR.Application.DTOs.Patients;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMR.API.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize] 
public class PatientsController : ControllerBase
{
    private readonly IPatientService _service;
    public PatientsController(IPatientService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PatientQueryParams queryParams)
    {
        var result = await _service.GetAllAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _service.GetByIdAsync(id);
        if (patient is null)
            return NotFound(new { message = "Patient not found" });

        return Ok(patient);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PatientCreateDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.PatientId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PatientUpdateDto dto)
    {
        if (id != dto.PatientId)
            return BadRequest(new { message = "Patient ID mismatch" });

        try
        {
            var result = await _service.UpdateAsync(dto);
            if (!result) return NotFound(new { message = "Patient not found" });
            return Ok(new { message = "Patient updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound(new { message = "Patient not found" });
        return Ok(new { message = "Patient deleted successfully" });
    }
}