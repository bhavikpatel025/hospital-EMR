using EMR.Application.DTOs.Patients;
using EMR.Shared.Common;

namespace EMR.Application.Interfaces;

public interface IPatientService
{
    Task<PagedResult<PatientListDto>> GetAllAsync(PatientQueryParams queryParams);
    Task<PatientDetailDto?> GetByIdAsync(int id);
    Task<PatientDetailDto> CreateAsync(PatientCreateDto dto);
    Task<bool> UpdateAsync(PatientUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}