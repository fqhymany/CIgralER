using System;

namespace LawyerProject.Application.Common.Models;

public class EncryptedData
{
    public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();
    public byte[] IV { get; set; } = Array.Empty<byte>();
    public byte[] Salt { get; set; } = Array.Empty<byte>();
    public byte[] AuthTag { get; set; } = Array.Empty<byte>();
    public byte[] EncryptedFileKey { get; set; } = Array.Empty<byte>();
    public byte[] DigitalSignature { get; set; } = Array.Empty<byte>();
}
