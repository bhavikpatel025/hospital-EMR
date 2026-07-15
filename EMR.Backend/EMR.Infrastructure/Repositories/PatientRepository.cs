using EMR.Application.DTOs.Patients;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Infrastructure.Data;
using EMR.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _context;
    public PatientRepository(AppDbContext context) => _context = context;

    public async Task<PagedResult<Patient>> GetAllAsync(PatientQueryParams q)
    {
        var query = _context.Patients.Where(p => p.IsActive).AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(q.SearchTerm))
        {
            var term = q.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.FullName.ToLower().Contains(term) ||
                p.Mobile.Contains(term) ||
                (p.Email != null && p.Email.ToLower().Contains(term)));
        }

        // Filter
        if (!string.IsNullOrWhiteSpace(q.Gender))
        {
            query = query.Where(p => p.Gender == q.Gender);
        }

        // Sorting
        query = q.SortBy?.ToLower() switch
        {
            "age" => q.SortDescending ? query.OrderByDescending(p => p.Age) : query.OrderBy(p => p.Age),
            "createdat" => q.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => q.SortDescending ? query.OrderByDescending(p => p.FullName) : query.OrderBy(p => p.FullName)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((q.PageNumber - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync();

        return new PagedResult<Patient>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = q.PageNumber,
            PageSize = q.PageSize
        };
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        return await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == id && p.IsActive);
    }

    public async Task<Patient> AddAsync(Patient patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task UpdateAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient is null) return false;

        // Soft Delete — real hospital data kabhi hard delete nahi hoti
        patient.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MobileExistsAsync(string mobile, int? excludePatientId = null)
    {
        return await _context.Patients.AnyAsync(p =>
            p.Mobile == mobile &&
            p.IsActive &&
            (!excludePatientId.HasValue || p.PatientId != excludePatientId.Value));
    }
}