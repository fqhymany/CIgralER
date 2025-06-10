using System.Threading.Tasks;

namespace LawyerProject.Application.Common.Interfaces;

public interface IPermissionChecker
{
    Task<bool> HasSectionPermissionAsync(string userId, string sectionName, SectionPermissionType permissionType);
}
