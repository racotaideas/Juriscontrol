using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

public class Cliente : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public TipoCliente Tipo { get; set; }

    // Persona física
    public string? Nombre { get; set; }
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Curp { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? Ocupacion { get; set; }

    // Persona moral
    public string? RazonSocial { get; set; }
    public string? NombreComercial { get; set; }
    public string? RepresentanteLegal { get; set; }
    public DateOnly? FechaConstitucion { get; set; }

    // Común
    public string? Rfc { get; set; }
    public string? CorreoPrincipal { get; set; }
    public string? TelefonoPrincipal { get; set; }
    public string? WhatsApp { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public string? CodigoPostal { get; set; }

    public string? ReferidoPor { get; set; }

    /// <summary>Etiquetas separadas por coma. Ej: "VIP,corporativo,retenedor-mensual".</summary>
    public string Etiquetas { get; set; } = string.Empty;

    public string? NotasPrivadas { get; set; }

    public bool Activo { get; set; } = true;

    public string DisplayName => Tipo == TipoCliente.PersonaMoral
        ? (RazonSocial ?? NombreComercial ?? string.Empty)
        : string.Join(' ', new[] { Nombre, ApellidoPaterno, ApellidoMaterno }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
}
