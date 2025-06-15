using LawyerProject.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace LawyerProject.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
        _baseUrl = _configuration["FileStorage:BaseUrl"] ?? "https://localhost:5001";
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string folderPath,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderPath);

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
        }

        return $"{_baseUrl}/uploads/{folderPath}/{fileName}";
    }

    public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var relativePath = uri.AbsolutePath.TrimStart('/');
            var filePath = Path.Combine(_environment.WebRootPath, relativePath);

            if (File.Exists(filePath))
            {
                // اصلاح: استفاده از async file operation
                await Task.Run(() => File.Delete(filePath), cancellationToken);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    // خط 68 - متد DownloadFileAsync  
    public async Task<Stream> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(fileUrl);
        var relativePath = uri.AbsolutePath.TrimStart('/');
        var filePath = Path.Combine(_environment.WebRootPath, relativePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found");

        // اصلاح: استفاده از async read
        var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        return new MemoryStream(fileBytes);
    }
}
