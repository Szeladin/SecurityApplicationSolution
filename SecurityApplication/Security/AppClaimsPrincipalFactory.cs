using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SecurityApplication.Data;

namespace SecurityApplication.Security
{
    // Do sesji trafiają role i ich uprawnienia (claimy ról) oraz znacznik wymaganej zmiany hasła.
    public class AppClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options,
        ApplicationDbContext db)
        : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
    {
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            if (PasswordService.IsPasswordChangeRequired(user, await db.GetSettingsAsync()))
            {
                identity.AddClaim(new Claim(AppClaims.PasswordChangeRequired, "true"));
            }
            return identity;
        }
    }
}
