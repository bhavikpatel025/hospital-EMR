using System.Threading.Tasks;
using EMR.Domain.Entities;

namespace EMR.Application.Interfaces;

public interface IPatientDocumentRepository
{
    Task<PatientDocument?> GetByIdAsync(int id);
    Task UpdateAsync(PatientDocument document);
}
