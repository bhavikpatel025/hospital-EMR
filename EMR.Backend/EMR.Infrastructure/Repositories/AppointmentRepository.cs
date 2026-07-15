using EMR.Application.DTOs.Appointments;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Domain.Enums;
using EMR.Infrastructure.Data;
using EMR.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;
    public AppointmentRepository(AppDbContext context) => _context = context;

    private IQueryable<Appointment> BaseQuery() =>
        _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.User);

    public async Task<PagedResult<Appointment>> GetAllAsync(AppointmentQueryParams q)
    {
        var query = BaseQuery().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.SearchTerm))
        {
            var term = q.SearchTerm.Trim().ToLower();
            query = query.Where(a =>
                a.Patient.FullName.ToLower().Contains(term) ||
                a.Patient.Mobile.Contains(term));
        }

        if (q.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == q.DoctorId.Value);

        if (q.Status.HasValue)
            query = query.Where(a => a.Status == q.Status.Value);

        if (q.FromDate.HasValue)
            query = query.Where(a => a.AppointmentDate >= q.FromDate.Value.Date);

        if (q.ToDate.HasValue)
            query = query.Where(a => a.AppointmentDate <= q.ToDate.Value.Date);

        query = q.SortBy?.ToLower() switch
        {
            "patientname" => q.SortDescending ? query.OrderByDescending(a => a.Patient.FullName) : query.OrderBy(a => a.Patient.FullName),
            "status" => q.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            _ => q.SortDescending
                ? query.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime)
                : query.OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip((q.PageNumber - 1) * q.PageSize).Take(q.PageSize).ToListAsync();

        return new PagedResult<Appointment>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = q.PageNumber,
            PageSize = q.PageSize
        };
    }

    public async Task<Appointment?> GetByIdAsync(int id) =>
        await BaseQuery().FirstOrDefaultAsync(a => a.AppointmentId == id);

    public async Task<Appointment> AddAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        appointment.UpdatedAt = DateTime.UtcNow;
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment is null) return false;

        // Appointments hard delete ho sakte hain (ya soft delete bhi kar sakte ho -
        // yaha hard delete rakha kyunki cancelled appointments alag se track hote hain Status se)
        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasConflictAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null)
    {
        return await _context.Appointments.AnyAsync(a =>
            a.DoctorId == doctorId &&
            a.AppointmentDate.Date == date.Date &&
            a.Status != AppointmentStatus.Cancelled &&
            (!excludeAppointmentId.HasValue || a.AppointmentId != excludeAppointmentId.Value) &&
            a.StartTime < endTime && a.EndTime > startTime);   // Overlap check
    }

    public async Task<List<Appointment>> GetByDateRangeAsync(DateTime from, DateTime to, int? doctorId = null)
    {
        var query = BaseQuery().Where(a => a.AppointmentDate.Date >= from.Date && a.AppointmentDate.Date <= to.Date);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId.Value);

        return await query.ToListAsync();
    }

    public async Task<List<Appointment>> GetTodayAppointmentsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await BaseQuery()
            .Where(a => a.AppointmentDate.Date == today && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetUpcomingAppointmentsAsync(int count)
    {
        var today = DateTime.UtcNow.Date;
        return await BaseQuery()
            .Where(a => a.AppointmentDate.Date > today && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Take(count)
            .ToListAsync();
    }
}