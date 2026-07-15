using EMR.Application.DTOs.Doctors;
using EMR.Domain.Entities;
using EMR.Shared.Common;

namespace EMR.Application.Interfaces;

public interface IDoctorRepository
{
    Task<PagedResult<Doctor>> GetAllAsync(DoctorQueryParams queryParams);
    Task<Doctor?> GetByIdAsync(int id);
    Task<Doctor> AddAsync(Doctor doctor);
    Task UpdateAsync(Doctor doctor);
    Task<bool> DeleteAsync(int id);
    Task<List<Doctor>> GetActiveDoctorsAsync();
}