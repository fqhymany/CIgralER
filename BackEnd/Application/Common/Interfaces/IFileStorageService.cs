namespace LawyerProject.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string folderPath,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
