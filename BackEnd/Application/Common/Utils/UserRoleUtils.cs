using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LawyerProject.Application.Common.Utils
{
    public static class UserRoleUtils
    {
        /// <summary>
        /// Retrieves the role ID based on the role name.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="roleName">The normalized role name to search for.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation if needed.</param>
        /// <returns>
        /// The role ID if the role is found, otherwise null.
        /// </returns>
        public static async Task<string?> GetRoleIdByRoleNameAsync(IApplicationDbContext context, string roleName, CancellationToken cancellationToken)
        {
            var normalizedRoleName = roleName.ToUpper();

            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);

            if (role == null)
            {
                Console.WriteLine($"Role not found for: {roleName}");
                return null;
            }

            Console.WriteLine($"Role found for {roleName}: RoleId = {role.Id}");
            return role.Id;
        }
    }
}
