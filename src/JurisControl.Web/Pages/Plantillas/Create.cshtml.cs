using System.ComponentModel.DataAnnotations;
using JurisControl.Data;
using JurisControl.Data.Services;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JurisControl.Web.Pages.Plantillas;

[Authorize]
public class CreateModel : PageModel
{
    private readonly JurisControlDbContext _db;
    private readonly IPlantillaRenderer _renderer;
    public CreateModel(JurisControlDbContext db, IPlantillaRenderer renderer)
    {
        _db = db; _renderer = renderer;
    }

    [BindProperty] public PlantillaForm Input { get; set; } = new();
    public IReadOnlyList<TokenInfo> TokensDisponibles => _renderer.TokensDisponibles;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        _db.Plantillas.Add(new Plantilla
        {
            Clave = Input.Clave.ToUpperInvariant(),
            Nombre = Input.Nombre,
            Categoria = Input.Categoria,
            Cuerpo = Input.Cuerpo,
            Descripcion = Input.Descripcion,
            CreatedBy = User.Identity?.Name
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Plantilla creada.";
        return RedirectToPage("Index");
    }

    public class PlantillaForm
    {
        [Required, StringLength(50)] public string Clave { get; set; } = string.Empty;
        [Required, StringLength(200)] public string Nombre { get; set; } = string.Empty;
        [Required, StringLength(50)] public string Categoria { get; set; } = "carta";
        [Required] public string Cuerpo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
