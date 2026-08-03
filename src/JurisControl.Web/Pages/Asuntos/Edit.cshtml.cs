using JurisControl.Data;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Asuntos;

[Authorize]
public class EditModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public EditModel(JurisControlDbContext db) => _db = db;

    [BindProperty] public CreateModel.AsuntoForm Input { get; set; } = new();
    [BindProperty] public Guid Id { get; set; }
    public string Folio { get; private set; } = string.Empty;

    public List<SelectListItem> ClientesList { get; private set; } = new();
    public List<SelectListItem> UsuariosList { get; private set; } = new();
    public List<SelectListItem> MateriasList { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var a = await _db.Asuntos.FindAsync(id);
        if (a is null) return NotFound();
        Id = a.Id;
        Folio = a.Folio;
        Input = new CreateModel.AsuntoForm
        {
            Titulo = a.Titulo,
            MateriaKey = a.MateriaKey,
            ClienteId = a.ClienteId,
            ResponsableId = a.ResponsableId,
            Estado = a.Estado,
            FechaRecepcion = DateOnly.FromDateTime(a.FechaRecepcion.LocalDateTime),
            Descripcion = a.Descripcion,
            NotasPrivadas = a.NotasPrivadas,
            Cuantia = a.Cuantia,
            Prioridad = a.Prioridad,
            Etiquetas = a.Etiquetas,
            EsCobranza = a.EsCobranza
        };
        await LoadListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadListsAsync();
        if (!ModelState.IsValid) return Page();

        var a = await _db.Asuntos.FindAsync(Id);
        if (a is null) return NotFound();

        a.Titulo = Input.Titulo;
        a.MateriaKey = Input.MateriaKey;
        a.ClienteId = Input.ClienteId;
        a.ResponsableId = Input.ResponsableId;
        var estadoAnterior = a.Estado;
        a.Estado = Input.Estado;
        if (estadoAnterior != EstadoAsunto.Cerrado && Input.Estado == EstadoAsunto.Cerrado)
            a.FechaCierre = DateTimeOffset.UtcNow;
        if (Input.Estado != EstadoAsunto.Cerrado)
            a.FechaCierre = null;
        a.FechaRecepcion = Input.FechaRecepcion.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        a.Descripcion = Input.Descripcion;
        a.NotasPrivadas = Input.NotasPrivadas;
        a.Cuantia = Input.Cuantia;
        a.Prioridad = Input.Prioridad;
        a.Etiquetas = Input.Etiquetas ?? string.Empty;
        a.EsCobranza = Input.EsCobranza;
        a.UpdatedBy = User.Identity?.Name;

        await _db.SaveChangesAsync();
        TempData["Msg"] = "Cambios guardados.";
        return RedirectToPage("Details", new { id = a.Id });
    }

    private async Task LoadListsAsync()
    {
        ClientesList = await _db.Clientes.AsNoTracking().Where(c => c.Activo)
            .OrderBy(c => c.RazonSocial ?? c.ApellidoPaterno ?? c.Nombre)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = (c.RazonSocial ?? c.NombreComercial ??
                       ((c.Nombre ?? "") + " " + (c.ApellidoPaterno ?? "")).Trim())
            }).ToListAsync();

        UsuariosList = await _db.Users.AsNoTracking().Where(u => u.Activo)
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.NombreCompleto ?? u.Email ?? "" })
            .ToListAsync();

        MateriasList = Materia.All.Select(m => new SelectListItem
        {
            Value = m,
            Text = Materia.Label(m)
        }).ToList();
    }
}
