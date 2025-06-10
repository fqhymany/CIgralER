namespace LawyerProject.Application.RegionAdmin.Staff.Queries;
public class StaffListVm
{
    public int RegionId { get; set; }
    public List<StaffDto> Staff { get; set; } = new();
}
