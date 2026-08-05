using JurisControl.Data;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Reportes;

[Authorize]
public class AsuntosPorEstadoModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public AsuntosPorEstadoModel(JurisControlDbContext db) => _db = db;

    public Dictionary<string, Dictionary<EstadoAsunto, int>> Matriz { get; private set; } = new();
    public EstadoAsunto[] Estados => Enum.GetValues<EstadoAsunto>();

    public async Task OnGetAsync()
    {
        var data = await _db.Asuntos.AsNoTracking()
            .GroupBy(a => new { a.MateriaKey, a.Estado })
            .Select(g => new { g.Key.MateriaKey, g.Key.Estado, Count = g.Count() })
            .ToListAsync();

        foreach (var m in Materia.All)
        {
            Matriz[m] = Enum.GetValues<EstadoAsunto>().ToDictionary(e => e, e => 0);
        }
        foreach (var d in data)
        {
            if (!Matriz.ContainsKey(d.MateriaKey))
                Matriz[d.MateriaKey] = Enum.GetValues<EstadoAsunto>().ToDictionary(e => e, e => 0);
            Matriz[d.MateriaKey][d.Estado] = d.Count;
        }
    }
}
