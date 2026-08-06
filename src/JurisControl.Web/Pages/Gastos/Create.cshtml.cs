using System.ComponentModel.DataAnnotations;
using JurisControl.Data;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Gastos;

[Authorize]
public class CreateModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public CreateModel(JurisControlDbContext db) => _db = db;

    [BindProperty] public GastoForm Input { get; set; } = new();
    public List<SelectListItem> AsuntosList { get; private set; } = new();
    public List<SelectListItem> JuiciosList { get; private set; } = new();

    public static readonly string[] Categorias =
    { "honorarios", "copias", "viáticos", "perito", "judiciales", "notariales", "otro" };

    public async Task<IActionResult> OnGetAsync(Guid? juicioId, Guid? asuntoId)
    {
        Input.Fecha = DateOnly.FromDateTime(DateTime.Now);
        Input.JuicioId = juicioId;
        Input.AsuntoId = asuntoId;
        await LoadListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadListsAsync();
        if (!ModelState.IsValid) return Page();

        _db.Gastos.Add(new Gasto
        {
            JuicioId = Input.JuicioId,
            AsuntoId = Input.AsuntoId,
            Fecha = Input.Fecha,
            Categoria = Input.Categoria,
            Concepto = Input.Concepto,
            Monto = Input.Monto,
            Reembolsable = Input.Reembolsable,
            Estado = Input.Estado,
            Comprobante = Input.Comprobante,
            Notas = Input.Notas,
            CreatedBy = User.Identity?.Name
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Gasto registrado.";
        return RedirectToPage("Index");
    }

    private async Task LoadListsAsync()
    {
        AsuntosList = await (from a in _db.Asuntos.AsNoTracking()
                             orderby a.CreatedAt descending
                             select new SelectListItem { Value = a.Id.ToString(), Text = a.Folio + " · " + a.Titulo })
                            .Take(300).ToListAsync();
        JuiciosList = await _db.Juicios.AsNoTracking()
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new SelectListItem { Value = j.Id.ToString(), Text = j.NumeroExpediente + " · " + j.Juzgado })
            .Take(300).ToListAsync();
    }

    public class GastoForm
    {
        public Guid? JuicioId { get; set; }
        public Guid? AsuntoId { get; set; }
        [Required] public DateOnly Fecha { get; set; }
        [Required] public string Categoria { get; set; } = "otro";
        [Required, StringLength(300)] public string Concepto { get; set; } = string.Empty;
        [Range(0.01, double.MaxValue)] public decimal Monto { get; set; }
        public bool Reembolsable { get; set; } = true;
        public string Estado { get; set; } = "pendiente";
        public string? Comprobante { get; set; }
        public string? Notas { get; set; }
    }
}
