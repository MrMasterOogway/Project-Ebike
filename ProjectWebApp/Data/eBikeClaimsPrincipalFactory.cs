using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ProjectWebApp.Data
{
    public class eBikeClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public eBikeClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor)
                : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("FullName", user.FirstName + " " + user.LastName));
            identity.AddClaim(new Claim("EmployeeID", user.Id));
            var roleClaims = await UserManager.GetRolesAsync(user);
            foreach (var role in roleClaims)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
            identity.AddClaims(await UserManager.GetClaimsAsync(user));
            return identity;
        }
    }
}
