using JurisControl.Data;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Reportes;

[Authorize]
public class PlazosVencidosModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public PlazosVencidosModel(JurisControlDbContext db) => _db = db;

    public record Row(string Expediente, string Descripcion, DateOnly Vence,
                     int DiasVencido, string? Responsable, EstadoPlazo Estado);

    public List<Row> Vencidos { get; private set; } = new();
    public List<Row> ProximosVencer { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        var enDosSemanas = hoy.AddDays(14);

        var q = from p in _db.Plazos.AsNoTracking()
                join j in _db.Juicios.AsNoTracking() on p.JuicioId equals j.Id
                join u in _db.Users.AsNoTracking() on p.ResponsableId equals u.Id into uu
                from u in uu.DefaultIfEmpty()
                where p.Estado == EstadoPlazo.Abierto
                orderby p.FechaVencimiento
                select new
                {
                    j.NumeroExpediente, p.Descripcion, p.FechaVencimiento,
                    Responsable = u != null ? u.NombreCompleto : null,
                    p.Estado
                };

        var todos = await q.ToListAsync();
        Vencidos = todos
            .Where(x => x.FechaVencimiento < hoy)
            .Select(x => new Row(x.NumeroExpediente, x.Descripcion, x.FechaVencimiento,
                hoy.DayNumber - x.FechaVencimiento.DayNumber, x.Responsable, x.Estado))
            .ToList();
        ProximosVencer = todos
            .Where(x => x.FechaVencimiento >= hoy && x.FechaVencimiento <= enDosSemanas)
            .Select(x => new Row(x.NumeroExpediente, x.Descripcion, x.FechaVencimiento,
                hoy.DayNumber - x.FechaVencimiento.DayNumber, x.Responsable, x.Estado))
            .ToList();
    }
}
