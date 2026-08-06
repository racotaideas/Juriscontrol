using JurisControl.Data;
using JurisControl.Data.TenantContext;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly JurisControlDbContext _db;
    private readonly ITenantContext _tenant;

    public IndexModel(JurisControlDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public string RazonSocial { get; private set; } = "—";
    public string NombreUsuario { get; private set; } = "—";
    public string MateriasAtiende { get; private set; } = "—";
    public bool ModoCobranza { get; private set; }
    public Guid DespachoId => _tenant.DespachoId ?? Guid.Empty;

    public int ClientesActivos { get; private set; }
    public int AsuntosActivos { get; private set; }
    public int JuiciosEnTramite { get; private set; }
    public int AudienciasSemana { get; private set; }
    public int PlazosPorVencer { get; private set; }
    public int PlazosVencidos { get; private set; }
    public int AsuntosCerradosMes { get; private set; }

    public record AsuntoBrief(Guid Id, string Folio, string Titulo, string Cliente, EstadoAsunto Estado, DateTimeOffset Fecha);
    public List<AsuntoBrief> UltimosAsuntos { get; private set; } = new();

    public async Task OnGetAsync()
    {
        NombreUsuario = User.Identity?.Name ?? "usuario";

        // Despachos NO implementa ITenantEntity, así que no lo filtra el Global
        // Query Filter. Con multi-tenant activo (3 despachos) hay que filtrar
        // explícitamente por el DespachoId del usuario.
        var miDespachoId = _tenant.DespachoId;
        var despacho = miDespachoId.HasValue
            ? await _db.Despachos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == miDespachoId.Value)
            : null;
        if (despacho is not null)
        {
            RazonSocial = despacho.RazonSocial;
            MateriasAtiende = despacho.MateriasAtiende.Replace(",", ", ");
            ModoCobranza = despacho.ModoCobranza;
        }

        var hoy = DateOnly.FromDateTime(DateTime.Now);
        var enUnaSemana = hoy.AddDays(7);
        var dtInicioSemana = DateTime.Now.Date;
        var dtFinSemana = DateTime.Now.Date.AddDays(7);

        ClientesActivos = await _db.Clientes.CountAsync(c => c.Activo);
        AsuntosActivos = await _db.Asuntos.CountAsync(a =>
            a.Estado == EstadoAsunto.Activo || a.Estado == EstadoAsunto.Asignado);
        JuiciosEnTramite = await _db.Juicios.CountAsync(j =>
            j.Estado != EstadoJuicio.Concluido && j.Estado != EstadoJuicio.Sobreseido);
        AudienciasSemana = await _db.Audiencias.CountAsync(a =>
            a.FechaHora >= dtInicioSemana && a.FechaHora <= dtFinSemana
            && a.Estado != EstadoAudiencia.Cancelada);
        PlazosPorVencer = await _db.Plazos.CountAsync(p =>
            p.Estado == EstadoPlazo.Abierto && p.FechaVencimiento >= hoy && p.FechaVencimiento <= enUnaSemana);
        PlazosVencidos = await _db.Plazos.CountAsync(p =>
            p.Estado == EstadoPlazo.Abierto && p.FechaVencimiento < hoy);
        var desdeInicioMes = new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
            TimeSpan.Zero);
        AsuntosCerradosMes = await _db.Asuntos.CountAsync(a =>
            a.Estado == EstadoAsunto.Cerrado && a.FechaCierre >= desdeInicioMes);

        var recientes = await (from a in _db.Asuntos.AsNoTracking()
                               join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id
                               orderby a.CreatedAt descending
                               select new
                               {
                                   a.Id, a.Folio, a.Titulo, a.Estado, a.CreatedAt,
                                   Cliente = c.RazonSocial ?? c.NombreComercial ??
                                             ((c.Nombre ?? "") + " " + (c.ApellidoPaterno ?? "")).Trim()
                               }).Take(8).ToListAsync();

        UltimosAsuntos = recientes.Select(x =>
            new AsuntoBrief(x.Id, x.Folio, x.Titulo, x.Cliente, x.Estado, x.CreatedAt)).ToList();
    }
}
