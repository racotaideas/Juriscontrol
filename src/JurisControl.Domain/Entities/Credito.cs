using JurisControl.Domain.Common;
using JurisControl.Domain.Enums;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Crédito de cartera de cobranza. En el modo cobranza del despacho, un
/// asunto se ve como un crédito con saldo, tasa, garantía y estado.
/// Vive en 3NF: los pagos van en Pago, las gestiones en GestionCobranza,
/// los remates en Remate — todos con FK a Credito.
/// </summary>
public class Credito : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    /// <summary>Asunto contenedor. Un crédito siempre pertenece a un asunto marcado como EsCobranza.</summary>
    public Guid AsuntoId { get; set; }
    public Asunto? Asunto { get; set; }

    /// <summary>Deudor principal. FK opcional al catálogo de clientes; si es externo va NombreDeudor.</summary>
    public Guid? DeudorClienteId { get; set; }
    public Cliente? DeudorCliente { get; set; }
    public string? NombreDeudor { get; set; }

    /// <summary>Número de contrato o crédito en el sistema del acreedor.</summary>
    public string NumeroCredito { get; set; } = string.Empty;

    /// <summary>Institución acreedora (banco, empresa, dependencia).</summary>
    public string Acreedor { get; set; } = string.Empty;

    public TipoCredito Tipo { get; set; } = TipoCredito.Personal;
    public EstadoCredito Estado { get; set; } = EstadoCredito.Cartera;

    public decimal MontoOriginal { get; set; }
    public decimal SaldoActual { get; set; }
    public decimal? TasaInteres { get; set; }

    public DateOnly? FechaOrigen { get; set; }
    public DateOnly? FechaUltimoPago { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public int? DiasMora { get; set; }

    /// <summary>Descripción de la garantía (inmueble, aval, prenda, etc.).</summary>
    public string? Garantia { get; set; }

    public string? Observaciones { get; set; }
}
