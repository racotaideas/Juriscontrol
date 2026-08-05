using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Audiencia programada dentro de un juicio: fecha, hora, lugar, estado,
/// y quién del despacho la atiende.
/// </summary>
public class Audiencia : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid JuicioId { get; set; }
    public Juicio? Juicio { get; set; }

    public DateTime FechaHora { get; set; }
    public TimeSpan? Duracion { get; set; }

    /// <summary>Tipo libre: conciliación, audiencia inicial, testimonial, etc.</summary>
    public string Tipo { get; set; } = string.Empty;

    public string? Lugar { get; set; }

    public EstadoAudiencia Estado { get; set; } = EstadoAudiencia.Programada;

    /// <summary>Abogado del despacho asignado a la audiencia.</summary>
    public Guid? AsignadoAId { get; set; }
    public ApplicationUser? AsignadoA { get; set; }

    public string? Resultado { get; set; }
    public string? Observaciones { get; set; }

    /// <summary>Cuando se difiere, aquí queda la nueva fecha propuesta.</summary>
    public DateTime? FechaDiferida { get; set; }
}
