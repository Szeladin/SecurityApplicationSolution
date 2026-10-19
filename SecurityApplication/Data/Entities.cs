using System.ComponentModel.DataAnnotations;

namespace SecurityApplication.Data
{
    // Ustawienia bezpieczeństwa edytowane w panelu administratora (zawsze jeden wiersz, Id = 1).
    public class SecuritySettings
    {
        public int Id { get; set; }

        [Range(8, 64)]
        public int MinPasswordLength { get; set; } = 14;

        // Reguła z wariantu 10: hasło nie może zawierać słów ze słownika zakazanych.
        public bool ForbiddenWordsEnabled { get; set; } = true;

        // 0 = hasło nie wygasa (zgodnie z NIST SP 800-63B-4).
        [Range(0, 365)]
        public int PasswordValidityDays { get; set; }

        [Range(1, 480)]
        public int SessionTimeoutMinutes { get; set; } = 15;
    }

    public class ForbiddenWord
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Word { get; set; } = "";
    }

    // Hashe poprzednich haseł - nowe hasło nie może się z żadnym z nich pokrywać.
    public class PasswordHistory
    {
        public int Id { get; set; }

        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; } = default!;

        public string PasswordHash { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }

    // Zasób, na którym demonstrowane są uprawnienia ról.
    public class Document
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Podaj tytuł.")]
        [MaxLength(200)]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Podaj treść.")]
        [MaxLength(4000)]
        public string Content { get; set; } = "";

        [MaxLength(256)]
        public string AuthorUserName { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
