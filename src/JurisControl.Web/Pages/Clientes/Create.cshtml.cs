using System.ComponentModel.DataAnnotations;
using JurisControl.Data;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JurisControl.Web.Pages.Clientes;

[Authorize]
public class CreateModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public CreateModel(JurisControlDbContext db) => _db = db;

    [BindProperty] public ClienteForm Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var cliente = new Cliente
        {
            Tipo = Input.Tipo,
            Nombre = Input.Nombre,
            ApellidoPaterno = Input.ApellidoPaterno,
            ApellidoMaterno = Input.ApellidoMaterno,
            Curp = Input.Curp,
            RazonSocial = Input.RazonSocial,
            NombreComercial = Input.NombreComercial,
            RepresentanteLegal = Input.RepresentanteLegal,
            Rfc = Input.Rfc?.ToUpperInvariant(),
            CorreoPrincipal = Input.CorreoPrincipal,
            TelefonoPrincipal = Input.TelefonoPrincipal,
            WhatsApp = Input.WhatsApp,
            Direccion = Input.Direccion,
            Ciudad = Input.Ciudad,
            Estado = Input.Estado,
            CodigoPostal = Input.CodigoPostal,
            ReferidoPor = Input.ReferidoPor,
            Etiquetas = Input.Etiquetas ?? string.Empty,
            NotasPrivadas = Input.NotasPrivadas,
            CreatedBy = User.Identity?.Name
        };

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        TempData["Msg"] = $"Cliente '{cliente.DisplayName}' creado.";
        return RedirectToPage("Details", new { id = cliente.Id });
    }

    public class ClienteForm
    {
        [Required] public TipoCliente Tipo { get; set; } = TipoCliente.PersonaFisica;

        // Física
        public string? Nombre { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        [StringLength(18)] public string? Curp { get; set; }

        // Moral
        public string? RazonSocial { get; set; }
        public string? NombreComercial { get; set; }
        public string? RepresentanteLegal { get; set; }

        [StringLength(13)] public string? Rfc { get; set; }
        [EmailAddress] public string? CorreoPrincipal { get; set; }
        public string? TelefonoPrincipal { get; set; }
        public string? WhatsApp { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Estado { get; set; }
        [StringLength(10)] public string? CodigoPostal { get; set; }
        public string? ReferidoPor { get; set; }
        public string? Etiquetas { get; set; }
        public string? NotasPrivadas { get; set; }
    }
}
