using JurisControl.Domain.Common;

namespace JurisControl.Domain.Entities;

/// <summary>
/// Pago recibido contra un crédito. Reduce el saldo. En 3NF: pertenece a
/// exactamente un Credito, no repite datos del deudor ni del acreedor.
/// </summary>
public class PagoCobranza : AuditableEntity, ITenantEntity
{
    public Guid DespachoId { get; set; }
    public Despacho? Despacho { get; set; }

    public Guid CreditoId { get; set; }
    public Credito? Credito { get; set; }

    public DateOnly Fecha { get; set; }
    public decimal Monto { get; set; }

    /// <summary>Aplicación del pago: capital / interés / gastos.</summary>
    public decimal AplicadoCapital { get; set; }
    public decimal AplicadoInteres { get; set; }
    public decimal AplicadoGastos { get; set; }

    public string? MedioPago { get; set; }
    public string? Referencia { get; set; }
    public string? Notas { get; set; }
}
