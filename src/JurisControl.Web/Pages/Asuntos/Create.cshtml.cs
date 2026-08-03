using System.ComponentModel.DataAnnotations;
using JurisControl.Data;
using JurisControl.Data.Services;
using JurisControl.Data.TenantContext;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Asuntos;

[Authorize]
public class CreateModel : PageModel
{
    private readonly JurisControlDbContext _db;
    private readonly IFolioService _folios;
    private readonly UserManager<ApplicationUser> _users;

    public CreateModel(JurisControlDbContext db, IFolioService folios, UserManager<ApplicationUser> users)
    {
        _db = db;
        _folios = folios;
        _users = users;
    }

    [BindProperty] public AsuntoForm Input { get; set; } = new();
    public List<SelectListItem> ClientesList { get; private set; } = new();
    public List<SelectListItem> UsuariosList { get; private set; } = new();
    public List<SelectListItem> MateriasList { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? clienteId)
    {
        if (clienteId.HasValue) Input.ClienteId = clienteId.Value;
        Input.FechaRecepcion = DateOnly.FromDateTime(DateTime.Now);
        await LoadListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadListsAsync();
        if (!ModelState.IsValid) return Page();

        var folio = await _folios.SiguienteFolioAsuntoAsync();
        var user = await _users.GetUserAsync(User);

        var a = new Asunto
        {
            Folio = folio,
            Titulo = Input.Titulo,
            MateriaKey = Input.MateriaKey,
            ClienteId = Input.ClienteId,
            ResponsableId = Input.ResponsableId,
            Estado = Input.Estado,
            FechaRecepcion = Input.FechaRecepcion.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local),
            Descripcion = Input.Descripcion,
            NotasPrivadas = Input.NotasPrivadas,
            Cuantia = Input.Cuantia,
            Prioridad = Input.Prioridad,
            Etiquetas = Input.Etiquetas ?? string.Empty,
            EsCobranza = Input.EsCobranza,
            CreatedBy = user?.Email
        };
        _db.Asuntos.Add(a);
        await _db.SaveChangesAsync();
        TempData["Msg"] = $"Asunto {a.Folio} creado.";
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
                       ((c.Nombre ?? "") + " " + (c.ApellidoPaterno ?? "") + " " + (c.ApellidoMaterno ?? "")).Trim())
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

    public class AsuntoForm
    {
        [Required, StringLength(300)] public string Titulo { get; set; } = string.Empty;
        [Required] public string MateriaKey { get; set; } = Materia.Civil;
        [Required] public Guid ClienteId { get; set; }
        public Guid? ResponsableId { get; set; }
        public EstadoAsunto Estado { get; set; } = EstadoAsunto.Recibido;
        [Required] public DateOnly FechaRecepcion { get; set; }
        public string? Descripcion { get; set; }
        public string? NotasPrivadas { get; set; }
        public decimal? Cuantia { get; set; }
        [Range(1, 5)] public int Prioridad { get; set; } = 3;
        public string? Etiquetas { get; set; }
        public bool EsCobranza { get; set; }
    }
}
