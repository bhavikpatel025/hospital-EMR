using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EMR.Application.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly ITranslationRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TranslationService> _logger;
        private readonly HttpClient _httpClient;

        public TranslationService(
            ITranslationRepository repository,
            IConfiguration configuration,
            ILogger<TranslationService> logger)
        {
            _repository = repository;
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task<Dictionary<string, string>> GetTranslationsAsync(List<string> texts, string targetLanguage)
        {
            var result = new Dictionary<string, string>();
            if (texts == null || !texts.Any())
                return result;

            // 1. Check Database for existing translations
            var existingTranslations = await _repository.GetTranslationsAsync(texts, targetLanguage);
            foreach (var t in existingTranslations)
            {
                result[t.OriginalText] = t.TranslatedText;
            }

            // 2. Identify missing translations
            var missingTexts = texts.Except(existingTranslations.Select(e => e.OriginalText)).ToList();

            if (!missingTexts.Any())
                return result; // Everything was in DB!

            // 3. Call Groq API for missing texts
            var newTranslations = await TranslateWithGroqAsync(missingTexts, targetLanguage);

            // 4. Save new translations to DB (Self-Learning)
            var entitiesToSave = new List<TranslationDictionary>();
            foreach (var kvp in newTranslations)
            {
                result[kvp.Key] = kvp.Value; // Add to result
                
                entitiesToSave.Add(new TranslationDictionary
                {
                    OriginalText = kvp.Key,
                    TargetLanguage = targetLanguage,
                    TranslatedText = kvp.Value,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (entitiesToSave.Any())
            {
                await _repository.AddTranslationsAsync(entitiesToSave);
            }

            return result;
        }

        private async Task<Dictionary<string, string>> TranslateWithGroqAsync(List<string> texts, string targetLanguage)
        {
            var result = new Dictionary<string, string>();
            
            string apiKey = _configuration["GroqSettings:ApiKey"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Equals("YOUR_GROQ_API_KEY_HERE", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Translation bypassed: No valid Groq API key found.");
                // Return original as fallback
                foreach (var text in texts) result[text] = text;
                return result;
            }

            string endpoint = "https://api.groq.com/openai/v1/chat/completions";
            string model = _configuration["GroqSettings:Model"] ?? "llama-3.3-70b-versatile";

            string prompt = $"You are a medical translator. Translate the following English medical instructions into {targetLanguage}. Maintain accurate medical context. Return ONLY a valid JSON object where keys are the exact English texts provided and values are their {targetLanguage} translations. Do not include markdown formatting or extra text.\n\nTexts to translate:\n" + string.Join("\n", texts.Select(t => $"- {t}"));

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.3,
                response_format = new { type = "json_object" }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            try
            {
                var response = await _httpClient.PostAsync(endpoint, jsonContent);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseString);
                var contentString = jsonDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (!string.IsNullOrEmpty(contentString))
                {
                    var translatedDict = JsonSerializer.Deserialize<Dictionary<string, string>>(contentString);
                    if (translatedDict != null)
                    {
                        foreach(var key in translatedDict.Keys)
                        {
                            result[key] = translatedDict[key];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate with Groq AI.");
            }

            // Fallback for any missing items that AI failed to translate
            foreach (var text in texts)
            {
                if (!result.ContainsKey(text))
                {
                    result[text] = text;
                }
            }

            return result;
        }
    }
}
