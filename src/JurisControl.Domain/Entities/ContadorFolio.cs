using JurisControl.Domain.Common;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Contador por despacho + año que emite folios secuenciales para asuntos
/// (JC-2026-0001). Fila única por (DespachoId, Anio). Se actualiza con lock
/// pesimista para garantizar unicidad sin colisiones concurrentes.
/// </summary>
public class ContadorFolio : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DespachoId { get; set; }
    public int Anio { get; set; }
    public int UltimoNumero { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
