using EMR.Application.DTOs.Appointments;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Domain.Enums;
using EMR.Shared.Common;

namespace EMR.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repository;

    public AppointmentService(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<AppointmentListDto>> GetAllAsync(AppointmentQueryParams queryParams)
    {
        var result = await _repository.GetAllAsync(queryParams);
        return new PagedResult<AppointmentListDto>
        {
            Items = result.Items.Select(MapToListDto).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<AppointmentDetailDto?> GetByIdAsync(int id)
    {
        var appointment = await _repository.GetByIdAsync(id);
        return appointment is null ? null : MapToDetailDto(appointment);
    }

    public async Task<AppointmentDetailDto> CreateAsync(AppointmentCreateDto dto)
    {
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (await _repository.HasConflictAsync(dto.DoctorId, dto.AppointmentDate, dto.StartTime, dto.EndTime))
            throw new InvalidOperationException("This time slot is already booked for the selected doctor");

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            AppointmentDate = dto.AppointmentDate.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Reason = dto.Reason,
            Notes = dto.Notes,
            Status = AppointmentStatus.Pending
        };

        var created = await _repository.AddAsync(appointment);

        // Repository se dobara fetch karenge taaki Patient/Doctor navigation properties load ho jayein
        var fullAppointment = await _repository.GetByIdAsync(created.AppointmentId);
        return MapToDetailDto(fullAppointment!);
    }

    public async Task<bool> UpdateAsync(AppointmentUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.AppointmentId);
        if (existing is null) return false;

        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (await _repository.HasConflictAsync(dto.DoctorId, dto.AppointmentDate, dto.StartTime, dto.EndTime, dto.AppointmentId))
            throw new InvalidOperationException("This time slot is already booked for the selected doctor");

        existing.PatientId = dto.PatientId;
        existing.DoctorId = dto.DoctorId;
        existing.AppointmentDate = dto.AppointmentDate.Date;
        existing.StartTime = dto.StartTime;
        existing.EndTime = dto.EndTime;
        existing.Reason = dto.Reason;
        existing.Notes = dto.Notes;

        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> UpdateStatusAsync(AppointmentStatusUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.AppointmentId);
        if (existing is null) return false;

        existing.Status = dto.Status;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<List<CalendarEventDto>> GetCalendarEventsAsync(DateTime from, DateTime to, int? doctorId)
    {
        var appointments = await _repository.GetByDateRangeAsync(from, to, doctorId);
        return appointments.Select(a => new CalendarEventDto
        {
            AppointmentId = a.AppointmentId,
            Title = $"{a.Patient.FullName} - Dr. {a.Doctor.User.FullName}",
            Start = a.AppointmentDate.Date + a.StartTime,
            End = a.AppointmentDate.Date + a.EndTime,
            Status = a.Status.ToString(),
            Color = GetStatusColor(a.Status)
        }).ToList();
    }

    public async Task<List<AppointmentListDto>> GetTodayAppointmentsAsync()
    {
        var appointments = await _repository.GetTodayAppointmentsAsync();
        return appointments.Select(MapToListDto).ToList();
    }

    public async Task<List<AppointmentListDto>> GetUpcomingAppointmentsAsync()
    {
        var appointments = await _repository.GetUpcomingAppointmentsAsync(10);
        return appointments.Select(MapToListDto).ToList();
    }
    public async Task<bool> RescheduleAsync(AppointmentRescheduleDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.AppointmentId);
        if (existing is null) return false;

        ValidateTimeRange(dto.NewStartTime, dto.NewEndTime);

        if (await _repository.HasConflictAsync(existing.DoctorId, dto.NewDate, dto.NewStartTime, dto.NewEndTime, dto.AppointmentId))
            throw new InvalidOperationException("This time slot is already booked for the selected doctor");

        existing.AppointmentDate = dto.NewDate.Date;
        existing.StartTime = dto.NewStartTime;
        existing.EndTime = dto.NewEndTime;

        await _repository.UpdateAsync(existing);
        return true;
    }

    private static void ValidateTimeRange(TimeSpan start, TimeSpan end)
    {
        if (start >= end)
            throw new InvalidOperationException("Start time must be before end time");
    }

    private static string GetStatusColor(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Pending => "#FFA726",     // Orange
        AppointmentStatus.Confirmed => "#42A5F5",   // Blue
        AppointmentStatus.Completed => "#66BB6A",   // Green
        AppointmentStatus.Cancelled => "#EF5350",   // Red
        _ => "#9E9E9E"
    };

    private static AppointmentListDto MapToListDto(Appointment a) => new()
    {
        AppointmentId = a.AppointmentId,
        PatientId = a.PatientId,
        PatientName = a.Patient.FullName,
        PatientMobile = a.Patient.Mobile,
        DoctorId = a.DoctorId,
        DoctorName = a.Doctor.User.FullName,
        Specialization = a.Doctor.Specialization,
        AppointmentDate = a.AppointmentDate,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = a.Status.ToString(),
        Reason = a.Reason
    };

    private static AppointmentDetailDto MapToDetailDto(Appointment a) => new()
    {
        AppointmentId = a.AppointmentId,
        PatientId = a.PatientId,
        PatientName = a.Patient.FullName,
        PatientMobile = a.Patient.Mobile,
        DoctorId = a.DoctorId,
        DoctorName = a.Doctor.User.FullName,
        Specialization = a.Doctor.Specialization,
        AppointmentDate = a.AppointmentDate,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = a.Status.ToString(),
        Reason = a.Reason,
        Notes = a.Notes,
        CreatedAt = a.CreatedAt
    };
}