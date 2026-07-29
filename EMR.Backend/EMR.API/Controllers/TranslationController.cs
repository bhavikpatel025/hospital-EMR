using System.Collections.Generic;
using System.Threading.Tasks;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;

        public TranslationController(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslationRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TargetLanguage) || request.Texts == null)
            {
                return BadRequest("Invalid request payload.");
            }

            var result = await _translationService.GetTranslationsAsync(request.Texts, request.TargetLanguage);
            return Ok(result);
        }
    }

    public class TranslationRequestDto
    {
        public List<string> Texts { get; set; } = new List<string>();
        public string TargetLanguage { get; set; } = string.Empty;
    }
}
