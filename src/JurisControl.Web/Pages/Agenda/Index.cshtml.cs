using JurisControl.Data;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Agenda;

[Authorize]
public class IndexModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public IndexModel(JurisControlDbContext db) => _db = db;

    public DateOnly Desde { get; private set; }
    public DateOnly Hasta { get; private set; }
    public string Rango { get; private set; } = "semana";

    public record ItemAudiencia(Guid Id, Guid JuicioId, string Expediente, string Tipo, DateTime FechaHora,
                                string? Lugar, EstadoAudiencia Estado, string? Asignado);
    public record ItemPlazo(Guid Id, Guid JuicioId, string Expediente, string Descripcion,
                            DateOnly Vencimiento, EstadoPlazo Estado, string? Responsable, int DiasRestantes);

    public List<ItemAudiencia> Audiencias { get; private set; } = new();
    public List<ItemPlazo> Plazos { get; private set; } = new();
    public int PlazosVencidos { get; private set; }

    public async Task OnGetAsync(string? rango, DateOnly? desde)
    {
        Rango = rango ?? "semana";
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        Desde = desde ?? hoy;
        Hasta = Rango switch
        {
            "dia" => Desde,
            "mes" => Desde.AddDays(30),
            _ => Desde.AddDays(7)
        };

        var dtDesde = Desde.ToDateTime(TimeOnly.MinValue);
        var dtHasta = Hasta.ToDateTime(new TimeOnly(23, 59, 59));

        Audiencias = await (from a in _db.Audiencias.AsNoTracking()
                            join j in _db.Juicios.AsNoTracking() on a.JuicioId equals j.Id
                            join u in _db.Users.AsNoTracking() on a.AsignadoAId equals u.Id into uu
                            from u in uu.DefaultIfEmpty()
                            where a.FechaHora >= dtDesde && a.FechaHora <= dtHasta
                                  && a.Estado != EstadoAudiencia.Cancelada
                            orderby a.FechaHora
                            select new ItemAudiencia(a.Id, j.Id, j.NumeroExpediente, a.Tipo,
                                a.FechaHora, a.Lugar, a.Estado,
                                u != null ? u.NombreCompleto : null))
                       .ToListAsync();

        Plazos = await (from p in _db.Plazos.AsNoTracking()
                        join j in _db.Juicios.AsNoTracking() on p.JuicioId equals j.Id
                        join u in _db.Users.AsNoTracking() on p.ResponsableId equals u.Id into uu
                        from u in uu.DefaultIfEmpty()
                        where p.FechaVencimiento >= Desde && p.FechaVencimiento <= Hasta
                              && p.Estado == EstadoPlazo.Abierto
                        orderby p.FechaVencimiento
                        select new ItemPlazo(p.Id, j.Id, j.NumeroExpediente, p.Descripcion,
                            p.FechaVencimiento, p.Estado,
                            u != null ? u.NombreCompleto : null,
                            p.FechaVencimiento.DayNumber - hoy.DayNumber))
                    .ToListAsync();

        PlazosVencidos = await _db.Plazos.CountAsync(p =>
            p.Estado == EstadoPlazo.Abierto && p.FechaVencimiento < hoy);
    }
}
