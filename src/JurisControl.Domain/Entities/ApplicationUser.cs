using JurisControl.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Usuario del sistema. Extiende IdentityUser con el despacho al que pertenece.
/// El <see cref="DespachoId"/> es la clave del multi-tenant: viaja como claim en el cookie/JWT
/// y alimenta el ITenantContext que aplica los Global Query Filters de EF Core.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }

    /// <summary>Cédula profesional (solo si el usuario es abogado).</summary>
    public string? CedulaProfesional { get; set; }
    public string? Especialidad { get; set; }
    public string? FotoStorageRef { get; set; }
    public string? FirmaDigitalStorageRef { get; set; }

    public bool Activo { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UltimoAcceso { get; set; }
}
