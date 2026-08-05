using System.ComponentModel.DataAnnotations;
using JurisControl.Data;
using JurisControl.Domain.Entities;
using JurisControl.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Web.Pages.Cobranza;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public DetailsModel(JurisControlDbContext db) => _db = db;

    public Credito Credito { get; private set; } = null!;
    public Asunto? Asunto { get; private set; }
    public List<PagoCobranza> Pagos { get; private set; } = new();
    public List<GestionCobranza> Gestiones { get; private set; } = new();
    public List<Remate> Remates { get; private set; } = new();
    public List<SelectListItem> UsuariosList { get; private set; } = new();

    public decimal TotalCobrado { get; private set; }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty] public NuevoPago Pago { get; set; } = new();
    [BindProperty] public NuevaGestion Gestion { get; set; } = new();
    [BindProperty] public NuevoRemate Remate { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Id = id;
        if (!await LoadAsync(id)) return NotFound();
        return Page();
    }

    private async Task<bool> LoadAsync(Guid id)
    {
        var c = await _db.Creditos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return false;
        Credito = c;
        Asunto = await _db.Asuntos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == c.AsuntoId);
        Pagos = await _db.PagosCobranza.AsNoTracking().Where(p => p.CreditoId == id)
            .OrderByDescending(p => p.Fecha).ToListAsync();
        Gestiones = await _db.GestionesCobranza.AsNoTracking().Where(g => g.CreditoId == id)
            .OrderByDescending(g => g.Fecha).ToListAsync();
        Remates = await _db.Remates.AsNoTracking().Where(r => r.CreditoId == id)
            .OrderBy(r => r.Almoneda).ToListAsync();
        UsuariosList = await _db.Users.AsNoTracking().Where(u => u.Activo)
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.NombreCompleto ?? u.Email ?? "" })
            .ToListAsync();

        TotalCobrado = Pagos.Sum(p => p.Monto);
        return true;
    }

    public async Task<IActionResult> OnPostPagoAsync()
    {
        if (!await LoadAsync(Id)) return NotFound();
        if (Pago.Monto <= 0)
        {
            ModelState.AddModelError("Pago.Monto", "El monto debe ser mayor a 0.");
            return Page();
        }

        var credito = await _db.Creditos.FirstAsync(x => x.Id == Id);
        _db.PagosCobranza.Add(new PagoCobranza
        {
            CreditoId = Id,
            Fecha = Pago.Fecha,
            Monto = Pago.Monto,
            AplicadoCapital = Pago.AplicadoCapital,
            AplicadoInteres = Pago.AplicadoInteres,
            AplicadoGastos = Pago.AplicadoGastos,
            MedioPago = Pago.MedioPago,
            Referencia = Pago.Referencia,
            Notas = Pago.Notas,
            CreatedBy = User.Identity?.Name
        });
        credito.SaldoActual = Math.Max(0, credito.SaldoActual - Pago.AplicadoCapital);
        credito.FechaUltimoPago = Pago.Fecha;
        if (credito.SaldoActual == 0) credito.Estado = EstadoCredito.Recuperado;
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Pago registrado.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostGestionAsync()
    {
        if (!await LoadAsync(Id)) return NotFound();
        if (string.IsNullOrWhiteSpace(Gestion.Descripcion))
        {
            ModelState.AddModelError("Gestion.Descripcion", "Descripción obligatoria.");
            return Page();
        }
        _db.GestionesCobranza.Add(new GestionCobranza
        {
            CreditoId = Id,
            Fecha = Gestion.Fecha,
            Canal = Gestion.Canal,
            Resultado = Gestion.Resultado,
            PersonaContactada = Gestion.PersonaContactada,
            Descripcion = Gestion.Descripcion,
            PromesaFecha = Gestion.PromesaFecha,
            PromesaMonto = Gestion.PromesaMonto,
            GestorId = Gestion.GestorId,
            CreatedBy = User.Identity?.Name
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Gestión registrada.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostRemateAsync()
    {
        if (!await LoadAsync(Id)) return NotFound();
        _db.Remates.Add(new Remate
        {
            CreditoId = Id,
            Almoneda = Remate.Almoneda,
            FechaHora = Remate.FechaHora,
            Lugar = Remate.Lugar,
            ValorAvaluoBase = Remate.ValorAvaluoBase,
            PosturaLegal = Remate.PosturaLegal,
            Estado = Remate.Estado,
            Observaciones = Remate.Observaciones,
            CreatedBy = User.Identity?.Name
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Remate programado.";
        return RedirectToPage(new { id = Id });
    }

    public class NuevoPago
    {
        [Required] public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public decimal Monto { get; set; }
        public decimal AplicadoCapital { get; set; }
        public decimal AplicadoInteres { get; set; }
        public decimal AplicadoGastos { get; set; }
        public string? MedioPago { get; set; }
        public string? Referencia { get; set; }
        public string? Notas { get; set; }
    }

    public class NuevaGestion
    {
        [Required] public DateTime Fecha { get; set; } = DateTime.Now;
        public string Canal { get; set; } = "telefono";
        public EstadoGestion Resultado { get; set; } = EstadoGestion.Contactado;
        public string? PersonaContactada { get; set; }
        [Required] public string Descripcion { get; set; } = string.Empty;
        public DateOnly? PromesaFecha { get; set; }
        public decimal? PromesaMonto { get; set; }
        public Guid? GestorId { get; set; }
    }

    public class NuevoRemate
    {
        public int Almoneda { get; set; } = 1;
        public DateTime FechaHora { get; set; } = DateTime.Now.AddDays(30);
        public string? Lugar { get; set; }
        public decimal ValorAvaluoBase { get; set; }
        public decimal? PosturaLegal { get; set; }
        public EstadoRemate Estado { get; set; } = EstadoRemate.Programado;
        public string? Observaciones { get; set; }
    }
}
