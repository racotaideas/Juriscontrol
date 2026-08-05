using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Juicio = expediente judicial derivado de un Asunto. Un Asunto puede tener
/// 0..N Juicios (ej. contrato → demanda mercantil + amparo). En 3NF, todos
/// los atributos dependen del PK Id y las relaciones van vía FK.
/// </summary>
public class Juicio : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid AsuntoId { get; set; }
    public Asunto? Asunto { get; set; }

    /// <summary>Número de expediente asignado por el juzgado, ej. "234/2026".</summary>
    public string NumeroExpediente { get; set; } = string.Empty;

    /// <summary>Juzgado o tribunal completo, ej. "Juzgado Sexto de lo Civil, CDMX".</summary>
    public string Juzgado { get; set; } = string.Empty;

    /// <summary>Tipo de juicio libre, ej. "Ordinario mercantil", "Ejecutivo mercantil".</summary>
    public string TipoJuicio { get; set; } = string.Empty;

    public string MateriaKey { get; set; } = Materia.Civil;

    public EstadoJuicio Estado { get; set; } = EstadoJuicio.Iniciado;

    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaConclusion { get; set; }

    public decimal? Cuantia { get; set; }

    /// <summary>Resumen de pretensiones o hechos relevantes.</summary>
    public string? Descripcion { get; set; }

    public string? Observaciones { get; set; }
}
