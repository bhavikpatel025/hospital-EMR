using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Repositories
{
    public class TranslationRepository : ITranslationRepository
    {
        private readonly AppDbContext _context;

        public TranslationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TranslationDictionary>> GetTranslationsAsync(List<string> texts, string targetLanguage)
        {
            return await _context.Translations
                .Where(t => t.TargetLanguage == targetLanguage && texts.Contains(t.OriginalText))
                .ToListAsync();
        }

        public async Task AddTranslationsAsync(IEnumerable<TranslationDictionary> translations)
        {
            await _context.Translations.AddRangeAsync(translations);
            await _context.SaveChangesAsync();
        }
    }
}
