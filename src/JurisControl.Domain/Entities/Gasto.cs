using JurisControl.Domain.Common;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Gasto o erogación asociada a un juicio (o al asunto si el gasto es previo
/// a que se abra un juicio). Incluye viáticos, copias, honorarios de perito,
/// gastos judiciales, transporte, notariales, etc. — bitácora del manual clásico.
/// </summary>
public class Gasto : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid? JuicioId { get; set; }
    public Juicio? Juicio { get; set; }

    public Guid? AsuntoId { get; set; }
    public Asunto? Asunto { get; set; }

    public DateOnly Fecha { get; set; }

    /// <summary>Categoría: honorarios, copias, viáticos, perito, judiciales, notariales, otro.</summary>
    public string Categoria { get; set; } = "otro";

    public string Concepto { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    /// <summary>¿El gasto es recuperable del cliente?</summary>
    public bool Reembolsable { get; set; } = true;

    /// <summary>Estado: pendiente, reembolsado, absorbido.</summary>
    public string Estado { get; set; } = "pendiente";

    public string? Comprobante { get; set; }
    public string? Notas { get; set; }
}
