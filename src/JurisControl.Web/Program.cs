using JurisControl.Data;
using JurisControl.Data.Extensions;
using JurisControl.Data.Seeding;
using JurisControl.Data.TenantContext;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------- Servicios --------

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection no configurada. " +
        "En SmartASP.NET se inyecta como Environment Variable " +
        "'ConnectionStrings__DefaultConnection'.");

builder.Services.AddJurisControlData(connectionString);

// Fábrica de claims que agrega despacho_id al ClaimsPrincipal en cada login.
builder.Services.AddScoped<
    IUserClaimsPrincipalFactory<ApplicationUser>,
    DespachoUserClaimsPrincipalFactory>();

// Cookies + rutas de login/logout
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddRazorPages(options =>
{
    // Todas las páginas requieren autenticación salvo las de Account.
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Account");
});

// -------- App --------

var app = builder.Build();

// Migraciones + seed en arranque (idempotente).
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<JurisControlDbContext>();
        db.SetPlatformScope();
        await db.Database.MigrateAsync();
        await RowLevelSecurityInstaller.ApplyAsync(db);
        await DbSeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al inicializar la base de datos");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
