namespace LawyerProject.Application.RegionAdmin.Staff.Queries;

public class StaffDetailsDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
    public DateTime JoinDate { get; set; }
}
