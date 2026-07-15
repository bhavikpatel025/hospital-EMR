using EMR.Application.DTOs.Appointments;
using EMR.Domain.Entities;
using EMR.Shared.Common;

namespace EMR.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<PagedResult<Appointment>> GetAllAsync(AppointmentQueryParams queryParams);
    Task<Appointment?> GetByIdAsync(int id);
    Task<Appointment> AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task<bool> DeleteAsync(int id);
    Task<bool> HasConflictAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null);
    Task<List<Appointment>> GetByDateRangeAsync(DateTime from, DateTime to, int? doctorId = null);
    Task<List<Appointment>> GetTodayAppointmentsAsync();
    Task<List<Appointment>> GetUpcomingAppointmentsAsync(int count);
}