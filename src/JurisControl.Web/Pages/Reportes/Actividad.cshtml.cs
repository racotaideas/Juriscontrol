using JurisControl.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Reportes;

[Authorize]
public class ActividadModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public ActividadModel(JurisControlDbContext db) => _db = db;

    public DateOnly Desde { get; private set; }
    public DateOnly Hasta { get; private set; }
    public record Row(string Expediente, string Tipo, int Actuaciones, int Promociones);
    public List<Row> Rows { get; private set; } = new();
    public int TotalActuaciones { get; private set; }
    public int TotalPromociones { get; private set; }

    public async Task OnGetAsync(DateOnly? desde, DateOnly? hasta)
    {
        Desde = desde ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        Hasta = hasta ?? DateOnly.FromDateTime(DateTime.Now);

        var acts = await _db.Actuaciones.AsNoTracking()
            .Where(a => a.Fecha >= Desde && a.Fecha <= Hasta)
            .GroupBy(a => a.JuicioId)
            .Select(g => new { JuicioId = g.Key, Count = g.Count() })
            .ToListAsync();
        var proms = await _db.Promociones.AsNoTracking()
            .Where(p => p.FechaPresentacion >= Desde && p.FechaPresentacion <= Hasta)
            .GroupBy(p => p.JuicioId)
            .Select(g => new { JuicioId = g.Key, Count = g.Count() })
            .ToListAsync();

        var juicioIds = acts.Select(a => a.JuicioId).Union(proms.Select(p => p.JuicioId)).ToHashSet();
        var juicios = await _db.Juicios.AsNoTracking()
            .Where(j => juicioIds.Contains(j.Id))
            .Select(j => new { j.Id, j.NumeroExpediente, j.TipoJuicio })
            .ToListAsync();

        Rows = juicios.Select(j => new Row(
            j.NumeroExpediente,
            j.TipoJuicio,
            acts.FirstOrDefault(a => a.JuicioId == j.Id)?.Count ?? 0,
            proms.FirstOrDefault(p => p.JuicioId == j.Id)?.Count ?? 0))
            .OrderByDescending(r => r.Actuaciones + r.Promociones)
            .ToList();
        TotalActuaciones = Rows.Sum(r => r.Actuaciones);
        TotalPromociones = Rows.Sum(r => r.Promociones);
    }
}
