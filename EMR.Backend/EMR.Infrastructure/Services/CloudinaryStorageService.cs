using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EMR.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EMR.Infrastructure.Services;

public class CloudinaryStorageService : ICloudStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryStorageService> _logger;
    private readonly string _envPrefix;

    public CloudinaryStorageService(IConfiguration configuration, ILogger<CloudinaryStorageService> logger)
    {
        _logger = logger;
        
        // Check if we are running in Development or Production
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        _envPrefix = env.Equals("Development", StringComparison.OrdinalIgnoreCase) ? "emr-dev" : "emr-prod";

        var cloudinaryUrl = configuration["Cloudinary:Url"];
        
        if (string.IsNullOrWhiteSpace(cloudinaryUrl))
        {
            _logger.LogWarning("Cloudinary URL is not configured. File uploads will fail in production.");
            // We initialize with dummy to avoid crashes on startup, but uploads will fail.
            _cloudinary = new Cloudinary(new Account("dummy", "dummy", "dummy"));
        }
        else
        {
            _cloudinary = new Cloudinary(cloudinaryUrl);
            _cloudinary.Api.Secure = true; // Always return HTTPS URLs
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty or null");

        using var stream = file.OpenReadStream();
        
        // Detect if it's a PDF or Image based on extension/content type
        bool isPdf = file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || 
                     file.ContentType.Contains("pdf");

        if (isPdf)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"{_envPrefix}/{folderName}",
                PublicId = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult?.SecureUrl?.ToString() ?? throw new Exception("Failed to upload PDF to Cloudinary");
        }
        else
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"{_envPrefix}/{folderName}",
                PublicId = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult?.SecureUrl?.ToString() ?? throw new Exception("Failed to upload Image to Cloudinary");
        }
    }

    public async Task<string> UploadFileAsync(string filePath, string fileName, string folderName)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Physical file not found for upload", filePath);

        bool isPdf = fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

        if (isPdf)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(filePath),
                Folder = $"{_envPrefix}/{folderName}",
                PublicId = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult?.SecureUrl?.ToString() ?? throw new Exception("Failed to upload physical PDF to Cloudinary");
        }
        else
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(filePath),
                Folder = $"{_envPrefix}/{folderName}",
                PublicId = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult?.SecureUrl?.ToString() ?? throw new Exception("Failed to upload physical Image to Cloudinary");
        }
    }
}
