using EMR.Application.DTOs.Doctors;
using EMR.Shared.Common;

namespace EMR.Application.Interfaces;

public interface IDoctorService
{
    Task<PagedResult<DoctorListDto>> GetAllAsync(DoctorQueryParams queryParams);
    Task<DoctorDetailDto?> GetByIdAsync(int id);
    Task<DoctorDetailDto> CreateAsync(DoctorCreateDto dto);
    Task<bool> UpdateAsync(DoctorUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<DoctorListDto>> GetActiveDoctorsAsync();
}