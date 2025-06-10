using System;

namespace LawyerProject.Domain.Entities;

public class EncryptedFileMetadata
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid EncryptionKeyId { get; set; }
    public int CaseId { get; set; }
    public virtual Case Case { get; set; } = null!;
    public int RegionId { get; set; }
    public virtual Region Region { get; set; } = null!;
    public string UploadedById { get; set; } = string.Empty;
    public virtual User UploadedBy { get; set; } = null!;
    public DateTime UploadedDate { get; set; }
    public bool IsDeleted { get; set; }
}
