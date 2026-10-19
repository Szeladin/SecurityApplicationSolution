using Microsoft.AspNetCore.Identity;

namespace SecurityApplication.Security
{
    public class PolishIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordMismatch() =>
            new() { Code = nameof(PasswordMismatch), Description = "Obecne hasło jest niepoprawne." };

        public override IdentityError DuplicateUserName(string userName) =>
            new() { Code = nameof(DuplicateUserName), Description = $"Login '{userName}' jest już zajęty." };

        public override IdentityError InvalidUserName(string? userName) =>
            new() { Code = nameof(InvalidUserName), Description = "Login może zawierać tylko litery, cyfry oraz znaki - . _ @ +" };

        public override IdentityError InvalidToken() =>
            new() { Code = nameof(InvalidToken), Description = "Nieprawidłowy token." };
    }
}
