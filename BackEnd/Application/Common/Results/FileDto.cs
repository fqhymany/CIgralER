using System;

namespace LawyerProject.Application.Common.Results;

public class FileDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedDate { get; set; }

    public string Area { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? UploadedBy { get; set; }
}
