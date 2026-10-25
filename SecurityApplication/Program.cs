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

    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    var validateSecurityStamp = options.Events.OnValidatePrincipal;
    options.Events.OnValidatePrincipal = context => SessionSecurity.ValidatePrincipalAsync(context, validateSecurityStamp);
});


builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

var authorization = builder.Services.AddAuthorizationBuilder();
foreach (var permission in Permissions.All)
{
    authorization.AddPolicy(permission, policy => policy.RequireClaim(Permissions.ClaimType, permission));
}

//Konfiguracja hashowania haseł
builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210_000);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//konfiguracja Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version2;

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
