using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityApplication.Components;
using SecurityApplication.Components.Account;
using SecurityApplication.Data;
using SecurityApplication.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<PasswordService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    // Górny limit życia ciasteczka; właściwy limit bezczynności liczy SessionSecurity (ustawienie admina).
    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    var validateSecurityStamp = options.Events.OnValidatePrincipal;
    options.Events.OnValidatePrincipal = context => SessionSecurity.ValidatePrincipalAsync(context, validateSecurityStamp);
});

// Security stamp sprawdzany przy każdym żądaniu - wylogowanie, blokada lub reset hasła
// natychmiast unieważniają sesję, a zmiana roli od razu zmienia uprawnienia.
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

var authorization = builder.Services.AddAuthorizationBuilder();
foreach (var permission in Permissions.All)
{
    authorization.AddPolicy(permission, policy => policy.RequireClaim(Permissions.ClaimType, permission));
}

// PBKDF2-HMAC-SHA512 (format v3 ASP.NET Identity) - liczba iteracji wg OWASP Password Storage Cheat Sheet.
// Starsze hashe (100 000 iteracji) są automatycznie przeliczane przy najbliższym poprawnym logowaniu.
builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210_000);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        // Version2 - schemat bez tabeli passkeys (passkeys nie są używane).
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version2;

        // Reguły złożoności wyłączone (NIST SP 800-63B-4) - politykę realizuje PasswordPolicyValidator.
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 1;
        options.Password.RequiredUniqueChars = 1;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddPasswordValidator<PasswordPolicyValidator>()
    .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>()
    .AddErrorDescriber<PolishIdentityErrorDescriber>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseForcedPasswordChange();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

await DbSeeder.SeedAsync(app.Services, app.Configuration);

app.Run();
