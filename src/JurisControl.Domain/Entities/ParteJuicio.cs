using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Parte procesal de un juicio (actor, demandado, tercero, etc.).
/// Puede referenciar a un Cliente registrado (nuestro cliente) o ser una
/// parte externa capturada por nombre libre — no todos los actores/demandados
/// están en la base de clientes. Ninguno de los dos campos es obligatorio
/// individualmente pero al menos uno debe existir (se valida en app).
/// </summary>
public class ParteJuicio : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid JuicioId { get; set; }
    public Juicio? Juicio { get; set; }

    public RolProcesal Rol { get; set; }

    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    /// <summary>Si no viene de la base de clientes, nombre completo capturado a mano.</summary>
    public string? NombreLibre { get; set; }

    /// <summary>Abogado o representante de la contraparte, si aplica.</summary>
    public string? Representante { get; set; }

    public string? Notas { get; set; }
}
