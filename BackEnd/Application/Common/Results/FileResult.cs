using System;
using System.Collections.Generic;

namespace LawyerProject.Application.Common.Results;

public class FileResult
{
    public bool Succeeded { get; set; }

    public string? Error { get; set; }
    
    public FileDto? Data { get; set; }
}
