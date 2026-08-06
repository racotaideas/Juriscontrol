using JurisControl.Domain.Common;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Plantilla ("machote") de documento. El cuerpo lleva tokens tipo
/// <c>{{NOMBRE_DEL_DEMANDADO}}</c> que el motor de resolución reemplaza contra
/// los datos de un Asunto/Juicio/Cliente al momento de generar el escrito.
///
/// Reemplaza el módulo "Creación de formatos / Machotes" del manual clásico
/// (Capítulo 6). En vez de macros tipo ^N/^M usamos Markdown básico y HTML
/// para negritas/subrayado, y el usuario imprime desde el navegador.
/// </summary>
public class Plantilla : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    /// <summary>Clave corta única por despacho, ej. "CARTA-COBRO-1".</summary>
    public string Clave { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Categoría libre: carta, escrito, informe, notificación, etc.</summary>
    public string Categoria { get; set; } = "carta";

    /// <summary>
    /// Cuerpo con tokens. Ej: "Sr. {{NOMBRE_DEL_DEMANDADO}}, le informamos
    /// que el saldo de {{CUANTIA_LETRAS}} está vencido…"
    /// </summary>
    public string Cuerpo { get; set; } = string.Empty;

    /// <summary>Notas internas para el despacho sobre cuándo usar la plantilla.</summary>
    public string? Descripcion { get; set; }

    public bool Activa { get; set; } = true;
}
