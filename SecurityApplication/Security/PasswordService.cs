using Microsoft.AspNetCore.Identity;
using SecurityApplication.Data;

namespace SecurityApplication.Security
{
    public class PasswordService(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        // Nadanie hasła jednorazowego przez administratora (nowe konto lub reset).
        // Hasło jednorazowe nie przechodzi przez politykę haseł - użytkownik i tak musi je zmienić.
        public async Task<(string Password, DateTime ExpiresAt)> SetOneTimePasswordAsync(ApplicationUser user)
        {
            var password = OneTimePasswordGenerator.Generate();
            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, password);
            user.MustChangePassword = true;
            user.OneTimePasswordExpiresAt = DateTime.UtcNow.Add(OneTimePasswordGenerator.Validity);
            // nowy security stamp unieważnia aktywne sesje użytkownika
            await userManager.UpdateSecurityStampAsync(user);
            return (password, user.OneTimePasswordExpiresAt.Value);
        }

        // Zmiana hasła przez użytkownika - wymaga starego hasła.
        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string oldPassword, string newPassword)
        {
            var oldHash = user.PasswordHash;
            var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                await OnPasswordChangedAsync(user, oldHash);
            }
            return result;
        }

        // Wymuszona zmiana hasła (pierwsze logowanie / wygasłe hasło) - użytkownik jest już uwierzytelniony.
        public async Task<IdentityResult> ForceChangePasswordAsync(ApplicationUser user, string newPassword)
        {
            var oldHash = user.PasswordHash;
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                await OnPasswordChangedAsync(user, oldHash);
            }
            return result;
        }

        public static bool IsPasswordChangeRequired(ApplicationUser user, SecuritySettings settings) =>
            user.MustChangePassword
            || (settings.PasswordValidityDays > 0
                && user.PasswordChangedAt is { } changed
                && changed.AddDays(settings.PasswordValidityDays) < DateTime.UtcNow);

        private async Task OnPasswordChangedAsync(ApplicationUser user, string? oldHash)
        {
            if (oldHash is not null)
            {
                db.PasswordHistory.Add(new PasswordHistory { UserId = user.Id, PasswordHash = oldHash, CreatedAt = DateTime.UtcNow });
            }
            user.MustChangePassword = false;
            user.OneTimePasswordExpiresAt = null;
            user.PasswordChangedAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        }
    }
}
