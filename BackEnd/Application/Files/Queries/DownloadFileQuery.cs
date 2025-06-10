using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Results;
using MediatR;

namespace LawyerProject.Application.Files.Queries;

public record DownloadFileQuery : IRequest<FileDownloadResult>
{
    public Guid FileId { get; init; }
}

public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, FileDownloadResult>
{
    private readonly IEncryptionService _encryptionService;

    public DownloadFileQueryHandler(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public async Task<FileDownloadResult> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
    {
        var result = await _encryptionService.RetrieveAndDecryptAsync(request.FileId, cancellationToken);

        if (!result.Succeeded)
        {
            return new FileDownloadResult
            {
                Succeeded = false,
                Error = result.Error
            };
        }           

        return new FileDownloadResult
        {
            Succeeded = true,
            FileName = result.FileName ?? "file",
            ContentType = "application/octet-stream",
            Stream = result.DecryptedStream
        };
    }
}
