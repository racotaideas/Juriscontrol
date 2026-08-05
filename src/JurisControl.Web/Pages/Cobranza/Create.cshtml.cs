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
public class CreateModel : PageModel
{
    private readonly JurisControlDbContext _db;
    public CreateModel(JurisControlDbContext db) => _db = db;

    [BindProperty] public CreditoForm Input { get; set; } = new();
    public List<SelectListItem> AsuntosCobranzaList { get; private set; } = new();
    public List<SelectListItem> ClientesList { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadListsAsync();
        if (!ModelState.IsValid) return Page();

        var c = new Credito
        {
            AsuntoId = Input.AsuntoId,
            DeudorClienteId = Input.DeudorClienteId,
            NombreDeudor = Input.NombreDeudor,
            NumeroCredito = Input.NumeroCredito,
            Acreedor = Input.Acreedor,
            Tipo = Input.Tipo,
            Estado = Input.Estado,
            MontoOriginal = Input.MontoOriginal,
            SaldoActual = Input.SaldoActual,
            TasaInteres = Input.TasaInteres,
            FechaOrigen = Input.FechaOrigen,
            FechaUltimoPago = Input.FechaUltimoPago,
            FechaVencimiento = Input.FechaVencimiento,
            DiasMora = Input.DiasMora,
            Garantia = Input.Garantia,
            Observaciones = Input.Observaciones,
            CreatedBy = User.Identity?.Name
        };
        _db.Creditos.Add(c);
        await _db.SaveChangesAsync();
        TempData["Msg"] = $"Crédito {c.NumeroCredito} creado.";
        return RedirectToPage("Details", new { id = c.Id });
    }

    private async Task LoadListsAsync()
    {
        AsuntosCobranzaList = await (from a in _db.Asuntos.AsNoTracking()
                                     join cl in _db.Clientes.AsNoTracking() on a.ClienteId equals cl.Id
                                     where a.EsCobranza
                                     orderby a.CreatedAt descending
                                     select new SelectListItem
                                     {
                                         Value = a.Id.ToString(),
                                         Text = a.Folio + " · " + a.Titulo
                                     }).Take(500).ToListAsync();

        ClientesList = await _db.Clientes.AsNoTracking().Where(x => x.Activo)
            .OrderBy(x => x.RazonSocial ?? x.ApellidoPaterno ?? x.Nombre)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.RazonSocial ?? x.NombreComercial ??
                       ((x.Nombre ?? "") + " " + (x.ApellidoPaterno ?? "")).Trim()
            }).ToListAsync();
    }

    public class CreditoForm
    {
        [Required] public Guid AsuntoId { get; set; }
        public Guid? DeudorClienteId { get; set; }
        public string? NombreDeudor { get; set; }
        [Required, StringLength(80)] public string NumeroCredito { get; set; } = string.Empty;
        [Required, StringLength(200)] public string Acreedor { get; set; } = string.Empty;
        public TipoCredito Tipo { get; set; } = TipoCredito.Personal;
        public EstadoCredito Estado { get; set; } = EstadoCredito.Cartera;
        public decimal MontoOriginal { get; set; }
        public decimal SaldoActual { get; set; }
        public decimal? TasaInteres { get; set; }
        public DateOnly? FechaOrigen { get; set; }
        public DateOnly? FechaUltimoPago { get; set; }
        public DateOnly? FechaVencimiento { get; set; }
        public int? DiasMora { get; set; }
        public string? Garantia { get; set; }
        public string? Observaciones { get; set; }
    }
}
