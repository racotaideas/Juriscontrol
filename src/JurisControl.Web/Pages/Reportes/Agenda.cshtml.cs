using JurisControl.Data;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Reportes;

[Authorize]
public class AgendaModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public AgendaModel(JurisControlDbContext db) => _db = db;

    public DateOnly Desde { get; private set; }
    public DateOnly Hasta { get; private set; }
    public record ItemAud(string Expediente, string Tipo, DateTime FechaHora, string? Lugar, string? Asignado, EstadoAudiencia Estado);
    public record ItemPlz(string Expediente, string Descripcion, DateOnly Vence, string? Responsable, EstadoPlazo Estado);

    public List<ItemAud> Audiencias { get; private set; } = new();
    public List<ItemPlz> Plazos { get; private set; } = new();

    public async Task OnGetAsync(DateOnly? desde, DateOnly? hasta)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        Desde = desde ?? hoy;
        Hasta = hasta ?? hoy.AddDays(30);
        var dtDesde = Desde.ToDateTime(TimeOnly.MinValue);
        var dtHasta = Hasta.ToDateTime(new TimeOnly(23, 59, 59));

        Audiencias = await (from a in _db.Audiencias.AsNoTracking()
                            join j in _db.Juicios.AsNoTracking() on a.JuicioId equals j.Id
                            join u in _db.Users.AsNoTracking() on a.AsignadoAId equals u.Id into uu
                            from u in uu.DefaultIfEmpty()
                            where a.FechaHora >= dtDesde && a.FechaHora <= dtHasta
                            orderby a.FechaHora
                            select new ItemAud(j.NumeroExpediente, a.Tipo, a.FechaHora, a.Lugar,
                                u != null ? u.NombreCompleto : null, a.Estado)).ToListAsync();

        Plazos = await (from p in _db.Plazos.AsNoTracking()
                        join j in _db.Juicios.AsNoTracking() on p.JuicioId equals j.Id
                        join u in _db.Users.AsNoTracking() on p.ResponsableId equals u.Id into uu
                        from u in uu.DefaultIfEmpty()
                        where p.FechaVencimiento >= Desde && p.FechaVencimiento <= Hasta
                        orderby p.FechaVencimiento
                        select new ItemPlz(j.NumeroExpediente, p.Descripcion, p.FechaVencimiento,
                            u != null ? u.NombreCompleto : null, p.Estado)).ToListAsync();
    }
}
