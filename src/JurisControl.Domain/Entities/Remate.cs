using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Remate judicial del bien en garantía. Puede haber varios (primera almoneda,
/// segunda almoneda, etc.), por eso cada uno es su propia fila con número.
/// </summary>
public class Remate : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid CreditoId { get; set; }
    public Credito? Credito { get; set; }

    /// <summary>1 = primera almoneda, 2 = segunda, etc.</summary>
    public int Almoneda { get; set; } = 1;

    public DateTime FechaHora { get; set; }
    public string? Lugar { get; set; }

    public decimal ValorAvaluoBase { get; set; }
    public decimal? PosturaLegal { get; set; }
    public decimal? MontoFincado { get; set; }
    public string? Postor { get; set; }

    public EstadoRemate Estado { get; set; } = EstadoRemate.Programado;
    public string? Observaciones { get; set; }
}
