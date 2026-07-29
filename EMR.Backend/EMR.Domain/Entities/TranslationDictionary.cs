using System;

namespace EMR.Domain.Entities
{
    public class TranslationDictionary
    {
        public int Id { get; set; }
        public string OriginalText { get; set; } = string.Empty;
        public string TargetLanguage { get; set; } = string.Empty; // e.g., "hi", "gu"
        public string TranslatedText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
