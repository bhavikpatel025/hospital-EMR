using System.Threading.Tasks;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Repositories;

public class PatientDocumentRepository : IPatientDocumentRepository
{
    private readonly AppDbContext _context;

    public PatientDocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PatientDocument?> GetByIdAsync(int id)
    {
        return await _context.PatientDocuments.FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task UpdateAsync(PatientDocument document)
    {
        _context.PatientDocuments.Update(document);
        await _context.SaveChangesAsync();
    }
}
