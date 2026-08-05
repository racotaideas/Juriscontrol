using JurisControl.Data;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Cobranza;

[Authorize]
public class IndexModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public IndexModel(JurisControlDbContext db) => _db = db;

    public record Row(Guid Id, string Numero, string Acreedor, string Deudor,
                     TipoCredito Tipo, EstadoCredito Estado, decimal Saldo, int? Mora);

    public List<Row> Items { get; private set; } = new();
    public string? Q { get; set; }
    public EstadoCredito? Estado { get; set; }

    public decimal SaldoTotal { get; private set; }
    public decimal SaldoJudicial { get; private set; }
    public int CreditosCartera { get; private set; }
    public int CreditosRecuperados { get; private set; }

    public async Task OnGetAsync(string? q, EstadoCredito? estado)
    {
        Q = q?.Trim();
        Estado = estado;

        var query = from c in _db.Creditos.AsNoTracking()
                    join dc in _db.Clientes.AsNoTracking() on c.DeudorClienteId equals dc.Id into dj
                    from dc in dj.DefaultIfEmpty()
                    select new
                    {
                        c.Id, c.NumeroCredito, c.Acreedor, c.Tipo, c.Estado, c.SaldoActual, c.DiasMora,
                        Deudor = dc != null
                            ? (dc.RazonSocial ?? dc.NombreComercial ??
                               ((dc.Nombre ?? "") + " " + (dc.ApellidoPaterno ?? "")).Trim())
                            : (c.NombreDeudor ?? "—")
                    };

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var like = $"%{Q}%";
            query = query.Where(x =>
                EF.Functions.Like(x.NumeroCredito, like) ||
                EF.Functions.Like(x.Acreedor, like) ||
                EF.Functions.Like(x.Deudor, like));
        }
        if (Estado.HasValue) query = query.Where(x => x.Estado == Estado.Value);

        var rows = await query.OrderByDescending(x => x.SaldoActual).Take(500).ToListAsync();
        Items = rows.Select(x => new Row(x.Id, x.NumeroCredito, x.Acreedor, x.Deudor,
            x.Tipo, x.Estado, x.SaldoActual, x.DiasMora)).ToList();

        SaldoTotal = await _db.Creditos.Where(c =>
            c.Estado != EstadoCredito.Recuperado && c.Estado != EstadoCredito.Incobrable)
            .SumAsync(c => (decimal?)c.SaldoActual) ?? 0m;
        SaldoJudicial = await _db.Creditos.Where(c => c.Estado == EstadoCredito.Judicial)
            .SumAsync(c => (decimal?)c.SaldoActual) ?? 0m;
        CreditosCartera = await _db.Creditos.CountAsync(c => c.Estado == EstadoCredito.Cartera);
        CreditosRecuperados = await _db.Creditos.CountAsync(c => c.Estado == EstadoCredito.Recuperado);
    }
}
