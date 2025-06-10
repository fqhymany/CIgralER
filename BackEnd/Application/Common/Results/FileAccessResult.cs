using System;

namespace LawyerProject.Application.Common.Results;

public class FileAccessResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
