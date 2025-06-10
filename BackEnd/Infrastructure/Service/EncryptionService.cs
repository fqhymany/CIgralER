using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;
using LawyerProject.Application.Common.Results;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Application.Keys.Queries;
using LawyerProject.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using File = System.IO.File;

namespace LawyerProject.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly IUser _currentUser;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EncryptionService> _logger;

    private const int PBKDF2_ITERATIONS = 10000;
    private const int KEY_SIZE_BYTES = 32;
    private const int IV_SIZE = 12;
    private const int SALT_SIZE = 32;
    private const int AUTH_TAG_SIZE = 16;

    public EncryptionService(
        IApplicationDbContext dbContext,
        IAuditService auditService,
        IUser currentUserService,
        IConfiguration configuration,
        ILogger<EncryptionService> logger)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUserService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Encrypts the file and saves it to the file system
    /// </summary>
    public async Task<EncryptionResult> EncryptAndSaveAsync(Stream content, Guid fileId, string fileName, string displayName, string area,
        int caseId, string uploadedById, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting encryption for file {FileId}", fileId);

            // Encrypt the file content
            var encryptionResult = await EncryptAsync(content, fileId, cancellationToken);
            if (!encryptionResult.Succeeded)
            {
                return encryptionResult;
            }

            // Save the encrypted file to disk
            var encryptedData = encryptionResult.EncryptedData;
            var savePath = await SaveEncryptedFileAsync(encryptedData, fileName, area, caseId, fileId, cancellationToken);

            if (string.IsNullOrEmpty(savePath))
            {
                _logger.LogError("Failed to save encrypted file {FileId}", fileId);
                //await _auditService.LogFileAccessAsync(fileId, "EncryptAndSave", false, "Failed to save file to disk", cancellationToken);
                return new EncryptionResult { Succeeded = false, Error = "Failed to save encrypted file to disk" };
            }

            // Store file metadata in database
            var fileMetadata = new EncryptedFileMetadata
            {
                Id = fileId,
                FileName = displayName,
                FilePath = savePath,
                FileSize = encryptedData!.EncryptedContent.Length,
                EncryptionKeyId = _dbContext.EncryptionKeys
                    .Where(k => k.KeyType == "RSA-Public" && k.IsActive)
                    .OrderByDescending(k => k.CreatedDate)
                    .Select(k => k.Id)
                    .FirstOrDefault(),
                CaseId = caseId,
                RegionId = _currentUser.RegionId,
                UploadedById = uploadedById,
                UploadedDate = DateTime.UtcNow
            };

            _dbContext.EncryptedFileMetadata.Add(fileMetadata);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new EncryptionResult
            {
                Succeeded = true,
                FilePath = savePath,
                EncryptedData = encryptionResult.EncryptedData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting and saving file {FileId}", fileId);
            //await _auditService.LogFileAccessAsync(fileId, "EncryptAndSave", false, ex.Message, cancellationToken);
            return new EncryptionResult { Succeeded = false, Error = $"Encryption and save failed: {ex.Message}" };
        }
    }

    /// <summary>
    /// Retrieves and decrypts a file from the file system
    /// </summary>
    public async Task<DecryptionResult> RetrieveAndDecryptAsync(Guid fileId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting retrieval and decryption for file {FileId}", fileId);

            // Get file metadata
            var fileMetadata = _dbContext.EncryptedFileMetadata.FirstOrDefault(f => f.Id == fileId);
            if (fileMetadata == null)
            {
                _logger.LogError("File metadata not found for file {FileId}", fileId);
                //  await _auditService.LogFileAccessAsync(fileId, "RetrieveAndDecrypt", false, "File metadata not found", cancellationToken);
                return new DecryptionResult { Succeeded = false, Error = "File metadata not found" };
            }

            // Check if file exists
            var filePath = fileMetadata.FilePath;
            if (!File.Exists(filePath))
            {
                _logger.LogError("File not found on disk for file {FileId} at path {FilePath}", fileId, filePath);
                //await _auditService.LogFileAccessAsync(fileId, "RetrieveAndDecrypt", false, "File not found on disk", cancellationToken);
                return new DecryptionResult { Succeeded = false, Error = "File not found on disk" };
            }

            // Read the encrypted file
            var encryptedData = await ReadEncryptedFileAsync(filePath, cancellationToken);
            if (encryptedData == null)
            {
                _logger.LogError("Failed to read encrypted file {FileId} from disk", fileId);
                // await _auditService.LogFileAccessAsync(fileId, "RetrieveAndDecrypt", false, "Failed to read file from disk", cancellationToken);
                return new DecryptionResult { Succeeded = false, Error = "Failed to read encrypted file from disk" };
            }

            // Decrypt the file
            var decryptionResult = await DecryptAsync(encryptedData, fileId, cancellationToken);
            if (!decryptionResult.Succeeded)
            {
                return decryptionResult;
            }

            // Log successful retrieval and decryption
            //await _auditService.LogFileAccessAsync(fileId, "RetrieveAndDecrypt", true, null, cancellationToken);

            // Add file name to result
            decryptionResult.FileName = fileMetadata.FileName;

            return decryptionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving and decrypting file {FileId}", fileId);
            // await _auditService.LogFileAccessAsync(fileId, "RetrieveAndDecrypt", false, ex.Message, cancellationToken);
            return new DecryptionResult { Succeeded = false, Error = $"Retrieval and decryption failed: {ex.Message}" };
        }
    }

    /// <summary>
    /// Creates a secure temporary URL for accessing encrypted files
    /// </summary>
    public async Task<FileAccessResult> GenerateSecureAccessTokenAsync(Guid fileId, int expirationMinutes, CancellationToken cancellationToken)
    {
        try
        {
            var fileMetadata = _dbContext.EncryptedFileMetadata.FirstOrDefault(f => f.Id == fileId);
            if (fileMetadata == null)
            {
                _logger.LogError("File metadata not found for file {FileId}", fileId);
                return new FileAccessResult { Succeeded = false, Error = "File metadata not found" };
            }

            // Create a temporary access token
            var token = new FileAccessToken
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                CreatedBy = _currentUser.Id,
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            _dbContext.FileAccessTokens.Add(token);
            await _dbContext.SaveChangesAsync(cancellationToken);

            //await _auditService.LogFileAccessAsync(fileId, "GenerateAccessToken", true, null, cancellationToken);

            return new FileAccessResult
            {
                Succeeded = true,
                AccessToken = token.Id.ToString(),
                ExpiresAt = token.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating secure access token for file {FileId}", fileId);
            return new FileAccessResult { Succeeded = false, Error = $"Token generation failed: {ex.Message}" };
        }
    }

    // Original EncryptAsync method
    public async Task<EncryptionResult> EncryptAsync(Stream content, Guid fileId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting encryption for file {FileId}", fileId);
            // Get active RSA key
            var activeRsaPublicKey = _dbContext.EncryptionKeys
                .Where(k => k.KeyType == "RSA-Public" && k.IsActive)
                .OrderByDescending(k => k.CreatedDate)
                .FirstOrDefault();

            if (activeRsaPublicKey == null)
            {
                _logger.LogError("No active RSA public key found");
                //    await _auditService.LogFileAccessAsync(fileId, "Encrypt", false, "No active RSA public key found",
                //        cancellationToken);
                return new EncryptionResult { Succeeded = false, Error = "No active RSA public key found" };
            }

            // Get the master key from configuration
            string masterKeyString = _configuration["Encryption:MasterKey"] ??
                                     throw new InvalidOperationException("Master key is not configured");

            // Generate salt and derive file key
            byte[] salt = GenerateRandomBytes(SALT_SIZE);
            byte[] fileKey = PBKDF2(masterKeyString, fileId.ToString() + Convert.ToBase64String(salt),
                PBKDF2_ITERATIONS, KEY_SIZE_BYTES);

            // Generate random IV for AES-GCM
            byte[] iv = GenerateRandomBytes(IV_SIZE);

            // Convert stream to byte array
            byte[] fileContent;
            using (var memoryStream = new MemoryStream())
            {
                await content.CopyToAsync(memoryStream, cancellationToken);
                fileContent = memoryStream.ToArray();
            }

            // Encrypt file with AES-GCM
            byte[] encryptedContent;
            byte[] authTag;
            using (var aesGcm = new AesGcm(fileKey, AUTH_TAG_SIZE))
            {
                encryptedContent = new byte[fileContent.Length];
                authTag = new byte[AUTH_TAG_SIZE];
                aesGcm.Encrypt(iv, fileContent, encryptedContent, authTag);
            }

            // Encrypt the file key with RSA public key
            byte[] encryptedFileKey;
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(activeRsaPublicKey.KeyData, out _);
                encryptedFileKey = rsa.Encrypt(fileKey, RSAEncryptionPadding.OaepSHA256);
            }

            // Create digital signature
            byte[] dataToSign = ConcatenateBytes(iv, salt, authTag, encryptedContent);
            byte[] digitalSignature;

            // Get the RSA private key for signing
            var rsaPrivateKeyForSigning = _dbContext.EncryptionKeys
                .Where(k => k.KeyType == "RSA-Private" && k.IsActive)
                .OrderByDescending(k => k.CreatedDate)
                .FirstOrDefault();

            if (rsaPrivateKeyForSigning == null)
            {
                _logger.LogError("No active RSA private key found for signing");
                // await _auditService.LogFileAccessAsync(fileId, "Encrypt", false,
                //     "No active RSA private key found for signing", cancellationToken);
                return new EncryptionResult
                {
                    Succeeded = false,
                    Error = "No active RSA private key found for signing"
                };
            }

            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(rsaPrivateKeyForSigning.KeyData, out _);
                digitalSignature = rsa.SignData(dataToSign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }

            // Log successful encryption
            // await _auditService.LogFileAccessAsync(fileId, "Encrypt", true, null, cancellationToken);

            // Return encrypted data
            return new EncryptionResult
            {
                Succeeded = true,
                EncryptedData = new EncryptedData
                {
                    EncryptedContent = encryptedContent,
                    IV = iv,
                    Salt = salt,
                    AuthTag = authTag,
                    EncryptedFileKey = encryptedFileKey,
                    DigitalSignature = digitalSignature
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting file {FileId}", fileId);
            // await _auditService.LogFileAccessAsync(fileId, "Encrypt", false, ex.Message, cancellationToken);
            return new EncryptionResult { Succeeded = false, Error = $"Encryption failed: {ex.Message}" };
        }
    }

    // Original DecryptAsync method
    public Task<DecryptionResult> DecryptAsync(EncryptedData encryptedData, Guid fileId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting decryption for file {FileId}", fileId);

            // Verify digital signature first
            byte[] dataToVerify = ConcatenateBytes(
                encryptedData.IV,
                encryptedData.Salt,
                encryptedData.AuthTag,
                encryptedData.EncryptedContent
            );

            // Get the RSA public key for signature verification
            var rsaPublicKeyForVerification = _dbContext.EncryptionKeys
                .Where(k => k.KeyType == "RSA-Public" && k.IsActive)
                .OrderByDescending(k => k.CreatedDate)
                .FirstOrDefault();

            if (rsaPublicKeyForVerification == null)
            {
                _logger.LogError("No active RSA public key found for signature verification");
                // await _auditService.LogFileAccessAsync(fileId, "Decrypt", false,
                //    "No active RSA public key found for verification", cancellationToken);
                return Task.FromResult(new DecryptionResult
                {
                    Succeeded = false,
                    Error = "No active RSA public key found for verification"
                });
            }

            bool isSignatureValid;
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(rsaPublicKeyForVerification.KeyData, out _);
                isSignatureValid = rsa.VerifyData(
                    dataToVerify,
                    encryptedData.DigitalSignature,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1
                );
            }

            if (!isSignatureValid)
            {
                _logger.LogWarning("Digital signature verification failed for file {FileId}", fileId);
                //     await _auditService.LogFileAccessAsync(fileId, "Decrypt", false,
                //       "Digital signature verification failed", cancellationToken);
                return Task.FromResult(new DecryptionResult
                {
                    Succeeded = false,
                    Error = "Digital signature verification failed. File may have been tampered with."
                });
            }

            // Get active RSA private key
            var activeRsaPrivateKey = _dbContext.EncryptionKeys
                .Where(k => k.KeyType == "RSA-Private" && k.IsActive)
                .OrderByDescending(k => k.CreatedDate)
                .FirstOrDefault();

            if (activeRsaPrivateKey == null)
            {
                _logger.LogError("No active RSA private key found");
                // await _auditService.LogFileAccessAsync(fileId, "Decrypt", false, "No active RSA private key found",
                //     cancellationToken);
                return Task.FromResult(new DecryptionResult { Succeeded = false, Error = "No active RSA private key found" });
            }

            // Decrypt the file key with RSA private key
            byte[] fileKey;
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(activeRsaPrivateKey.KeyData, out _);
                fileKey = rsa.Decrypt(encryptedData.EncryptedFileKey, RSAEncryptionPadding.OaepSHA256);
            }

            // Get the master key and derive the file key for verification
            string masterKeyString = _configuration["Encryption:MasterKey"] ??
                                     throw new InvalidOperationException("Master key is not configured");

            byte[] derivedFileKey = PBKDF2(
                masterKeyString,
                fileId.ToString() + Convert.ToBase64String(encryptedData.Salt),
                PBKDF2_ITERATIONS,
                KEY_SIZE_BYTES
            );
            if (!fileKey.SequenceEqual(derivedFileKey))
            {
                _logger.LogError("Decrypted file key does not match the derived file key for file {FileId}", fileId);
                //  await _auditService.LogFileAccessAsync(fileId, "Decrypt", false, "File key integrity check failed", cancellationToken);
                return Task.FromResult(new DecryptionResult
                {
                    Succeeded = false,
                    Error = "Decryption failed: File key integrity verification failed."
                });
            }
            // Decrypt file with AES-GCM
            byte[] decryptedContent;
            try
            {
                using var aesGcm = new AesGcm(fileKey, AUTH_TAG_SIZE);
                decryptedContent = new byte[encryptedData.EncryptedContent.Length];
                aesGcm.Decrypt(
                    encryptedData.IV,
                    encryptedData.EncryptedContent,
                    encryptedData.AuthTag,
                    decryptedContent
                );
            }
            catch (CryptographicException ex)
            {
                _logger.LogWarning(ex, "AES-GCM decryption failed for file {FileId}", fileId);
                //    await _auditService.LogFileAccessAsync(fileId, "Decrypt", false, "Decryption authentication failed",
                //         cancellationToken);
                return Task.FromResult(new DecryptionResult
                {
                    Succeeded = false,
                    Error = "Decryption failed: The file may have been tampered with."
                });
            }

            if (decryptedContent == null || decryptedContent.Length == 0)
            {
                _logger.LogError("Decrypted content is empty for file {FileId}", fileId);
                return Task.FromResult(new DecryptionResult
                {
                    Succeeded = false,
                    Error = "Decrypted content is empty"
                });
            }
            // Log successful decryption
            //  await _auditService.LogFileAccessAsync(fileId, "Decrypt", true, null, cancellationToken);

            // Return decrypted data
            return Task.FromResult(new DecryptionResult
            {
                Succeeded = true,
                DecryptedData = decryptedContent,
                DecryptedStream = new MemoryStream(decryptedContent)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting file {FileId}", fileId);
            //    await _auditService.LogFileAccessAsync(fileId, "Decrypt", false, ex.Message, cancellationToken);
            return Task.FromResult(new DecryptionResult { Succeeded = false, Error = $"Decryption failed: {ex.Message}" });
        }
    }

    public async Task<KeyResult> GenerateKeyPairAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generating new RSA key pair");

            using var rsa = RSA.Create(2048);

            // Export the public key
            byte[] publicKeyData = rsa.ExportRSAPublicKey();
            var publicKey = new EncryptionKey
            {
                Id = Guid.NewGuid(),
                KeyType = "RSA-Public",
                KeyData = publicKeyData,
                CreatedDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                IsActive = true,
                Name = $"RSA-Public-{DateTime.UtcNow:yyyyMMdd}",
                Description = "Automatically generated RSA public key"
            };

            // Export the private key
            byte[] privateKeyData = rsa.ExportRSAPrivateKey();
            var privateKey = new EncryptionKey
            {
                Id = Guid.NewGuid(),
                KeyType = "RSA-Private",
                KeyData = privateKeyData,
                CreatedDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                IsActive = true,
                Name = $"RSA-Private-{DateTime.UtcNow:yyyyMMdd}",
                Description = "Automatically generated RSA private key"
            };

            // Add keys to database
            _dbContext.EncryptionKeys.Add(publicKey);
            _dbContext.EncryptionKeys.Add(privateKey);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Return the public key information
            return new KeyResult
            {
                Succeeded = true,
                Key = new KeyDto
                {
                    Id = publicKey.Id,
                    KeyType = publicKey.KeyType,
                    CreatedDate = publicKey.CreatedDate,
                    ExpirationDate = publicKey.ExpirationDate,
                    IsActive = publicKey.IsActive,
                    Name = publicKey.Name
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating RSA key pair");
            return new KeyResult { Succeeded = false, Error = $"Key generation failed: {ex.Message}" };
        }
    }

    public Task<KeyResult> GetActiveEncryptionKeysAsync(CancellationToken cancellationToken)
    {
        try
        {
            var activeKeys = _dbContext.EncryptionKeys
                .Where(k => k.IsActive && k.KeyType != "RSA-Private")
                .OrderByDescending(k => k.CreatedDate)
                .ToList();

            if (!activeKeys.Any())
            {
                _logger.LogWarning("No active encryption keys found");
                return Task.FromResult(new KeyResult { Succeeded = false, Error = "No active encryption keys found" });
            }

            var keysDto = activeKeys.Select(k => new KeyDto
            {
                Id = k.Id,
                KeyType = k.KeyType,
                CreatedDate = k.CreatedDate,
                ExpirationDate = k.ExpirationDate,
                IsActive = k.IsActive,
                Name = k.Name
            }).ToList();

            return Task.FromResult(new KeyResult { Succeeded = true, Keys = keysDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active encryption keys");
            return Task.FromResult(new KeyResult { Succeeded = false, Error = $"Failed to retrieve active keys: {ex.Message}" });
        }
    }

    public async Task<SecurityResult> RotateEncryptionKeysAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting encryption key rotation");

            // Generate new key pair
            var keyResult = await GenerateKeyPairAsync(cancellationToken);
            if (!keyResult.Succeeded)
            {
                return new SecurityResult { Succeeded = false, Error = keyResult.Error };
            }

            // Deactivate old keys
            var oldKeys = _dbContext.EncryptionKeys
                .Where(k => k.IsActive && k.CreatedDate < DateTime.UtcNow.AddMonths(-1))
                .ToList();

            foreach (var key in oldKeys)
            {
                key.IsActive = false;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new SecurityResult
            {
                Succeeded = true,
                Message = $"Successfully rotated encryption keys. Deactivated {oldKeys.Count} old keys."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating encryption keys");
            return new SecurityResult { Succeeded = false, Error = $"Key rotation failed: {ex.Message}" };
        }
    }

    public async Task<SecurityResult> RevokeKeyAsync(Guid keyId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Revoking key with ID {KeyId}", keyId);

            var key = _dbContext.EncryptionKeys.FirstOrDefault(k => k.Id == keyId);

            if (key == null)
            {
                _logger.LogWarning("Key with ID {KeyId} not found", keyId);
                return new SecurityResult { Succeeded = false, Error = $"Key with ID {keyId} not found" };
            }

            key.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully revoked key {KeyId}", keyId);
            return new SecurityResult
            {
                Succeeded = true,
                Message = $"Successfully revoked key {key.Name}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking key {KeyId}", keyId);
            return new SecurityResult { Succeeded = false, Error = $"Key revocation failed: {ex.Message}" };
        }
    }

    // Helper methods for file storage

    /// <summary>
    /// Saves the encrypted file to disk using the structured path
    /// </summary>
    private async Task<string?> SaveEncryptedFileAsync(EncryptedData? encryptedData, string fileName, string area, int caseId, Guid fileId, CancellationToken cancellationToken)
    {
        try
        {
            // Create the directory structure: ./ناحیه/سال/شناسه پرونده/
            string baseDir = _configuration["Files:StoragePath"] ?? "./Files/EncryptedFiles";
            string currentYear = DateUtils.GetCurrentPersianYear();
            string sanitizedArea = SanitizePathComponent(area);

            string directoryPath = Path.Combine(baseDir, sanitizedArea, currentYear, caseId.ToString());

            // Create directory if it doesn't exist
            Directory.CreateDirectory(directoryPath);

            // Generate a unique filename with the original extension
            string extension = Path.GetExtension(fileName);
            string safeFileName = $"{fileId}{extension}";
            string? filePath = Path.Combine(directoryPath, safeFileName);

            // Create a structured file with all the necessary encrypted data
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                // Write file format version
                await fs.WriteAsync(BitConverter.GetBytes((int)1), 0, sizeof(int), cancellationToken);

                // Write the length of each component before the data itself to make reading easier
                await WriteByteArrayWithLengthAsync(fs, encryptedData!.IV, cancellationToken);
                await WriteByteArrayWithLengthAsync(fs, encryptedData.Salt, cancellationToken);
                await WriteByteArrayWithLengthAsync(fs, encryptedData.AuthTag, cancellationToken);
                await WriteByteArrayWithLengthAsync(fs, encryptedData.EncryptedFileKey, cancellationToken);
                await WriteByteArrayWithLengthAsync(fs, encryptedData.DigitalSignature, cancellationToken);

                // Write the encrypted content
                await WriteByteArrayWithLengthAsync(fs, encryptedData.EncryptedContent, cancellationToken);
            }

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving encrypted file to disk for file {FileId}", fileId);
            return null;
        }
    }

    /// <summary>
    /// Reads an encrypted file from disk and reconstructs the EncryptedData object
    /// </summary>
    private async Task<EncryptedData?> ReadEncryptedFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                // Read file format version (با اطمینان از خواندن کامل)
                var versionBuffer = new byte[sizeof(int)];
                int bytesRead = 0;
                int remaining = sizeof(int);

                while (remaining > 0)
                {
                    int read = await fs.ReadAsync(versionBuffer, bytesRead, remaining, cancellationToken);
                    if (read <= 0)
                        throw new EndOfStreamException("End of stream reached while reading version");

                    bytesRead += read;
                    remaining -= read;
                }

                int version = BitConverter.ToInt32(versionBuffer, 0);

                if (version != 1)
                {
                    _logger.LogError("Unsupported file format version: {Version}", version);
                    return null;
                }

                // Read each component
                byte[] iv = await ReadByteArrayWithLengthAsync(fs, cancellationToken);
                byte[] salt = await ReadByteArrayWithLengthAsync(fs, cancellationToken);
                byte[] authTag = await ReadByteArrayWithLengthAsync(fs, cancellationToken);
                byte[] encryptedFileKey = await ReadByteArrayWithLengthAsync(fs, cancellationToken);
                byte[] digitalSignature = await ReadByteArrayWithLengthAsync(fs, cancellationToken);
                byte[] encryptedContent = await ReadByteArrayWithLengthAsync(fs, cancellationToken);

                return new EncryptedData
                {
                    IV = iv,
                    Salt = salt,
                    AuthTag = authTag,
                    EncryptedFileKey = encryptedFileKey,
                    DigitalSignature = digitalSignature,
                    EncryptedContent = encryptedContent
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading encrypted file from disk: {FilePath}", filePath);
            return null;
        }
    }
    private async Task WriteByteArrayWithLengthAsync(FileStream fs, byte[] data, CancellationToken cancellationToken)
    {
        await fs.WriteAsync(BitConverter.GetBytes(data.Length), 0, sizeof(int), cancellationToken);
        await fs.WriteAsync(data, 0, data.Length, cancellationToken);
    }

    private async Task<byte[]> ReadByteArrayWithLengthAsync(FileStream fs, CancellationToken cancellationToken)
    {
        // خواندن طول آرایه
        var lengthBuffer = new byte[sizeof(int)];
        int bytesRead = 0;
        int remaining = sizeof(int);

        while (remaining > 0)
        {
            int read = await fs.ReadAsync(lengthBuffer, bytesRead, remaining, cancellationToken);
            if (read <= 0)
                throw new EndOfStreamException("End of stream reached while reading length");

            bytesRead += read;
            remaining -= read;
        }

        int length = BitConverter.ToInt32(lengthBuffer, 0);

        // خواندن داده‌ها
        var data = new byte[length];
        bytesRead = 0;
        remaining = length;

        while (remaining > 0)
        {
            int read = await fs.ReadAsync(data, bytesRead, remaining, cancellationToken);
            if (read <= 0)
                throw new EndOfStreamException("End of stream reached while reading data");

            bytesRead += read;
            remaining -= read;
        }

        return data;
    }

    private string SanitizePathComponent(string component)
    {
        // Remove invalid path characters while preserving unicode
        return string.Join("_", component.Split(Path.GetInvalidPathChars()));
    }

    // Existing helper methods
    private byte[] GenerateRandomBytes(int length)
    {
        byte[] bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return bytes;
    }

    private byte[] PBKDF2(string password, string salt, int iterations, int outputBytes)
    {
        byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(outputBytes);
    }

    private byte[] ConcatenateBytes(params byte[][] arrays)
    {
        int length = arrays.Sum(a => a.Length);
        byte[] result = new byte[length];
        int offset = 0;
        foreach (byte[] array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }
        return result;
    }

}
