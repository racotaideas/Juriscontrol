using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Un asunto es la unidad de trabajo del despacho: un problema legal que se
/// trabaja para un cliente. Puede o no derivar en un Juicio. Sigue la ontología
/// del manual clásico ISAC 1995 y la especificación JurisControl v2.
/// </summary>
public class Asunto : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    /// <summary>Folio autogenerado por el despacho, formato JC-2026-0001.</summary>
    public string Folio { get; set; } = string.Empty;

    /// <summary>Título corto que el despacho usa internamente (ej. "Contrato Bimbo — revisión").</summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Materia clave de <see cref="Materia"/>. Debe estar dentro de las que atiende el despacho.</summary>
    public string MateriaKey { get; set; } = Materia.Civil;

    public EstadoAsunto Estado { get; set; } = EstadoAsunto.Recibido;

    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    /// <summary>Abogado responsable (opcional hasta que se asigna).</summary>
    public Guid? ResponsableId { get; set; }
    public ApplicationUser? Responsable { get; set; }

    public DateTimeOffset FechaRecepcion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FechaCierre { get; set; }

    /// <summary>Descripción libre del asunto — antecedentes, pretensiones, contexto.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Notas privadas del despacho, no visibles al cliente.</summary>
    public string? NotasPrivadas { get; set; }

    /// <summary>Cuantía monetaria si aplica (contratos, cobranza, indemnizaciones).</summary>
    public decimal? Cuantia { get; set; }

    /// <summary>Prioridad 1 (alta) a 5 (baja).</summary>
    public int Prioridad { get; set; } = 3;

    /// <summary>Etiquetas separadas por coma, ej "urgente,retenedor".</summary>
    public string Etiquetas { get; set; } = string.Empty;

    /// <summary>Cuando el despacho activa modo cobranza, este asunto puede
    /// tener extensiones bancarias en el módulo de cobranza.</summary>
    public bool EsCobranza { get; set; }
}
