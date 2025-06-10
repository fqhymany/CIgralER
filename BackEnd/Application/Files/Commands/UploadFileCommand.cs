using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Results;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Application.Files.Queries;
using MediatR;

namespace LawyerProject.Application.Files.Commands;

public record UploadFileCommand : IRequest<FileResult>
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string CustomName { get; init; } = string.Empty;
    public Guid CaseId { get; init; }
    public string Area { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, FileResult>
{
    private readonly IEncryptionService _encryptionService;

    public UploadFileCommandHandler(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public async Task<FileResult> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var fileId = Guid.NewGuid();
        var intCaseId = CastDataTypesUtils.ConvertGuidToInt(request.CaseId.ToString());
        var displayName = string.IsNullOrEmpty(request.CustomName) ? request.FileName : request.CustomName;

        var result = await _encryptionService.EncryptAndSaveAsync(
            request.FileStream,
            fileId,
            request.FileName,
            displayName,
            request.Area,
            intCaseId,
            request.UserId,
            cancellationToken);

        if (!result.Succeeded)
        {
            return new FileResult
            {
                Succeeded = false,
                Error = result.Error
            };
        }

        return new FileResult
        {
            Succeeded = true,
            Data = new FileDto
            {
                FileId = fileId,
                FileName = displayName,
                FilePath = result.FilePath ?? string.Empty
            }
        };
    }
}
