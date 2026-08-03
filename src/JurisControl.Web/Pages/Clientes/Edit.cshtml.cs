using System.ComponentModel.DataAnnotations;
using JurisControl.Data;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JurisControl.Web.Pages.Clientes;

[Authorize]
public class EditModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public EditModel(JurisControlDbContext db) => _db = db;

    [BindProperty] public CreateModel.ClienteForm Input { get; set; } = new();
    [BindProperty] public Guid Id { get; set; }
    public string? DisplayName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var c = await _db.Clientes.FindAsync(id);
        if (c is null) return NotFound();
        Id = c.Id;
        DisplayName = c.DisplayName;
        Input = new CreateModel.ClienteForm
        {
            Tipo = c.Tipo,
            Nombre = c.Nombre,
            ApellidoPaterno = c.ApellidoPaterno,
            ApellidoMaterno = c.ApellidoMaterno,
            Curp = c.Curp,
            RazonSocial = c.RazonSocial,
            NombreComercial = c.NombreComercial,
            RepresentanteLegal = c.RepresentanteLegal,
            Rfc = c.Rfc,
            CorreoPrincipal = c.CorreoPrincipal,
            TelefonoPrincipal = c.TelefonoPrincipal,
            WhatsApp = c.WhatsApp,
            Direccion = c.Direccion,
            Ciudad = c.Ciudad,
            Estado = c.Estado,
            CodigoPostal = c.CodigoPostal,
            ReferidoPor = c.ReferidoPor,
            Etiquetas = c.Etiquetas,
            NotasPrivadas = c.NotasPrivadas
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var c = await _db.Clientes.FindAsync(Id);
        if (c is null) return NotFound();

        c.Tipo = Input.Tipo;
        c.Nombre = Input.Nombre;
        c.ApellidoPaterno = Input.ApellidoPaterno;
        c.ApellidoMaterno = Input.ApellidoMaterno;
        c.Curp = Input.Curp;
        c.RazonSocial = Input.RazonSocial;
        c.NombreComercial = Input.NombreComercial;
        c.RepresentanteLegal = Input.RepresentanteLegal;
        c.Rfc = Input.Rfc?.ToUpperInvariant();
        c.CorreoPrincipal = Input.CorreoPrincipal;
        c.TelefonoPrincipal = Input.TelefonoPrincipal;
        c.WhatsApp = Input.WhatsApp;
        c.Direccion = Input.Direccion;
        c.Ciudad = Input.Ciudad;
        c.Estado = Input.Estado;
        c.CodigoPostal = Input.CodigoPostal;
        c.ReferidoPor = Input.ReferidoPor;
        c.Etiquetas = Input.Etiquetas ?? string.Empty;
        c.NotasPrivadas = Input.NotasPrivadas;
        c.UpdatedBy = User.Identity?.Name;

        await _db.SaveChangesAsync();
        TempData["Msg"] = "Cambios guardados.";
        return RedirectToPage("Details", new { id = c.Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var c = await _db.Clientes.FindAsync(Id);
        if (c is null) return NotFound();
        c.Activo = false;
        c.UpdatedBy = User.Identity?.Name;
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Cliente marcado como inactivo.";
        return RedirectToPage("Index");
    }
}
