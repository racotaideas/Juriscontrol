using JurisControl.Data;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Asuntos;

[Authorize]
public class IndexModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public IndexModel(JurisControlDbContext db) => _db = db;

    public record Row(Guid Id, string Folio, string Titulo, string Materia, EstadoAsunto Estado,
                     string Cliente, string? Responsable, DateTimeOffset Fecha, int Prioridad);

    public List<Row> Items { get; private set; } = new();
    public string? Q { get; set; }
    public EstadoAsunto? Estado { get; set; }
    public string? Materia { get; set; }
    public int Total { get; private set; }

    public async Task OnGetAsync(string? q, EstadoAsunto? estado, string? materia)
    {
        Q = q?.Trim();
        Estado = estado;
        Materia = materia;

        var query = from a in _db.Asuntos.AsNoTracking()
                    join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id
                    join r in _db.Users.AsNoTracking() on a.ResponsableId equals r.Id into rr
                    from r in rr.DefaultIfEmpty()
                    select new
                    {
                        a.Id, a.Folio, a.Titulo, a.MateriaKey, a.Estado, a.FechaRecepcion, a.Prioridad,
                        Cliente = c.RazonSocial ?? c.NombreComercial ??
                                  ((c.Nombre ?? "") + " " + (c.ApellidoPaterno ?? "")).Trim(),
                        Responsable = r != null ? r.NombreCompleto : null
                    };

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var like = $"%{Q}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Folio, like) ||
                EF.Functions.Like(x.Titulo, like) ||
                EF.Functions.Like(x.Cliente, like));
        }
        if (Estado.HasValue) query = query.Where(x => x.Estado == Estado.Value);
        if (!string.IsNullOrWhiteSpace(Materia)) query = query.Where(x => x.MateriaKey == Materia);

        Total = await query.CountAsync();
        var rows = await query.OrderBy(x => x.Prioridad).ThenByDescending(x => x.FechaRecepcion).Take(300).ToListAsync();
        Items = rows.Select(x => new Row(x.Id, x.Folio, x.Titulo,
            JurisControl.Domain.Enums.Materia.Label(x.MateriaKey), x.Estado,
            x.Cliente, x.Responsable, x.FechaRecepcion, x.Prioridad)).ToList();
    }
}
