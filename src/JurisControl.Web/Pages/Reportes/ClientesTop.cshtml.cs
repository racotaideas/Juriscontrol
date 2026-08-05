using JurisControl.Data;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Reportes;

[Authorize]
public class ClientesTopModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public ClientesTopModel(JurisControlDbContext db) => _db = db;

    public record Row(string Nombre, int AsuntosActivos, int AsuntosTotal, decimal CuantiaTotal);
    public List<Row> Rows { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var rows = await (from a in _db.Asuntos.AsNoTracking()
                          join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id
                          group new { a, c } by new { c.Id, c.RazonSocial, c.NombreComercial,
                              c.Nombre, c.ApellidoPaterno } into g
                          select new
                          {
                              Nombre = g.Key.RazonSocial ?? g.Key.NombreComercial ??
                                       ((g.Key.Nombre ?? "") + " " + (g.Key.ApellidoPaterno ?? "")).Trim(),
                              AsuntosTotal = g.Count(),
                              AsuntosActivos = g.Count(x => x.a.Estado == EstadoAsunto.Activo
                                                          || x.a.Estado == EstadoAsunto.Asignado),
                              CuantiaTotal = g.Sum(x => x.a.Cuantia ?? 0m)
                          }).ToListAsync();

        Rows = rows.OrderByDescending(r => r.AsuntosActivos).ThenByDescending(r => r.CuantiaTotal)
            .Take(50)
            .Select(r => new Row(r.Nombre, r.AsuntosActivos, r.AsuntosTotal, r.CuantiaTotal))
            .ToList();
    }
}
