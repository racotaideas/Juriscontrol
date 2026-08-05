using JurisControl.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Reportes;

[Authorize]
public class RecuperacionModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public RecuperacionModel(JurisControlDbContext db) => _db = db;

    public record Row(int Anio, int Mes, decimal Total, int Pagos);
    public List<Row> Rows { get; private set; } = new();
    public decimal TotalGlobal { get; private set; }

    public async Task OnGetAsync()
    {
        var pagos = await _db.PagosCobranza.AsNoTracking()
            .Select(p => new { p.Fecha, p.Monto }).ToListAsync();

        Rows = pagos
            .GroupBy(p => new { p.Fecha.Year, p.Fecha.Month })
            .Select(g => new Row(g.Key.Year, g.Key.Month, g.Sum(x => x.Monto), g.Count()))
            .OrderByDescending(r => r.Anio).ThenByDescending(r => r.Mes)
            .Take(24)
            .ToList();

        TotalGlobal = pagos.Sum(p => p.Monto);
    }

    public string MesLabel(int mes) => new[]{"", "Ene","Feb","Mar","Abr","May","Jun","Jul","Ago","Sep","Oct","Nov","Dic"}[mes];
}
