using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyerProject.Domain.Entities;

public class FileAccessLog
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid FileId { get; set; }

    [ForeignKey("FileId")]
    public virtual EncryptedFileMetadata File { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    public string ActionType { get; set; } = null!; // Upload, Download, Encrypt, Decrypt, Delete

    public DateTime AccessTime { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public bool IsSuccessful { get; set; }

    public string? ErrorMessage { get; set; }
}
