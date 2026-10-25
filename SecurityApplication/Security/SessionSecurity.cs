using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SecurityApplication.Data;

namespace SecurityApplication.Security
{
    public static class SessionSecurity
    {
        private const string LastActivityKey = ".last_activity";
        public const string TimeoutItemKey = "SessionTimeoutMinutes";
        public static async Task ValidatePrincipalAsync(
            CookieValidatePrincipalContext context, Func<CookieValidatePrincipalContext, Task> validateSecurityStamp)
        {
            await validateSecurityStamp(context);
            if (context.Principal is null)
            {
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var timeoutMinutes = (await db.GetSettingsAsync()).SessionTimeoutMinutes;
            var timeout = TimeSpan.FromMinutes(timeoutMinutes);
            var now = DateTimeOffset.UtcNow;

            if (context.Properties.Items.TryGetValue(LastActivityKey, out var value)
                && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var lastActivity)
                && now - lastActivity > timeout)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                return;
            }

            context.Properties.Items[LastActivityKey] = now.ToString("O", CultureInfo.InvariantCulture);
            context.ShouldRenew = true;
            context.HttpContext.Items[TimeoutItemKey] = timeoutMinutes;
        }
        public static IApplicationBuilder UseForcedPasswordChange(this IApplicationBuilder app) =>
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path;
                if (context.User.HasClaim(c => c.Type == AppClaims.PasswordChangeRequired)
                    && !path.StartsWithSegments("/Account/ForceChangePassword")
                    && !path.StartsWithSegments("/Account/Logout")
                    && !path.StartsWithSegments("/_framework")
                    && !Path.HasExtension(path.Value))
                {
                    context.Response.Redirect("/Account/ForceChangePassword");
                    return;
                }
                await next();
            });
    }
}
