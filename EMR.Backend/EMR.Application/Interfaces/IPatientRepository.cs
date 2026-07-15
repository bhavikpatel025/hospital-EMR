using EMR.Application.DTOs.Patients;
using EMR.Domain.Entities;
using EMR.Shared.Common;

namespace EMR.Application.Interfaces;

public interface IPatientRepository
{
    Task<PagedResult<Patient>> GetAllAsync(PatientQueryParams queryParams);
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient> AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task<bool> DeleteAsync(int id);
    Task<bool> MobileExistsAsync(string mobile, int? excludePatientId = null);
}