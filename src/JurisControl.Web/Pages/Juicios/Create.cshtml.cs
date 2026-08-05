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
public class CreateModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public CreateModel(JurisControlDbContext db) => _db = db;

    [BindProperty] public JuicioForm Input { get; set; } = new();
    public List<SelectListItem> AsuntosList { get; private set; } = new();
    public List<SelectListItem> MateriasList { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? asuntoId)
    {
        if (asuntoId.HasValue)
        {
            Input.AsuntoId = asuntoId.Value;
            var a = await _db.Asuntos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == asuntoId.Value);
            if (a is not null) Input.MateriaKey = a.MateriaKey;
        }
        Input.FechaInicio = DateOnly.FromDateTime(DateTime.Now);
        await LoadListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadListsAsync();
        if (!ModelState.IsValid) return Page();

        var j = new Juicio
        {
            AsuntoId = Input.AsuntoId,
            NumeroExpediente = Input.NumeroExpediente,
            Juzgado = Input.Juzgado,
            TipoJuicio = Input.TipoJuicio,
            MateriaKey = Input.MateriaKey,
            Estado = Input.Estado,
            FechaInicio = Input.FechaInicio,
            Cuantia = Input.Cuantia,
            Descripcion = Input.Descripcion,
            Observaciones = Input.Observaciones,
            CreatedBy = User.Identity?.Name
        };
        _db.Juicios.Add(j);
        await _db.SaveChangesAsync();
        TempData["Msg"] = $"Juicio {j.NumeroExpediente} creado.";
        return RedirectToPage("Details", new { id = j.Id });
    }

    private async Task LoadListsAsync()
    {
        AsuntosList = await (from a in _db.Asuntos.AsNoTracking()
                             join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id
                             orderby a.CreatedAt descending
                             select new SelectListItem
                             {
                                 Value = a.Id.ToString(),
                                 Text = a.Folio + " · " + a.Titulo + " (" +
                                        (c.RazonSocial ?? c.NombreComercial ??
                                         ((c.Nombre ?? "") + " " + (c.ApellidoPaterno ?? "")).Trim()) + ")"
                             }).Take(500).ToListAsync();

        MateriasList = Materia.All.Select(m => new SelectListItem { Value = m, Text = Materia.Label(m) }).ToList();
    }

    public class JuicioForm
    {
        [Required] public Guid AsuntoId { get; set; }
        [Required, StringLength(50)] public string NumeroExpediente { get; set; } = string.Empty;
        [Required, StringLength(200)] public string Juzgado { get; set; } = string.Empty;
        [Required, StringLength(150)] public string TipoJuicio { get; set; } = string.Empty;
        [Required] public string MateriaKey { get; set; } = Materia.Civil;
        public EstadoJuicio Estado { get; set; } = EstadoJuicio.Iniciado;
        [Required] public DateOnly FechaInicio { get; set; }
        public decimal? Cuantia { get; set; }
        public string? Descripcion { get; set; }
        public string? Observaciones { get; set; }
    }
}
