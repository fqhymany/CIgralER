using LawyerProject.Application.Common.Interfaces; 

namespace LawyerProject.Application.Common.Utils
{
    //این کد بایستی تغییر کند و برخی موارد از سشن بیاد بجای اینکه ما در ساختار از پروژه اطلاعات را بگیریم
    public class UserUtils
    {
        private static IApplicationDbContext? _context;
        /// <summary>
        /// Retrieves detailed user information based on user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A UserDetails object containing user information.</returns>
        public static UserDetails? GetUserDetails(IApplicationDbContext context,string? userId)
        {
            _context = context;
            if (_context == null) return null;
            if (userId == null) return null;
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.", nameof(userId));
            }
            var regionUser = _context.RegionsUsers.FirstOrDefault(ru => ru.UserId == userId);
            int? regionId = regionUser?.RegionId; 

            var userRole = _context.UsersRoles.Where(ur => ur.UserId == userId).FirstOrDefault();
            string? roleId = userRole?.RoleId;
            string? roleName = _context.Roles.Where(ur => ur.Id == roleId).FirstOrDefault()?.NormalizedName;
            string fullName = $"{user.FirstName} {user.LastName}".Trim();
            string normalizedUserName = user.NormalizedUserName ?? string.Empty;
            string userName = user.UserName ?? string.Empty;
            string normalizedEmail = user.NormalizedEmail ?? string.Empty;
            string nationalCode = user.NationalCode ?? string.Empty;

            return new UserDetails
            {
                Id = userId,
                NormalizedUserName = normalizedUserName,
                UserName = userName,
                FullName = fullName,
                NormalizedEmail = normalizedEmail,
                IsActive = user.IsActive,
                NationalCode = nationalCode,
                RegionId = regionId,
                RoleId = roleId,
                RoleName = roleName
            };
        }
    }

    public class UserDetails
    {
        public string Id { get; set; } = string.Empty;
        public string NormalizedUserName { get; set; } = string.Empty; 
        public string UserName { get; set; } = string.Empty; 
        public string FullName { get; set; } = string.Empty; 
        public string NormalizedEmail { get; set; } = string.Empty; 
        public bool IsActive { get; set; }
        public string NationalCode { get; set; } = string.Empty; 
        public int? RegionId { get; set; } 
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
