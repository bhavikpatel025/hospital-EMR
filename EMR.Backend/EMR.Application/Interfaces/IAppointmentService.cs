using EMR.Application.DTOs.Appointments;
using EMR.Shared.Common;

namespace EMR.Application.Interfaces;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentListDto>> GetAllAsync(AppointmentQueryParams queryParams);
    Task<AppointmentDetailDto?> GetByIdAsync(int id);
    Task<AppointmentDetailDto> CreateAsync(AppointmentCreateDto dto);
    Task<bool> UpdateAsync(AppointmentUpdateDto dto);
    Task<bool> UpdateStatusAsync(AppointmentStatusUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<CalendarEventDto>> GetCalendarEventsAsync(DateTime from, DateTime to, int? doctorId);
    Task<List<AppointmentListDto>> GetTodayAppointmentsAsync();
    Task<List<AppointmentListDto>> GetUpcomingAppointmentsAsync();
    Task<bool> RescheduleAsync(AppointmentRescheduleDto dto);
}