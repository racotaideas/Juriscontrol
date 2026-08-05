using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Gestión de cobranza: cada contacto (llamada, visita, correo, WhatsApp)
/// realizado al deudor. Bitácora auditable del despacho.
/// </summary>
public class GestionCobranza : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid CreditoId { get; set; }
    public Credito? Credito { get; set; }

    public DateTime Fecha { get; set; }
    public string Canal { get; set; } = "telefono"; // telefono | visita | correo | whatsapp | sms
    public EstadoGestion Resultado { get; set; } = EstadoGestion.Pendiente;

    public string? PersonaContactada { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateOnly? PromesaFecha { get; set; }
    public decimal? PromesaMonto { get; set; }

    /// <summary>Abogado o gestor que hizo el contacto.</summary>
    public Guid? GestorId { get; set; }
    public ApplicationUser? Gestor { get; set; }
}
