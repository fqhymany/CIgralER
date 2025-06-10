using LawyerProject.Application.Common.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Results;
using MediatR;

namespace LawyerProject.Application.Files.Queries;

public record ValidateAccessTokenQuery : IRequest<FileDownloadResult>
{
    public Guid TokenId { get; init; }
}

public class ValidateAccessTokenQueryHandler : IRequestHandler<ValidateAccessTokenQuery, FileDownloadResult>
{
    private readonly IEncryptionService _encryptionService;
    private readonly IApplicationDbContext _dbContext;

    public ValidateAccessTokenQueryHandler(
        IEncryptionService encryptionService,
        IApplicationDbContext dbContext)
    {
        _encryptionService = encryptionService;
        _dbContext = dbContext;
    }

    public async Task<FileDownloadResult> Handle(ValidateAccessTokenQuery request, CancellationToken cancellationToken)
    {
        var token = await _dbContext.FileAccessTokens
            .FirstOrDefaultAsync(t => t.Id == request.TokenId, cancellationToken);

        if (token == null)
        {
            return new FileDownloadResult
            {
                Succeeded = false,
                Error = "Token not found"
            };
        }

        if (token.ExpiresAt < DateTime.UtcNow)
        {
            return new FileDownloadResult
            {
                Succeeded = false,
                Error = "Token has expired"
            };
        }

        if (token.IsUsed)
        {
            return new FileDownloadResult
            {
                Succeeded = false,
                Error = "Token has already been used"
            };
        }

        var result = await _encryptionService.RetrieveAndDecryptAsync(token.FileId, cancellationToken);

        if (!result.Succeeded)
        {
            return new FileDownloadResult
            {
                Succeeded = false,
                Error = result.Error
            };
        }

        // اختیاری: علامت‌گذاری توکن به عنوان استفاده شده
        // token.IsUsed = true;
        // await _dbContext.SaveChangesAsync(cancellationToken);

        return new FileDownloadResult
        {
            Succeeded = true,
            FileName = result.FileName ?? "file",
            ContentType = "application/octet-stream",
            Stream = result.DecryptedStream
        };
    }
}
