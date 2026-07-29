using System.Collections.Generic;
using System.Threading.Tasks;
using EMR.Domain.Entities;

namespace EMR.Application.Interfaces
{
    public interface ITranslationRepository
    {
        Task<List<TranslationDictionary>> GetTranslationsAsync(List<string> texts, string targetLanguage);
        Task AddTranslationsAsync(IEnumerable<TranslationDictionary> translations);
    }
}
