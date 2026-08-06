using JurisControl.Domain.Common;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Documento adjunto: puede pertenecer a un Cliente (papeles del cliente) o a un
/// Asunto (documentos del caso). El binario se guarda en <see cref="StorageRef"/>
/// (por ahora ruta local App_Data/uploads/{despacho}/{id}; en futuro S3/Blob).
/// </summary>
public class Documento : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public Guid? AsuntoId { get; set; }
    public Asunto? Asunto { get; set; }

    /// <summary>Si el adjunto pertenece a una actuación específica del juzgado.</summary>
    public Guid? ActuacionId { get; set; }
    public Actuacion? Actuacion { get; set; }

    /// <summary>Si el adjunto pertenece a una promoción del despacho.</summary>
    public Guid? PromocionId { get; set; }
    public Promocion? Promocion { get; set; }

    /// <summary>Nombre visible del documento (ej. "Poder notarial 45892.pdf").</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Categoría libre para agrupar: contrato, poder, escrito, sentencia, etc.</summary>
    public string Categoria { get; set; } = "otro";

    /// <summary>Ruta en storage donde vive el binario.</summary>
    public string StorageRef { get; set; } = string.Empty;

    public string? ContentType { get; set; }
    public long TamanoBytes { get; set; }

    public string? Notas { get; set; }
}
