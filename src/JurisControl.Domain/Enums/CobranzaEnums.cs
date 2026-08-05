namespace JurisControl.Domain.Enums;

public enum TipoCredito
{
    Personal = 0,
    Hipotecario = 1,
    Automotriz = 2,
    Empresarial = 3,
    TarjetaCredito = 4,
    Nomina = 5,
    Comercial = 6,
    Otro = 99
}

public enum EstadoCredito
{
    Cartera = 0,
    Judicial = 1,
    Convenio = 2,
    Ejecutado = 3,
    Recuperado = 4,
    Incobrable = 5,
    Sobreseido = 6
}

public enum EstadoGestion
{
    Pendiente = 0,
    Contactado = 1,
    Promesa = 2,
    NoLocalizado = 3,
    Rechazado = 4,
    PagoRecibido = 5
}

public enum EstadoRemate
{
    Programado = 0,
    Suspendido = 1,
    Celebrado = 2,
    Fincado = 3,
    Desierto = 4,
    Cancelado = 5
}

public static class CobranzaLabels
{
    public static string Label(this TipoCredito t) => t switch
    {
        TipoCredito.Personal => "Personal",
        TipoCredito.Hipotecario => "Hipotecario",
        TipoCredito.Automotriz => "Automotriz",
        TipoCredito.Empresarial => "Empresarial",
        TipoCredito.TarjetaCredito => "Tarjeta de crédito",
        TipoCredito.Nomina => "Nómina",
        TipoCredito.Comercial => "Comercial",
        TipoCredito.Otro => "Otro",
        _ => t.ToString()
    };

    public static string Label(this EstadoCredito e) => e switch
    {
        EstadoCredito.Cartera => "En cartera",
        EstadoCredito.Judicial => "En juicio",
        EstadoCredito.Convenio => "Convenio",
        EstadoCredito.Ejecutado => "Ejecutado",
        EstadoCredito.Recuperado => "Recuperado",
        EstadoCredito.Incobrable => "Incobrable",
        EstadoCredito.Sobreseido => "Sobreseído",
        _ => e.ToString()
    };

    public static string BadgeClass(this EstadoCredito e) => e switch
    {
        EstadoCredito.Recuperado => "badge-success",
        EstadoCredito.Incobrable or EstadoCredito.Sobreseido => "badge-danger",
        EstadoCredito.Ejecutado or EstadoCredito.Convenio => "badge-warning",
        _ => "badge-info"
    };

    public static string Label(this EstadoGestion g) => g switch
    {
        EstadoGestion.Pendiente => "Pendiente",
        EstadoGestion.Contactado => "Contactado",
        EstadoGestion.Promesa => "Promesa de pago",
        EstadoGestion.NoLocalizado => "No localizado",
        EstadoGestion.Rechazado => "Rechazado",
        EstadoGestion.PagoRecibido => "Pago recibido",
        _ => g.ToString()
    };

    public static string Label(this EstadoRemate r) => r switch
    {
        EstadoRemate.Programado => "Programado",
        EstadoRemate.Suspendido => "Suspendido",
        EstadoRemate.Celebrado => "Celebrado",
        EstadoRemate.Fincado => "Fincado",
        EstadoRemate.Desierto => "Desierto",
        EstadoRemate.Cancelado => "Cancelado",
        _ => r.ToString()
    };

    public static string BadgeClass(this EstadoRemate r) => r switch
    {
        EstadoRemate.Fincado or EstadoRemate.Celebrado => "badge-success",
        EstadoRemate.Cancelado or EstadoRemate.Desierto => "badge-danger",
        EstadoRemate.Suspendido => "badge-warning",
        _ => "badge-info"
    };
}
