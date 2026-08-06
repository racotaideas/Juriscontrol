using JurisControl.Data.TenantContext;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JurisControl.Data.Seeding;

/// <summary>
/// Crea los despachos piloto y sus usuarios administradores.
/// - Despacho 1 (piloto principal): Rafael Corona · admin rafael.corona.tavarez
/// - Despacho 2 (multi-tenant demo): Bufete Álvarez y Asociados · admin gabriela.alvarez
/// - Despacho 3 (multi-tenant demo): Corporativo Jurídico Reforma · admin fernando.reforma
/// Todos con password inicial <c>Piloto1234</c>.
/// Además crea 2-3 abogados adicionales por despacho.
/// </summary>
public static class DbSeeder
{
    public sealed record DespachoDemo(
        Guid Id, string RazonSocial, string Materias, bool ModoCobranza,
        string AdminEmail, string AdminNombre, string[] AbogadosEmails, string[] AbogadosNombres);

    public static readonly DespachoDemo[] Despachos =
    {
        new DespachoDemo(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "Rafael Corona · Abogados",
            "civil,mercantil,familiar,laboral,cobranza,penal,amparo,administrativo",
            ModoCobranza: true,
            AdminEmail: "rafael.corona.tavarez@gmail.com",
            AdminNombre: "Rafael Corona Tavárez",
            AbogadosEmails: new[]
            {
                "carlos.mendoza@rafacorona.mx",
                "monica.perez@rafacorona.mx",
                "luis.ramirez@rafacorona.mx"
            },
            AbogadosNombres: new[]
            {
                "Lic. Carlos Mendoza Hernández",
                "Lic. Mónica Pérez Sánchez",
                "Lic. Luis Ramírez Torres"
            }
        ),
        new DespachoDemo(
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            "Bufete Álvarez y Asociados S.C.",
            "civil,mercantil,corporativo,amparo,fiscal",
            ModoCobranza: false,
            AdminEmail: "gabriela.alvarez@bufete-alvarez.mx",
            AdminNombre: "Lic. Gabriela Álvarez Martínez",
            AbogadosEmails: new[]
            {
                "roberto.solis@bufete-alvarez.mx",
                "andrea.vega@bufete-alvarez.mx"
            },
            AbogadosNombres: new[]
            {
                "Lic. Roberto Solís Guzmán",
                "Lic. Andrea Vega López"
            }
        ),
        new DespachoDemo(
            Guid.Parse("00000000-0000-0000-0000-000000000003"),
            "Corporativo Jurídico Reforma S.C.",
            "mercantil,cobranza,corporativo,laboral,fiscal",
            ModoCobranza: true,
            AdminEmail: "fernando.reforma@cjreforma.mx",
            AdminNombre: "Lic. Fernando Reforma Cárdenas",
            AbogadosEmails: new[]
            {
                "patricia.ochoa@cjreforma.mx",
                "javier.montes@cjreforma.mx"
            },
            AbogadosNombres: new[]
            {
                "Lic. Patricia Ochoa Rivera",
                "Lic. Javier Montes Aguilar"
            }
        )
    };

    private const string PasswordEstandar = "Piloto1234";

    public static async Task SeedAsync(IServiceProvider services)
    {
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

        foreach (var d in Despachos)
        {
            await SembrarDespachoAsync(db, userManager, logger, d);
        }
    }

    private static async Task SembrarDespachoAsync(
        JurisControlDbContext db, UserManager<ApplicationUser> userManager,
        ILogger logger, DespachoDemo d)
    {
        var despacho = await db.Despachos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == d.Id);

        if (despacho is null)
        {
            despacho = new Despacho
            {
                Id = d.Id,
                RazonSocial = d.RazonSocial,
                MateriasAtiende = d.Materias,
                ModoCobranza = d.ModoCobranza,
                Estado_ = EstadoDespacho.Activo,
                FechaAlta = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "seed"
            };
            db.Despachos.Add(despacho);
            await db.SaveChangesAsync();
            logger.LogInformation("Despacho '{Razon}' sembrado.", d.RazonSocial);
        }

        // Admin
        await AsegurarUsuarioAsync(userManager, logger, d.Id, d.AdminEmail, d.AdminNombre, RolUsuario.FirmAdmin);

        // Abogados asociados
        for (int i = 0; i < d.AbogadosEmails.Length; i++)
        {
            await AsegurarUsuarioAsync(userManager, logger, d.Id,
                d.AbogadosEmails[i], d.AbogadosNombres[i], RolUsuario.Lawyer);
        }
    }

    private static async Task AsegurarUsuarioAsync(
        UserManager<ApplicationUser> userManager, ILogger logger,
        Guid despachoId, string email, string nombreCompleto, string rol)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NombreCompleto = nombreCompleto,
                DespachoId = despachoId,
                Activo = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var result = await userManager.CreateAsync(user, PasswordEstandar);
            if (!result.Succeeded)
            {
                var errors = string.Join(" · ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"No se pudo crear usuario '{email}': {errors}");
            }
            await userManager.AddToRoleAsync(user, rol);
            logger.LogInformation("Usuario {Email} creado con rol {Rol} en despacho {Despacho}.",
                email, rol, despachoId);
        }
        else if (!await userManager.CheckPasswordAsync(user, PasswordEstandar))
        {
            // Reset one-shot al password estándar del piloto
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await userManager.ResetPasswordAsync(user, token, PasswordEstandar);
            if (reset.Succeeded)
                logger.LogWarning("Password del usuario {Email} re-alineado a '{Password}'.", email, PasswordEstandar);
        }
    }
}
