
using DigitalnePozivnice.Data;
using DigitalnePozivnice.Data.Generated;
using DigitalnePozivnice.Helpers;
using DigitalnePozivnice.Localization;
using DigitalnePozivnice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Podaci za uplatu i spisak admina (izdvojeno da se ne mijesa sa konekcijom)
builder.Configuration.AddJsonFile("appsettings.Placanje.json", optional: true, reloadOnChange: true);
// Podesavanja slanja emailova (SMTP) - ne ide na GitHub
builder.Configuration.AddJsonFile("appsettings.Email.json", optional: true, reloadOnChange: true);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Konekcija DefaultConnection nije pronađena.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<DigitalnePozivniceScaffoldContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<PlacanjeOptions>(builder.Configuration.GetSection("Placanje"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<AdminProvjera>();
builder.Services.AddSingleton<IAuthorizationHandler, AdminHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p.RequireAuthenticatedUser().AddRequirements(new AdminRequirement()));

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<GeoJezik>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSiteLanguage();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();