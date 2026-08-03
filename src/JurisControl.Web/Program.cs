using JurisControl.Data;
using JurisControl.Data.Extensions;
using JurisControl.Data.Seeding;
using JurisControl.Data.TenantContext;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// ------------------ Bootstrap logging ------------------
// Fase 0: si la app crashea al arrancar (500.30), el error queda escrito a
// App_Data/logs/startup-YYYY-MM-DD.log para que se pueda leer por FTP.
// El try/catch envuelve TODO — desde builder hasta app.Run().

var logsDir = Path.Combine(AppContext.BaseDirectory, "App_Data", "logs");
Directory.CreateDirectory(logsDir);
var startupLog = Path.Combine(logsDir, $"startup-{DateTime.UtcNow:yyyy-MM-dd}.log");

void Bootlog(string message)
{
    var line = $"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}";
    Console.Error.WriteLine(line);
    try { File.AppendAllText(startupLog, line + Environment.NewLine); } catch { /* fs read-only? sigue */ }
}

try
{
    Bootlog("== Bootstrap iniciado ==");

    var builder = WebApplication.CreateBuilder(args);

    // -------- Servicios --------

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        var msg = "ConnectionStrings:DefaultConnection está vacía. " +
                  "En SmartASP.NET se inyecta como Environment Variable " +
                  "'ConnectionStrings__DefaultConnection' (doble guion bajo).";
        Bootlog("FATAL: " + msg);
        throw new InvalidOperationException(msg);
    }

    Bootlog($"Connection string leída (longitud={connectionString.Length}, empieza con '{connectionString[..Math.Min(20, connectionString.Length)]}...').");

    builder.Services.AddJurisControlData(connectionString);

    builder.Services.AddScoped<
        IUserClaimsPrincipalFactory<ApplicationUser>,
        DespachoUserClaimsPrincipalFactory>();

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
        options.Conventions.AuthorizeFolder("/");
        options.Conventions.AllowAnonymousToFolder("/Account");
    });

    Bootlog("Services registered. Building app...");

    var app = builder.Build();

    Bootlog("App built. Running migrations + RLS + seed...");

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var db = services.GetRequiredService<JurisControlDbContext>();
            db.SetPlatformScope();
            await db.Database.MigrateAsync();
            Bootlog("Migrations applied.");
            await RowLevelSecurityInstaller.ApplyAsync(db);
            Bootlog("RLS policies applied.");
            await DbSeeder.SeedAsync(app.Services);
            Bootlog("Seed complete.");
        }
        catch (Exception ex)
        {
            Bootlog("FATAL en init de BD: " + ex);
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

    Bootlog("== Pipeline configurado. Corriendo app.Run() ==");
    app.Run();
}
catch (Exception ex)
{
    Bootlog("== BOOTSTRAP FAILED ==");
    Bootlog(ex.ToString());
    throw;
}
