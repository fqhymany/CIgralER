using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Results;
using LawyerProject.Application.Files.Queries;
using Microsoft.AspNetCore.Http;

namespace LawyerProject.Application.Common.Interfaces;

public interface IFileService
{
    Task<FileResult> UploadFileAsync(IFormFile file, Guid? caseId, CancellationToken cancellationToken);

    Task<FileResult> GetFileAsync(Guid fileId, CancellationToken cancellationToken);

    Task<FileResult> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken);

    Task<FileResult> GetFilesByUserAsync(string userId, CancellationToken cancellationToken);

    Task<FileResult> GetFilesByCaseAsync(Guid caseId, CancellationToken cancellationToken);

    Task<FileResult> RestoreFileAsync(Guid fileId, CancellationToken cancellationToken);

    Task<FileResult> SearchFilesAsync(FileSearchRequest request, CancellationToken cancellationToken);
}
