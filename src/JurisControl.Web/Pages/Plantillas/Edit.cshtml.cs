using JurisControl.Data;
using JurisControl.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JurisControl.Web.Pages.Plantillas;

[Authorize]
public class EditModel : PageModel
{
    private readonly JurisControlDbContext _db;
    private readonly IPlantillaRenderer _renderer;
    public EditModel(JurisControlDbContext db, IPlantillaRenderer renderer)
    {
        _db = db; _renderer = renderer;
    }

    [BindProperty] public CreateModel.PlantillaForm Input { get; set; } = new();
    [BindProperty] public Guid Id { get; set; }
    public IReadOnlyList<TokenInfo> TokensDisponibles => _renderer.TokensDisponibles;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var p = await _db.Plantillas.FindAsync(id);
        if (p is null) return NotFound();
        Id = p.Id;
        Input = new CreateModel.PlantillaForm
        {
            Clave = p.Clave, Nombre = p.Nombre, Categoria = p.Categoria,
            Cuerpo = p.Cuerpo, Descripcion = p.Descripcion
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var p = await _db.Plantillas.FindAsync(Id);
        if (p is null) return NotFound();
        p.Clave = Input.Clave.ToUpperInvariant();
        p.Nombre = Input.Nombre;
        p.Categoria = Input.Categoria;
        p.Cuerpo = Input.Cuerpo;
        p.Descripcion = Input.Descripcion;
        p.UpdatedBy = User.Identity?.Name;
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Plantilla actualizada.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var p = await _db.Plantillas.FindAsync(Id);
        if (p is null) return NotFound();
        p.Activa = false;
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Plantilla desactivada.";
        return RedirectToPage("Index");
    }
}
