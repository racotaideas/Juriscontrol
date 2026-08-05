using JurisControl.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Reportes;

[Authorize]
public class ProductividadModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public ProductividadModel(JurisControlDbContext db) => _db = db;

    public DateOnly Desde { get; private set; }
    public DateOnly Hasta { get; private set; }
    public record Row(string Abogado, int Asuntos, int Promociones, int AudienciasAsignadas, int PlazosCumplidos);
    public List<Row> Rows { get; private set; } = new();

    public async Task OnGetAsync(DateOnly? desde, DateOnly? hasta)
    {
        Desde = desde ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        Hasta = hasta ?? DateOnly.FromDateTime(DateTime.Now);
        var dtDesde = Desde.ToDateTime(TimeOnly.MinValue);
        var dtHasta = Hasta.ToDateTime(new TimeOnly(23, 59, 59));

        var usuarios = await _db.Users.AsNoTracking().Where(u => u.Activo)
            .Select(u => new { u.Id, u.NombreCompleto, u.Email }).ToListAsync();

        var rows = new List<Row>();
        foreach (var u in usuarios)
        {
            var asuntos = await _db.Asuntos.CountAsync(a => a.ResponsableId == u.Id);
            var proms = await _db.Promociones.CountAsync(p => p.FirmanteId == u.Id
                && p.FechaPresentacion >= Desde && p.FechaPresentacion <= Hasta);
            var auds = await _db.Audiencias.CountAsync(a => a.AsignadoAId == u.Id
                && a.FechaHora >= dtDesde && a.FechaHora <= dtHasta);
            var plazos = await _db.Plazos.CountAsync(p => p.ResponsableId == u.Id
                && p.FechaCumplimiento >= dtDesde && p.FechaCumplimiento <= dtHasta);
            if (asuntos == 0 && proms == 0 && auds == 0 && plazos == 0) continue;
            rows.Add(new Row(u.NombreCompleto ?? u.Email ?? "", asuntos, proms, auds, plazos));
        }
        Rows = rows.OrderByDescending(r => r.Promociones + r.AudienciasAsignadas + r.PlazosCumplidos).ToList();
    }
}
