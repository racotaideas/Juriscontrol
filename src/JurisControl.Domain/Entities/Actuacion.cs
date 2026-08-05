using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Cada acto procesal del juzgado dentro de un juicio: acuerdo, notificación,
/// audiencia programada, sentencia. Es el corazón del control de casos.
/// </summary>
public class Actuacion : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid JuicioId { get; set; }
    public Juicio? Juicio { get; set; }

    public TipoActuacion Tipo { get; set; } = TipoActuacion.Acuerdo;

    /// <summary>Fecha en la que el juzgado dictó la actuación.</summary>
    public DateOnly Fecha { get; set; }

    /// <summary>Fecha en la que el despacho se enteró — puede diferir de la anterior.</summary>
    public DateOnly? FechaNotificacion { get; set; }

    /// <summary>Resumen breve, ej. "Auto que admite pruebas".</summary>
    public string Resumen { get; set; } = string.Empty;

    public string? Detalle { get; set; }

    /// <summary>Si esta actuación abre un plazo procesal, se enlaza vía FK.</summary>
    public Guid? PlazoId { get; set; }
}
