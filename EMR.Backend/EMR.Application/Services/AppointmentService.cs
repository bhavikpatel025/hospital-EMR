using EMR.Application.DTOs.Appointments;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Domain.Enums;
using EMR.Shared.Common;

namespace EMR.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repository;
    private readonly IPatientRepository _patientRepository;
    private readonly INotificationService _notificationService;

    public AppointmentService(
        IAppointmentRepository repository, 
        IPatientRepository patientRepository,
        INotificationService notificationService)
    {
        _repository = repository;
        _patientRepository = patientRepository;
        _notificationService = notificationService;
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
            Status = AppointmentStatus.Confirmed // Receptionist manual booking is auto-confirmed
        };

        var created = await _repository.AddAsync(appointment);
        var fullAppointment = await _repository.GetByIdAsync(created.AppointmentId);
        
        // Notify the doctor
        if (fullAppointment != null)
        {
            var doctorUserId = fullAppointment.Doctor.UserId;
            var timeFormatted = DateTime.Today.Add(fullAppointment.StartTime).ToString("hh:mm tt");
            await _notificationService.SendToUserAsync(doctorUserId, "New Appointment", $"An appointment was booked for {fullAppointment.Patient.FullName} on {fullAppointment.AppointmentDate:MMM dd} at {timeFormatted}.");
        }

        return MapToDetailDto(fullAppointment!);
    }

    public async Task<AppointmentDetailDto> BookPublicAsync(PublicAppointmentCreateDto dto)
    {
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        if (await _repository.HasConflictAsync(dto.DoctorId, dto.AppointmentDate, dto.StartTime, dto.EndTime))
            throw new InvalidOperationException("The selected time slot is no longer available.");

        // Check if patient exists by mobile
        var patient = await _patientRepository.GetByMobileAsync(dto.Mobile);

        if (patient == null)
        {
            patient = new Patient
            {
                FullName = $"{dto.FirstName} {dto.LastName}".Trim(),
                Mobile = dto.Mobile,
                IsActive = true
            };
            await _patientRepository.AddAsync(patient);
        }

        var appointment = new Appointment
        {
            PatientId = patient.PatientId,
            DoctorId = dto.DoctorId,
            AppointmentDate = dto.AppointmentDate.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Reason = dto.Reason ?? "Web Booking",
            Status = AppointmentStatus.Pending // Explicitly set to Pending for Receptionist approval
        };

        var created = await _repository.AddAsync(appointment);
        var fullAppointment = await _repository.GetByIdAsync(created.AppointmentId);
        
        if (fullAppointment != null)
        {
            var timeFormatted = DateTime.Today.Add(fullAppointment.StartTime).ToString("hh:mm tt");
            var msg = $"New online web booking request from {fullAppointment.Patient.FullName} for Dr. {fullAppointment.Doctor.User.FullName} on {fullAppointment.AppointmentDate:MMM dd} at {timeFormatted}.";
            // Notify Doctor
            await _notificationService.SendToUserAsync(fullAppointment.Doctor.UserId, "New Online Booking", msg);
            // Notify Receptionists
            await _notificationService.SendToRoleAsync("Receptionist", "New Online Booking", msg);
            // Notify Admins
            await _notificationService.SendToRoleAsync("Admin", "New Online Booking", msg);
        }

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
        
        if (dto.Status == AppointmentStatus.Cancelled)
        {
            var timeFormatted = DateTime.Today.Add(existing.StartTime).ToString("hh:mm tt");
            var msg = $"Appointment for {existing.Patient.FullName} with Dr. {existing.Doctor.User.FullName} on {existing.AppointmentDate:MMM dd} at {timeFormatted} has been cancelled.";
            await _notificationService.SendToRoleAsync("Receptionist", "Appointment Cancelled", msg);
        }

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