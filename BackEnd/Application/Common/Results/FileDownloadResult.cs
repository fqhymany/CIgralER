namespace LawyerProject.Application.Common.Results;

public class FileDownloadResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public object FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public Stream? Stream { get; set; }
}
