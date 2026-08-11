using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EMR.Application.Interfaces;

public interface ICloudStorageService
{
    /// <summary>
    /// Uploads a file to Cloud storage and returns the public URL.
    /// </summary>
    Task<string> UploadFileAsync(IFormFile file, string folderName);

    /// <summary>
    /// Uploads a file from a physical path to Cloud storage and returns the public URL.
    /// </summary>
    Task<string> UploadFileAsync(string filePath, string fileName, string folderName);
}
