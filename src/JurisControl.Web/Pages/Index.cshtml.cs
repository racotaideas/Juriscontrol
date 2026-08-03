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
    public int AsuntosRecibidos { get; private set; }
    public int AsuntosCerradosMes { get; private set; }

    public record AsuntoBrief(Guid Id, string Folio, string Titulo, string Cliente, EstadoAsunto Estado, DateTimeOffset Fecha);
    public List<AsuntoBrief> UltimosAsuntos { get; private set; } = new();

    public async Task OnGetAsync()
    {
        NombreUsuario = User.Identity?.Name ?? "usuario";

        var despacho = await _db.Despachos.AsNoTracking().SingleOrDefaultAsync();
        if (despacho is not null)
        {
            RazonSocial = despacho.RazonSocial;
            MateriasAtiende = despacho.MateriasAtiende.Replace(",", ", ");
            ModoCobranza = despacho.ModoCobranza;
        }

        ClientesActivos = await _db.Clientes.CountAsync(c => c.Activo);
        AsuntosActivos = await _db.Asuntos.CountAsync(a =>
            a.Estado == EstadoAsunto.Activo || a.Estado == EstadoAsunto.Asignado);
        AsuntosRecibidos = await _db.Asuntos.CountAsync(a => a.Estado == EstadoAsunto.Recibido);
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
