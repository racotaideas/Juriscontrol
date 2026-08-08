using System.Security.Claims;
using JurisControl.Data;
using JurisControl.Data.TenantContext;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Diagnostico;

/// <summary>
/// Página de verificación multi-tenant. Muestra al usuario:
///   - Quién es (correo + claims)
///   - En qué despacho está (nombre + ID)
///   - Conteos de cada entidad, todos filtrados automáticamente por RLS +
///     Global Query Filter → si los datos se mezclaran, aquí saldría el ruido.
///   - Ejemplos de folios/expedientes visibles.
///
/// Al entrar con distintas cuentas (Rafael / Gabriela / Fernando) los números
/// deben ser distintos y sin traslape.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly JurisControlDbContext _db;
    private readonly ITenantContext _tenant;

    public IndexModel(JurisControlDbContext db, ITenantContext tenant)
    {
        _db = db; _tenant = tenant;
    }

    public string Email => User.Identity?.Name ?? "—";
    public string DespachoIdClaim => User.FindFirstValue(HttpTenantContext.DespachoIdClaimType) ?? "—";
    public Guid? DespachoIdEnContexto => _tenant.DespachoId;
    public bool EsPlatformScope => _tenant.IsPlatformScope;

    public Despacho? Despacho { get; private set; }

    public int Clientes { get; private set; }
    public int Asuntos { get; private set; }
    public int Juicios { get; private set; }
    public int Actuaciones { get; private set; }
    public int Promociones { get; private set; }
    public int Audiencias { get; private set; }
    public int Plazos { get; private set; }
    public int Documentos { get; private set; }
    public int Creditos { get; private set; }
    public int Plantillas { get; private set; }
    public int Gastos { get; private set; }
    public int Usuarios { get; private set; }

    public record FolioEjemplo(string Folio, string Titulo);
    public record ExpedienteEjemplo(string Expediente, string Juzgado);
    public List<FolioEjemplo> UltimosFolios { get; private set; } = new();
    public List<ExpedienteEjemplo> UltimosExpedientes { get; private set; } = new();

    public async Task OnGetAsync()
    {
        if (_tenant.DespachoId is Guid did)
            Despacho = await _db.Despachos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == did);

        // Todos estos count() los filtra automáticamente el Global Query Filter
        // por DespachoId del usuario. Aparte, la RLS de SQL Server bloquea
        // cualquier fuga que se escapase por debajo de EF.
        Clientes = await _db.Clientes.CountAsync();
        Asuntos = await _db.Asuntos.CountAsync();
        Juicios = await _db.Juicios.CountAsync();
        Actuaciones = await _db.Actuaciones.CountAsync();
        Promociones = await _db.Promociones.CountAsync();
        Audiencias = await _db.Audiencias.CountAsync();
        Plazos = await _db.Plazos.CountAsync();
        Documentos = await _db.Documentos.CountAsync();
        Creditos = await _db.Creditos.CountAsync();
        Plantillas = await _db.Plantillas.CountAsync();
        Gastos = await _db.Gastos.CountAsync();
        Usuarios = await _db.Users.CountAsync();

        UltimosFolios = await _db.Asuntos.AsNoTracking()
            .OrderByDescending(a => a.CreatedAt).Take(5)
            .Select(a => new FolioEjemplo(a.Folio, a.Titulo))
            .ToListAsync();

        UltimosExpedientes = await _db.Juicios.AsNoTracking()
            .OrderByDescending(j => j.CreatedAt).Take(5)
            .Select(j => new ExpedienteEjemplo(j.NumeroExpediente, j.Juzgado))
            .ToListAsync();
    }
}
