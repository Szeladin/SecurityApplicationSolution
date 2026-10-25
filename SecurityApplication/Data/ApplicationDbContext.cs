using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SecurityApplication.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<SecuritySettings> SecuritySettings => Set<SecuritySettings>();
        public DbSet<ForbiddenWord> ForbiddenWords => Set<ForbiddenWord>();
        public DbSet<PasswordHistory> PasswordHistory => Set<PasswordHistory>();
        public DbSet<Document> Documents => Set<Document>();

        public Task<SecuritySettings> GetSettingsAsync() => SecuritySettings.AsNoTracking().SingleAsync(s => s.Id == 1);

        private static readonly string[] DefaultForbiddenWords =
        [
            "firma", "uczelnia", "securityapplication",
            "styczeń", "luty", "marzec", "kwiecień", "maj", "czerwiec",
            "lipiec", "sierpień", "wrzesień", "październik", "listopad", "grudzień",
            "wiosna", "lato", "jesień", "zima",
        ];

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SecuritySettings>().HasData(new SecuritySettings { Id = 1 });

            builder.Entity<ForbiddenWord>().HasIndex(w => w.Word).IsUnique();
            builder.Entity<ForbiddenWord>().HasData(
                DefaultForbiddenWords.Select((w, i) => new ForbiddenWord { Id = i + 1, Word = w }));

            builder.Entity<PasswordHistory>().HasIndex(h => h.UserId);
        }
    }
}
