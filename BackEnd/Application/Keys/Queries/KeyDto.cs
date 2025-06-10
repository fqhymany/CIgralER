using System;

namespace LawyerProject.Application.Keys.Queries;

public class KeyDto
{
    public Guid Id { get; set; }
    public string KeyType { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsActive { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
