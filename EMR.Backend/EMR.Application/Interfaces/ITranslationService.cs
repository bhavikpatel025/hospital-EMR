using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMR.Application.Interfaces
{
    public interface ITranslationService
    {
        Task<Dictionary<string, string>> GetTranslationsAsync(List<string> texts, string targetLanguage);
    }
}
