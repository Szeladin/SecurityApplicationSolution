using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityApplication.Security;

namespace SecurityApplication.Data
{
    public static class DbSeeder
    {
        public const string AdminUserName = "ADMIN";

        // Stałe konto ADMIN + 7 użytkowników testowych .
        private static readonly (string UserName, string FullName, string Role)[] Users =
        [
            (AdminUserName, "Administrator systemu", Roles.Administrator),
            ("kierownik1", "Anna Kowalska", Roles.Kierownik),
            ("redaktor1", "Piotr Nowak", Roles.Redaktor),
            ("redaktor2", "Ewa Wiśniewska", Roles.Redaktor),
            ("pracownik1", "Tomasz Wójcik", Roles.Pracownik),
            ("pracownik2", "Magdalena Kamińska", Roles.Pracownik),
            ("czytelnik1", "Jan Lewandowski", Roles.Czytelnik),
            ("czytelnik2", "Katarzyna Zielińska", Roles.Czytelnik),
        ];

        public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await db.Database.MigrateAsync();

            // Role i przypisane im uprawnienia (synchronizowane z Permissions.ByRole).
            foreach (var (roleName, permissions) in Permissions.ByRole)
            {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role is null)
                {
                    role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);
                }

                var current = (await roleManager.GetClaimsAsync(role))
                    .Where(c => c.Type == Permissions.ClaimType).ToList();
                foreach (var claim in current.Where(c => !permissions.Contains(c.Value)))
                {
                    await roleManager.RemoveClaimAsync(role, claim);
                }
                foreach (var permission in permissions.Where(p => current.All(c => c.Value != p)))
                {
                    await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
                }
            }

            // Konta tworzone z hasłem początkowym z konfiguracji - przy pierwszym logowaniu wymuszana jest zmiana.
            var initialPassword = configuration["Seed:InitialPassword"];
            if (string.IsNullOrEmpty(initialPassword))
            {
                return;
            }

            foreach (var (userName, fullName, role) in Users)
            {
                if (await userManager.FindByNameAsync(userName) is not null)
                {
                    continue;
                }

                var user = new ApplicationUser { UserName = userName, FullName = fullName, MustChangePassword = true };
                user.PasswordHash = userManager.PasswordHasher.HashPassword(user, initialPassword);
                await userManager.CreateAsync(user);
                await userManager.AddToRoleAsync(user, role);
            }

            if (!await db.Documents.AnyAsync())
            {
                db.Documents.AddRange(
                    new Document { Title = "Regulamin pracy", Content = "Przykładowy dokument dostępny dla wszystkich ról z uprawnieniem odczytu.", AuthorUserName = "kierownik1", CreatedAt = DateTime.UtcNow },
                    new Document { Title = "Plan publikacji", Content = "Lista artykułów do przygotowania przez redakcję.", AuthorUserName = "redaktor1", CreatedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
            }
        }
    }
}
