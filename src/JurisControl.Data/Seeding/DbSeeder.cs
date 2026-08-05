using JurisControl.Data.TenantContext;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JurisControl.Data.Seeding;

/// <summary>
/// Crea el despacho piloto y su usuario administrador si aún no existen.
/// Corre en modo plataforma (bypass del query filter) por medio de <see cref="BackgroundTenantContext"/>.
/// </summary>
public static class DbSeeder
{
    // Datos del despacho piloto (rafacorona-001).
    private static readonly Guid PilotoDespachoId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string PilotoRazonSocial = "Rafael Corona";
    private const string PilotoAdminEmail = "rafael.corona.tavarez@gmail.com";
    private const string PilotoAdminInitialPassword = "Piloto1234"; // fácil para piloto; rotar antes de comercializar

    public static async Task SeedAsync(IServiceProvider services)
    {
        // Scope propio: el seed es la única operación legítima que corre fuera de un tenant.
        // Con EnterPlatformScope() el mismo ITenantContext (scoped) queda en modo plataforma
        // durante todo el seed. Eso lo ven a la vez:
        //   - el Global Query Filter de EF Core (deja pasar todos los registros)
        //   - el TenantSessionInterceptor (manda PlatformScope=1 al SESSION_CONTEXT)
        // Sin esta segunda parte, la RLS de SQL Server (fn_TenantAccessPredicate) devuelve
        // block predicate y rechaza los INSERT del seed con error 33504.
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        var tenant = sp.GetRequiredService<ITenantContext>();
        using var _platform = tenant.EnterPlatformScope();

        var db = sp.GetRequiredService<JurisControlDbContext>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        // Roles del sistema
        foreach (var role in RolUsuario.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role, Id = Guid.NewGuid() });
                logger.LogInformation("Rol {Role} creado.", role);
            }
        }

        // Despacho piloto (con IgnoreQueryFilters porque aún no hay tenant en el scope)
        var despacho = await db.Despachos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == PilotoDespachoId);

        if (despacho is null)
        {
            despacho = new Despacho
            {
                Id = PilotoDespachoId,
                RazonSocial = PilotoRazonSocial,
                MateriasAtiende = "civil,mercantil,familiar,laboral,cobranza,penal,amparo,administrativo",
                ModoCobranza = true,
                Estado_ = EstadoDespacho.Activo,
                FechaAlta = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "seed"
            };
            db.Despachos.Add(despacho);
            await db.SaveChangesAsync();
            logger.LogInformation("Despacho piloto '{Razon}' sembrado con Id={Id}.", despacho.RazonSocial, despacho.Id);
        }

        // Usuario admin del despacho piloto
        var admin = await userManager.FindByEmailAsync(PilotoAdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = PilotoAdminEmail,
                Email = PilotoAdminEmail,
                EmailConfirmed = true,
                NombreCompleto = "Rafael Corona (admin)",
                DespachoId = despacho.Id,
                Activo = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var result = await userManager.CreateAsync(admin, PilotoAdminInitialPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(" · ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"No se pudo crear el admin piloto: {errors}");
            }

            await userManager.AddToRoleAsync(admin, RolUsuario.FirmAdmin);
            logger.LogInformation("Admin piloto {Email} creado con rol {Rol}.", PilotoAdminEmail, RolUsuario.FirmAdmin);
            logger.LogWarning(
                "Password inicial del admin: '{Password}'. Debe rotarse en el primer acceso.",
                PilotoAdminInitialPassword);
        }
        else if (!await userManager.CheckPasswordAsync(admin, PilotoAdminInitialPassword))
        {
            // Reset one-shot durante el piloto: si el password actual no coincide
            // con el de referencia, se re-alinea. Quitar este bloque cuando exista
            // la UI de "Cambiar contraseña" y el admin gestione su propia rotación.
            var token = await userManager.GeneratePasswordResetTokenAsync(admin);
            var reset = await userManager.ResetPasswordAsync(admin, token, PilotoAdminInitialPassword);
            if (reset.Succeeded)
                logger.LogWarning("Password del admin piloto re-alineado a '{Password}'.", PilotoAdminInitialPassword);
            else
                logger.LogError("No se pudo re-alinear password del admin: {Errors}",
                    string.Join(" · ", reset.Errors.Select(e => e.Description)));
        }
    }
}
