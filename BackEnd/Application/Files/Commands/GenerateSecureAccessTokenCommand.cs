using LawyerProject.Application.Common.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Results;
using MediatR;

namespace LawyerProject.Application.Files.Commands;

public record GenerateSecureAccessTokenCommand : IRequest<FileAccessTokenResult>
{
    public Guid FileId { get; init; }
    public int ExpirationMinutes { get; init; } = 30;
}

public class GenerateSecureAccessTokenCommandHandler : IRequestHandler<GenerateSecureAccessTokenCommand, FileAccessTokenResult>
{
    private readonly IEncryptionService _encryptionService;

    public GenerateSecureAccessTokenCommandHandler(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public async Task<FileAccessTokenResult> Handle(GenerateSecureAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _encryptionService.GenerateSecureAccessTokenAsync(
            request.FileId,
            request.ExpirationMinutes,
            cancellationToken);

        return new FileAccessTokenResult
        {
            Succeeded = result.Succeeded,
            Error = result.Error,
            AccessToken = result.AccessToken,
            ExpiresAt = result.ExpiresAt
        };
    }
}
