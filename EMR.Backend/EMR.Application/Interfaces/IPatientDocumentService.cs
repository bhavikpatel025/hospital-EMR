using System.Threading.Tasks;

namespace EMR.Application.Interfaces;

public interface IPatientDocumentService
{
    Task<(string explanation, bool cached)> ExplainDocumentAsync(int documentId, string language, string type = "DOC");
}
