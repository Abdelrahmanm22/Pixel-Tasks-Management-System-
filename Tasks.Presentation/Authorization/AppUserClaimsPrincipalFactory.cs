using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Tasks.Domain.Authorization;
using Tasks.Domain.Models.Identity;

namespace Tasks.Presentation.Authorization
{
    public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, IdentityRole>
    {
        public AppUserClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            var roles = await UserManager.GetRolesAsync(user);
            var permissions = RolePermissions.GetPermissions(roles);

            foreach (var permission in permissions)
                identity.AddClaim(new Claim(Permissions.ClaimType, permission));

            return identity;
        }
    }
}
