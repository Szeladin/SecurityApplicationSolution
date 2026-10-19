using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SecurityApplication.Data
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; } = "";

        // Blokada nadawana ręcznie przez administratora (niezależna od lockoutu po nieudanych próbach).
        public bool IsBlocked { get; set; }

        // Hasło nadane przez administratora (jednorazowe) - przy logowaniu wymuszana jest zmiana.
        public bool MustChangePassword { get; set; }

        // Termin ważności hasła jednorazowego (UTC); null = bez limitu (konta testowe z seedera).
        public DateTime? OneTimePasswordExpiresAt { get; set; }

        // Data ostatniej zmiany hasła przez użytkownika (UTC) - do liczenia ważności hasła.
        public DateTime? PasswordChangedAt { get; set; }
    }
}
