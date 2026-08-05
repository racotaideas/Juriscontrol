using JurisControl.Data;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Reportes;

[Authorize]
public class CobranzaCarteraModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public CobranzaCarteraModel(JurisControlDbContext db) => _db = db;

    public record Row(string Acreedor, int Creditos, decimal SaldoTotal, decimal MontoOriginal);
    public List<Row> Rows { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Rows = await _db.Creditos.AsNoTracking()
            .GroupBy(c => c.Acreedor)
            .Select(g => new Row(
                g.Key,
                g.Count(),
                g.Sum(c => c.SaldoActual),
                g.Sum(c => c.MontoOriginal)))
            .OrderByDescending(r => r.SaldoTotal)
            .ToListAsync();
    }
}
