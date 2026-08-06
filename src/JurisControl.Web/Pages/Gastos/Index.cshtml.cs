using JurisControl.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Gastos;

[Authorize]
public class IndexModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public IndexModel(JurisControlDbContext db) => _db = db;

    public record Row(Guid Id, DateOnly Fecha, string Categoria, string Concepto,
                     decimal Monto, bool Reembolsable, string Estado,
                     string? Expediente, string? Folio);

    public List<Row> Items { get; private set; } = new();
    public decimal Total { get; private set; }
    public decimal TotalReembolsable { get; private set; }
    public decimal TotalPendiente { get; private set; }
    public string? Q { get; set; }

    public async Task OnGetAsync(string? q)
    {
        Q = q?.Trim();
        var query = from g in _db.Gastos.AsNoTracking()
                    join j in _db.Juicios.AsNoTracking() on g.JuicioId equals j.Id into jj
                    from j in jj.DefaultIfEmpty()
                    join a in _db.Asuntos.AsNoTracking() on g.AsuntoId equals a.Id into aa
                    from a in aa.DefaultIfEmpty()
                    select new
                    {
                        g.Id, g.Fecha, g.Categoria, g.Concepto, g.Monto, g.Reembolsable, g.Estado,
                        Expediente = j != null ? j.NumeroExpediente : null,
                        Folio = a != null ? a.Folio : null
                    };

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var like = $"%{Q}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Concepto, like) ||
                EF.Functions.Like(x.Categoria, like) ||
                EF.Functions.Like(x.Expediente ?? "", like));
        }

        var rows = await query.OrderByDescending(x => x.Fecha).Take(500).ToListAsync();
        Items = rows.Select(x => new Row(x.Id, x.Fecha, x.Categoria, x.Concepto, x.Monto,
            x.Reembolsable, x.Estado, x.Expediente, x.Folio)).ToList();

        Total = Items.Sum(i => i.Monto);
        TotalReembolsable = Items.Where(i => i.Reembolsable).Sum(i => i.Monto);
        TotalPendiente = Items.Where(i => i.Estado == "pendiente").Sum(i => i.Monto);
    }
}
