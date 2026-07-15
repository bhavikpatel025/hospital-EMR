using AutoMapper;
using EMR.Application.DTOs.Patients;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Shared.Common;

namespace EMR.Application.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repository;
    private readonly IMapper _mapper;

    public PatientService(IPatientRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<PatientListDto>> GetAllAsync(PatientQueryParams queryParams)
    {
        var result = await _repository.GetAllAsync(queryParams);

        return new PagedResult<PatientListDto>
        {
            Items = _mapper.Map<List<PatientListDto>>(result.Items),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<PatientDetailDto?> GetByIdAsync(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        return patient is null ? null : _mapper.Map<PatientDetailDto>(patient);
    }

    public async Task<PatientDetailDto> CreateAsync(PatientCreateDto dto)
    {
        if (await _repository.MobileExistsAsync(dto.Mobile))
            throw new InvalidOperationException("A patient with this mobile number already exists");

        var patient = _mapper.Map<Patient>(dto);
        var created = await _repository.AddAsync(patient);
        return _mapper.Map<PatientDetailDto>(created);
    }

    public async Task<bool> UpdateAsync(PatientUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.PatientId);
        if (existing is null) return false;

        if (await _repository.MobileExistsAsync(dto.Mobile, dto.PatientId))
            throw new InvalidOperationException("Another patient already has this mobile number");

        _mapper.Map(dto, existing);   // existing object ko update karta hai
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}