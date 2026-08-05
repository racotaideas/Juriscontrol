using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Escrito o promoción presentada por el despacho ante el juzgado.
/// Distinto de Actuacion (que es del juzgado) — Promocion es del despacho.
/// </summary>
public class Promocion : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid JuicioId { get; set; }
    public Juicio? Juicio { get; set; }

    public TipoPromocion Tipo { get; set; } = TipoPromocion.Otro;

    public DateOnly FechaPresentacion { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Contenido { get; set; }

    /// <summary>Abogado firmante de la promoción.</summary>
    public Guid? FirmanteId { get; set; }
    public ApplicationUser? Firmante { get; set; }

    public string? NumeroAcuse { get; set; }
}
