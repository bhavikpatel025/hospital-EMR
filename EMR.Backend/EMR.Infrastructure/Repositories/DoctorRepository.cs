using EMR.Application.DTOs.Doctors;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Infrastructure.Data;
using EMR.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly AppDbContext _context;
    public DoctorRepository(AppDbContext context) => _context = context;

    public async Task<PagedResult<Doctor>> GetAllAsync(DoctorQueryParams q)
    {
        var query = _context.Doctors.Include(d => d.User).Where(d => d.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.SearchTerm))
        {
            var term = q.SearchTerm.Trim().ToLower();
            query = query.Where(d =>
                d.User.FullName.ToLower().Contains(term) ||
                d.User.Email.ToLower().Contains(term) ||
                d.Specialization.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(q.Specialization))
        {
            query = query.Where(d => d.Specialization == q.Specialization);
        }

        query = q.SortBy?.ToLower() switch
        {
            "specialization" => q.SortDescending ? query.OrderByDescending(d => d.Specialization) : query.OrderBy(d => d.Specialization),
            "fee" => q.SortDescending ? query.OrderByDescending(d => d.ConsultationFee) : query.OrderBy(d => d.ConsultationFee),
            _ => q.SortDescending ? query.OrderByDescending(d => d.User.FullName) : query.OrderBy(d => d.User.FullName)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip((q.PageNumber - 1) * q.PageSize).Take(q.PageSize).ToListAsync();

        return new PagedResult<Doctor>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = q.PageNumber,
            PageSize = q.PageSize
        };
    }

    public async Task<Doctor?> GetByIdAsync(int id)
    {
        return await _context.Doctors.Include(d => d.User)
            .FirstOrDefaultAsync(d => d.DoctorId == id && d.IsActive);
    }

    public async Task<Doctor> AddAsync(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();
        return doctor;
    }

    public async Task UpdateAsync(Doctor doctor)
    {
        _context.Doctors.Update(doctor);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor is null) return false;

        doctor.IsActive = false;   // Soft delete
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Doctor>> GetActiveDoctorsAsync()
    {
        return await _context.Doctors.Include(d => d.User)
            .Where(d => d.IsActive)
            .OrderBy(d => d.User.FullName)
            .ToListAsync();
    }
}