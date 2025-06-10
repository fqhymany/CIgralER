using LawyerProject.Application.Common.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Results;
using MediatR;

namespace LawyerProject.Application.Files.Queries;

public record GetImageQuery : IRequest<FileDownloadResult>
{
    public Guid FileId { get; init; }
}

public class GetImageQueryHandler : IRequestHandler<GetImageQuery, FileDownloadResult>
{
    private readonly IEncryptionService _encryptionService;
    private readonly IApplicationDbContext _dbContext;

    public GetImageQueryHandler(
        IEncryptionService encryptionService,
        IApplicationDbContext dbContext)
    {
        _encryptionService = encryptionService;
        _dbContext = dbContext;
    }

    public async Task<FileDownloadResult> Handle(GetImageQuery request, CancellationToken cancellationToken)
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

        // تشخیص نوع محتوا بر اساس پسوند فایل
        string contentType = "image/jpeg"; // پیش‌فرض
        if (!string.IsNullOrEmpty(result.FileName))
        {
            string extension = Path.GetExtension(result.FileName).ToLowerInvariant();
            contentType = extension switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
        }

        return new FileDownloadResult
        {
            Succeeded = true,
            FileName = result.FileName ?? "image",
            ContentType = contentType,
            Stream = result.DecryptedStream
        };
    }
}
