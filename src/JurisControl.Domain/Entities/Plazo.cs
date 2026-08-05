using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Plazo procesal. Vive colgado de un Juicio y opcionalmente de una Actuacion
/// que lo detonó (ej. "notificado sentencia → 8 días para apelar"). El estado
/// se calcula/actualiza automáticamente al comparar FechaVencimiento con hoy.
/// </summary>
public class Plazo : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid JuicioId { get; set; }
    public Juicio? Juicio { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaVencimiento { get; set; }

    public EstadoPlazo Estado { get; set; } = EstadoPlazo.Abierto;

    /// <summary>Días naturales o hábiles configurados originalmente.</summary>
    public int? DiasOriginales { get; set; }
    public bool DiasHabiles { get; set; } = true;

    /// <summary>Abogado responsable de cumplir el plazo.</summary>
    public Guid? ResponsableId { get; set; }
    public ApplicationUser? Responsable { get; set; }

    public DateTimeOffset? FechaCumplimiento { get; set; }
    public string? NotasCumplimiento { get; set; }
}
