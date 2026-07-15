using AutoMapper;
using EMR.Application.DTOs.Doctors;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Shared.Common;

namespace EMR.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public DoctorService(IDoctorRepository doctorRepository, IUserRepository userRepository, IMapper mapper)
    {
        _doctorRepository = doctorRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<DoctorListDto>> GetAllAsync(DoctorQueryParams queryParams)
    {
        var result = await _doctorRepository.GetAllAsync(queryParams);

        return new PagedResult<DoctorListDto>
        {
            Items = result.Items.Select(MapToListDto).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<DoctorDetailDto?> GetByIdAsync(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        return doctor is null ? null : MapToDetailDto(doctor);
    }

    public async Task<DoctorDetailDto> CreateAsync(DoctorCreateDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser is not null)
            throw new InvalidOperationException("A user with this email already exists");

        EMR.Shared.Common.PasswordHasher.CreateHash(dto.Password, out var hash, out var salt);

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            RoleId = 2,     // 2 = Doctor (jo humne seed kiya tha)
            IsActive = true
        };

        var doctor = new Doctor
        {
            User = user,
            Specialization = dto.Specialization,
            Qualification = dto.Qualification,
            ConsultationFee = dto.ConsultationFee,
            ExperienceYears = dto.ExperienceYears
        };

        var created = await _doctorRepository.AddAsync(doctor);
        return MapToDetailDto(created);
    }

    public async Task<bool> UpdateAsync(DoctorUpdateDto dto)
    {
        var existing = await _doctorRepository.GetByIdAsync(dto.DoctorId);
        if (existing is null) return false;

        existing.User.FullName = dto.FullName;
        existing.Specialization = dto.Specialization;
        existing.Qualification = dto.Qualification;
        existing.ConsultationFee = dto.ConsultationFee;
        existing.ExperienceYears = dto.ExperienceYears;

        await _doctorRepository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _doctorRepository.DeleteAsync(id);
    }

    public async Task<List<DoctorListDto>> GetActiveDoctorsAsync()
    {
        var doctors = await _doctorRepository.GetActiveDoctorsAsync();
        return doctors.Select(MapToListDto).ToList();
    }

    // Manual mapping — kyunki User + Doctor dono se data combine karna hai
    private static DoctorListDto MapToListDto(Doctor d) => new()
    {
        DoctorId = d.DoctorId,
        FullName = d.User.FullName,
        Email = d.User.Email,
        Specialization = d.Specialization,
        ConsultationFee = d.ConsultationFee,
        IsActive = d.IsActive
    };

    private static DoctorDetailDto MapToDetailDto(Doctor d) => new()
    {
        DoctorId = d.DoctorId,
        FullName = d.User.FullName,
        Email = d.User.Email,
        Specialization = d.Specialization,
        Qualification = d.Qualification,
        ConsultationFee = d.ConsultationFee,
        ExperienceYears = d.ExperienceYears,
        IsActive = d.IsActive,
        CreatedAt = d.CreatedAt
    };
}