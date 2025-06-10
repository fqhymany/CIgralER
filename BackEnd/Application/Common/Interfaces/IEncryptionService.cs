using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Models;
using LawyerProject.Application.Common.Results;

namespace LawyerProject.Application.Common.Interfaces;

public interface IEncryptionService
{

    Task<EncryptionResult> EncryptAsync(Stream content, Guid fileId, CancellationToken cancellationToken);

    Task<DecryptionResult> DecryptAsync(EncryptedData encryptedData, Guid fileId, CancellationToken cancellationToken);

    Task<KeyResult> GenerateKeyPairAsync(CancellationToken cancellationToken);

    Task<KeyResult> GetActiveEncryptionKeysAsync(CancellationToken cancellationToken);

    Task<SecurityResult> RotateEncryptionKeysAsync(CancellationToken cancellationToken);

    Task<SecurityResult> RevokeKeyAsync(Guid keyId, CancellationToken cancellationToken);

    Task<EncryptionResult> EncryptAndSaveAsync(Stream content, Guid fileId, string fileName, string displayName, string area, int caseId, string uploadedById,
        CancellationToken cancellationToken);

    Task<DecryptionResult> RetrieveAndDecryptAsync(Guid fileId, CancellationToken cancellationToken);

    Task<FileAccessResult> GenerateSecureAccessTokenAsync(Guid fileId, int expirationMinutes, CancellationToken cancellationToken);
}
