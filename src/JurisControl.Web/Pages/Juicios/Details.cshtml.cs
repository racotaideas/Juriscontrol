using System.ComponentModel.DataAnnotations;
using JurisControl.Data;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Juicios;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public DetailsModel(JurisControlDbContext db) => _db = db;

    public Juicio Juicio { get; private set; } = null!;
    public Asunto? Asunto { get; private set; }
    public Cliente? ClientePrincipal { get; private set; }
    public List<ParteJuicio> Partes { get; private set; } = new();
    public List<Actuacion> Actuaciones { get; private set; } = new();
    public List<Promocion> Promociones { get; private set; } = new();
    public List<Audiencia> Audiencias { get; private set; } = new();
    public List<Plazo> Plazos { get; private set; } = new();
    public List<SelectListItem> UsuariosList { get; private set; } = new();
    public List<SelectListItem> ClientesList { get; private set; } = new();

    [BindProperty] public NuevaActuacion Act { get; set; } = new();
    [BindProperty] public NuevaPromocion Prom { get; set; } = new();
    [BindProperty] public NuevaAudiencia Aud { get; set; } = new();
    [BindProperty] public NuevoPlazo Plz { get; set; } = new();
    [BindProperty] public NuevaParte Parte { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Id = id;
        if (!await LoadAsync(id)) return NotFound();
        return Page();
    }

    private async Task<bool> LoadAsync(Guid id)
    {
        var j = await _db.Juicios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (j is null) return false;
        Juicio = j;
        Asunto = await _db.Asuntos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == j.AsuntoId);
        if (Asunto is not null)
            ClientePrincipal = await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Asunto.ClienteId);

        Partes = await _db.PartesJuicio.AsNoTracking().Where(p => p.JuicioId == id).ToListAsync();
        Actuaciones = await _db.Actuaciones.AsNoTracking().Where(a => a.JuicioId == id)
            .OrderByDescending(a => a.Fecha).ToListAsync();
        Promociones = await _db.Promociones.AsNoTracking().Where(p => p.JuicioId == id)
            .OrderByDescending(p => p.FechaPresentacion).ToListAsync();
        Audiencias = await _db.Audiencias.AsNoTracking().Where(a => a.JuicioId == id)
            .OrderBy(a => a.FechaHora).ToListAsync();
        Plazos = await _db.Plazos.AsNoTracking().Where(p => p.JuicioId == id)
            .OrderBy(p => p.FechaVencimiento).ToListAsync();

        UsuariosList = await _db.Users.AsNoTracking().Where(u => u.Activo)
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.NombreCompleto ?? u.Email ?? "" })
            .ToListAsync();
        ClientesList = await _db.Clientes.AsNoTracking().Where(c => c.Activo)
            .OrderBy(c => c.RazonSocial ?? c.ApellidoPaterno ?? c.Nombre)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.RazonSocial ?? c.NombreComercial ??
                       ((c.Nombre ?? "") + " " + (c.ApellidoPaterno ?? "")).Trim()
            }).ToListAsync();
        return true;
    }

    // ---- Handlers de sub-formularios ----

    public async Task<IActionResult> OnPostActuacionAsync()
    {
        if (!await LoadAsync(Id)) return NotFound();
        if (string.IsNullOrWhiteSpace(Act.Resumen))
        {
            ModelState.AddModelError("Act.Resumen", "El resumen es obligatorio.");
            return Page();
        }
        _db.Actuaciones.Add(new Actuacion
        {
            JuicioId = Id,
            Tipo = Act.Tipo,
            Fecha = Act.Fecha,
            FechaNotificacion = Act.FechaNotificacion,
            Resumen = Act.Resumen,
            Detalle = Act.Detalle,
            CreatedBy = User.Identity?.Name
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Actuación registrada.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostPromocionAsync()
    {
        if (!await LoadAsync(Id)) return NotFound();
        if (string.IsNullOrWhiteSpace(Prom.Titulo))
        {
            ModelState.AddModelError("Prom.Titulo", "El título es obligatorio.");
            return Page();
        }
        _db.Promociones.Add(new Promocion
        {
            JuicioId = Id,
            Tipo = Prom.Tipo,
            FechaPresentacion = Prom.FechaPresentacion,
            Titulo = Prom.Titulo,
            Contenido = Prom.Contenido,
            FirmanteId = Prom.FirmanteId,
            NumeroAcuse = Prom.NumeroAcuse,
            CreatedBy = User.Identity?.Name
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Promoción registrada.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostAudienciaAsync()
    {
        if (!await LoadAsync(Id)) return NotFound();
        if (string.IsNullOrWhiteSpace(Aud.Tipo))
        {
            ModelState.AddModelError("Aud.Tipo", "El tipo es obligatorio.");
            return Page();
        }
        _db.Audiencias.Add(new Audiencia
        {
            JuicioId = Id,
            FechaHora = Aud.FechaHora,
            Tipo = Aud.Tipo,
            Lugar = Aud.Lugar,
            Estado = Aud.Estado,
            AsignadoAId = Aud.AsignadoAId,
            CreatedBy = User.Identity?.Name
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Audiencia programada.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostPlazoAsync()
    {
        if (!await LoadAsync(Id)) return NotFound();
        if (string.IsNullOrWhiteSpace(Plz.Descripcion))
        {
            ModelState.AddModelError("Plz.Descripcion", "La descripción es obligatoria.");
            return Page();
        }
        _db.Plazos.Add(new Plazo
        {
            JuicioId = Id,
            Descripcion = Plz.Descripcion,
            FechaInicio = Plz.FechaInicio,
            FechaVencimiento = Plz.FechaVencimiento,
            DiasOriginales = Plz.DiasOriginales,
            DiasHabiles = Plz.DiasHabiles,
            ResponsableId = Plz.ResponsableId,
            CreatedBy = User.Identity?.Name
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Plazo agregado.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostParteAsync()
    {
        if (!await LoadAsync(Id)) return NotFound();
        if (Parte.ClienteId is null && string.IsNullOrWhiteSpace(Parte.NombreLibre))
        {
            ModelState.AddModelError("Parte.NombreLibre", "Selecciona un cliente o captura un nombre libre.");
            return Page();
        }
        _db.PartesJuicio.Add(new ParteJuicio
        {
            JuicioId = Id,
            Rol = Parte.Rol,
            ClienteId = Parte.ClienteId,
            NombreLibre = Parte.NombreLibre,
            Representante = Parte.Representante,
            Notas = Parte.Notas,
            CreatedBy = User.Identity?.Name
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Parte agregada.";
        return RedirectToPage(new { id = Id });
    }

    // ---- DTOs ----
    public class NuevaActuacion
    {
        public TipoActuacion Tipo { get; set; } = TipoActuacion.Acuerdo;
        [Required] public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly? FechaNotificacion { get; set; }
        [Required] public string Resumen { get; set; } = string.Empty;
        public string? Detalle { get; set; }
    }

    public class NuevaPromocion
    {
        public TipoPromocion Tipo { get; set; } = TipoPromocion.Otro;
        [Required] public DateOnly FechaPresentacion { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [Required] public string Titulo { get; set; } = string.Empty;
        public string? Contenido { get; set; }
        public Guid? FirmanteId { get; set; }
        public string? NumeroAcuse { get; set; }
    }

    public class NuevaAudiencia
    {
        [Required] public DateTime FechaHora { get; set; } = DateTime.Now.AddDays(7);
        [Required] public string Tipo { get; set; } = string.Empty;
        public string? Lugar { get; set; }
        public EstadoAudiencia Estado { get; set; } = EstadoAudiencia.Programada;
        public Guid? AsignadoAId { get; set; }
    }

    public class NuevoPlazo
    {
        [Required] public string Descripcion { get; set; } = string.Empty;
        [Required] public DateOnly FechaInicio { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [Required] public DateOnly FechaVencimiento { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(8));
        public int? DiasOriginales { get; set; }
        public bool DiasHabiles { get; set; } = true;
        public Guid? ResponsableId { get; set; }
    }

    public class NuevaParte
    {
        public RolProcesal Rol { get; set; } = RolProcesal.Demandado;
        public Guid? ClienteId { get; set; }
        public string? NombreLibre { get; set; }
        public string? Representante { get; set; }
        public string? Notas { get; set; }
    }
}
