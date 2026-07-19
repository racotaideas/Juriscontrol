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
    private const string PilotoAdminInitialPassword = "Cambiar1!Yaesto"; // el admin la rota en el primer login

    public static async Task SeedAsync(IServiceProvider services)
    {
        // Scope propio con ITenantContext en modo plataforma para saltarnos los query filters
        // y las validaciones de "no insertar sin tenant". El seed es la única operación
        // legítima que puede correr fuera de un tenant.
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        // Sustituye el HttpTenantContext scoped por uno de plataforma solo dentro de este scope.
        // Como el ServiceProvider del scope ya construyó el DbContext, tenemos que reasignar el
        // campo _tenant manualmente vía reflexión — o mejor, exponerlo con setter. Vamos por el
        // enfoque limpio: el DbContext recibe el ITenantContext por constructor y ya está
        // resuelto del scope. Aquí lo que hacemos es garantizar que el TenantContext registrado
        // sea el de plataforma antes de resolver el DbContext.
        var db = sp.GetRequiredService<JurisControlDbContext>();
        db.SetPlatformScope();

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
    }
}
