namespace LawyerProject.Application.RegionAdmin.Staff.Queries;
public class StaffDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public IList<string>? Roles { get; set; }
    public DateTime JoinDate { get; set; }
}
