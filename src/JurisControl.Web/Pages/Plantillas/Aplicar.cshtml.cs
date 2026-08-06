using JurisControl.Data;
using JurisControl.Data.Services;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Plantillas;

[Authorize]
public class AplicarModel : PageModel
{
    private readonly JurisControlDbContext _db;
    private readonly IPlantillaRenderer _renderer;
    public AplicarModel(JurisControlDbContext db, IPlantillaRenderer renderer)
    {
        _db = db; _renderer = renderer;
    }

    public Plantilla Plantilla { get; private set; } = null!;
    public List<SelectListItem> AsuntosList { get; private set; } = new();
    public List<SelectListItem> JuiciosList { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? AsuntoId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? JuicioId { get; set; }

    public string? Resultado { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var p = await _db.Plantillas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
        if (p is null) return NotFound();
        Plantilla = p;

        AsuntosList = await (from a in _db.Asuntos.AsNoTracking()
                             join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id
                             orderby a.CreatedAt descending
                             select new SelectListItem
                             {
                                 Value = a.Id.ToString(),
                                 Text = a.Folio + " · " + a.Titulo
                             }).Take(300).ToListAsync();

        if (AsuntoId.HasValue)
        {
            JuiciosList = await _db.Juicios.AsNoTracking()
                .Where(j => j.AsuntoId == AsuntoId.Value)
                .Select(j => new SelectListItem { Value = j.Id.ToString(), Text = j.NumeroExpediente + " · " + j.Juzgado })
                .ToListAsync();

            Resultado = await _renderer.RenderAsync(p, AsuntoId.Value, JuicioId);
        }

        return Page();
    }
}
