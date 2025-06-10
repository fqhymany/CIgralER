using System;

namespace LawyerProject.Domain.Entities;

public class FileAccessToken
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsUsed { get; set; }
}
