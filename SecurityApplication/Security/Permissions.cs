namespace SecurityApplication.Security
{
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string Kierownik = "Kierownik";
        public const string Redaktor = "Redaktor";
        public const string Pracownik = "Pracownik";
        public const string Czytelnik = "Czytelnik";

        // Rolę Administrator ma wyłącznie stałe konto ADMIN - nie można jej nadać w panelu.
        public static readonly string[] Assignable = [Kierownik, Redaktor, Pracownik, Czytelnik];
    }

    public static class Permissions
    {
        public const string ClaimType = "permission";

        public const string UsersManage = "users.manage";
        public const string SettingsManage = "settings.manage";
        public const string DocumentsRead = "documents.read";
        public const string DocumentsCreate = "documents.create";
        public const string DocumentsEdit = "documents.edit";
        public const string DocumentsDelete = "documents.delete";

        public static readonly string[] All =
            [UsersManage, SettingsManage, DocumentsRead, DocumentsCreate, DocumentsEdit, DocumentsDelete];

        // RBAC: uprawnienia przypisane rolom (zasada najmniejszych uprawnień).
        // Zapisywane w bazie jako claimy ról (AspNetRoleClaims) przy starcie aplikacji.
        public static readonly IReadOnlyDictionary<string, string[]> ByRole = new Dictionary<string, string[]>
        {
            [Roles.Administrator] = [UsersManage, SettingsManage],
            [Roles.Kierownik] = [DocumentsRead, DocumentsCreate, DocumentsEdit, DocumentsDelete],
            [Roles.Redaktor] = [DocumentsRead, DocumentsCreate, DocumentsEdit],
            [Roles.Pracownik] = [DocumentsRead, DocumentsCreate],
            [Roles.Czytelnik] = [DocumentsRead],
        };
    }

    public static class AppClaims
    {
        // Obecny w sesji, gdy użytkownik musi zmienić hasło (pierwsze logowanie lub hasło wygasło).
        public const string PasswordChangeRequired = "pwd_change_required";
    }
}
