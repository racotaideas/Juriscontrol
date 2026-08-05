using JurisControl.Data;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Juicios;

[Authorize]
public class IndexModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public IndexModel(JurisControlDbContext db) => _db = db;

    public record Row(Guid Id, string Expediente, string Juzgado, string Tipo, string Materia,
                     EstadoJuicio Estado, string AsuntoFolio, string Cliente, DateOnly Inicio);

    public List<Row> Items { get; private set; } = new();
    public string? Q { get; set; }
    public EstadoJuicio? Estado { get; set; }

    public async Task OnGetAsync(string? q, EstadoJuicio? estado)
    {
        Q = q?.Trim();
        Estado = estado;

        var query = from j in _db.Juicios.AsNoTracking()
                    join a in _db.Asuntos.AsNoTracking() on j.AsuntoId equals a.Id
                    join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id
                    select new
                    {
                        j.Id, j.NumeroExpediente, j.Juzgado, j.TipoJuicio, j.MateriaKey, j.Estado, j.FechaInicio,
                        AsuntoFolio = a.Folio,
                        Cliente = c.RazonSocial ?? c.NombreComercial ??
                                  ((c.Nombre ?? "") + " " + (c.ApellidoPaterno ?? "")).Trim()
                    };

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var like = $"%{Q}%";
            query = query.Where(x =>
                EF.Functions.Like(x.NumeroExpediente, like) ||
                EF.Functions.Like(x.Juzgado, like) ||
                EF.Functions.Like(x.TipoJuicio, like) ||
                EF.Functions.Like(x.Cliente, like) ||
                EF.Functions.Like(x.AsuntoFolio, like));
        }
        if (Estado.HasValue) query = query.Where(x => x.Estado == Estado.Value);

        var rows = await query.OrderByDescending(x => x.FechaInicio).Take(300).ToListAsync();
        Items = rows.Select(x => new Row(x.Id, x.NumeroExpediente, x.Juzgado, x.TipoJuicio,
            JurisControl.Domain.Enums.Materia.Label(x.MateriaKey), x.Estado, x.AsuntoFolio, x.Cliente, x.FechaInicio)).ToList();
    }
}
