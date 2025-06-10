using System;

namespace LawyerProject.Application.Files.Queries;

public class FileSearchRequest
{
    public string? FileName { get; set; }

    public Guid? CaseId { get; set; }

    public string? UploadedById { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? ContentType { get; set; }

    public bool IncludeDeleted { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
