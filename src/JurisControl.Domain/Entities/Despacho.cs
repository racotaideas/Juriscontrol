using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

public class Despacho : AuditableEntity
{
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string? Rfc { get; set; }

    public string? DomicilioFiscal { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public string? CodigoPostal { get; set; }

    public string? CorreoInstitucional { get; set; }
    public string? WhatsApp { get; set; }
    public string? SitioWeb { get; set; }
    public string? LogoStorageRef { get; set; }

    public string ZonaHoraria { get; set; } = "America/Mexico_City";

    /// <summary>Materias en las que trabaja el despacho, separadas por coma. Ej: "civil,mercantil,familiar".</summary>
    public string MateriasAtiende { get; set; } = string.Empty;

    /// <summary>Si está activo, aparecen los módulos de garantías, almonedas, gastos por concepto bancario y la jerarquía Regional/Zona/Plaza/Sucursal.</summary>
    public bool ModoCobranza { get; set; }

    public EstadoDespacho Estado_ { get; set; } = EstadoDespacho.Activo;

    public DateTimeOffset FechaAlta { get; set; }
    public DateTimeOffset? FechaRenovacion { get; set; }
}
