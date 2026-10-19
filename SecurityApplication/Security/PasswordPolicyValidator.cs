using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityApplication.Data;

namespace SecurityApplication.Security
{
    // Polityka haseł wariantu 10: minimalna długość (domyślnie 14), opcjonalnie zakaz słów
    // ze słownika organizacji, oraz brak powtórzenia poprzednich haseł.
    public class PasswordPolicyValidator(ApplicationDbContext db) : IPasswordValidator<ApplicationUser>
    {
        public const int MaxLength = 128;

        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
        {
            password ??= "";
            var settings = await db.GetSettingsAsync();
            var errors = new List<IdentityError>();

            if (password.Length < settings.MinPasswordLength)
            {
                errors.Add(Error("PasswordTooShort", $"Hasło musi mieć co najmniej {settings.MinPasswordLength} znaków."));
            }
            if (password.Length > MaxLength)
            {
                errors.Add(Error("PasswordTooLong", $"Hasło może mieć najwyżej {MaxLength} znaków."));
            }

            if (settings.ForbiddenWordsEnabled)
            {
                var normalized = Normalize(password);
                var words = await db.ForbiddenWords.Select(w => w.Word).ToListAsync();
                var found = words.Where(w => normalized.Contains(Normalize(w), StringComparison.Ordinal)).ToList();
                if (found.Count > 0)
                {
                    errors.Add(Error("PasswordForbiddenWord", $"Hasło zawiera zakazane słowo: {string.Join(", ", found)}."));
                }
            }

            if (await IsPreviousPasswordAsync(manager, user, password))
            {
                errors.Add(Error("PasswordReused", "Nowe hasło musi różnić się od poprzednich haseł."));
            }

            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed([.. errors]);
        }

        private async Task<bool> IsPreviousPasswordAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
        {
            var hashes = await db.PasswordHistory
                .Where(h => h.UserId == user.Id)
                .Select(h => h.PasswordHash)
                .ToListAsync();
            if (user.PasswordHash is not null)
            {
                hashes.Add(user.PasswordHash);
            }

            return hashes.Any(hash =>
                manager.PasswordHasher.VerifyHashedPassword(user, hash, password) != PasswordVerificationResult.Failed);
        }

        // Porównanie bez względu na wielkość liter i polskie znaki (np. "Styczen" == "styczeń").
        public static string Normalize(string value)
        {
            var decomposed = value.ToLowerInvariant().Replace('ł', 'l').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposed.Length);
            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static IdentityError Error(string code, string description) => new() { Code = code, Description = description };
    }
}
