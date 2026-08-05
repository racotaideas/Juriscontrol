namespace JurisControl.Domain.Enums;

public enum EstadoJuicio
{
    Iniciado = 0,
    EnPruebas = 1,
    Alegatos = 2,
    Sentencia = 3,
    Apelacion = 4,
    Amparo = 5,
    Ejecucion = 6,
    Concluido = 7,
    Sobreseido = 8
}

public enum RolProcesal
{
    Actor = 0,
    Demandado = 1,
    Tercero = 2,
    Denunciante = 3,
    Imputado = 4,
    Quejoso = 5,
    AutoridadResponsable = 6
}

public enum TipoActuacion
{
    Acuerdo = 0,
    Notificacion = 1,
    Audiencia = 2,
    Escrito = 3,
    Sentencia = 4,
    Resolucion = 5,
    Diligencia = 6,
    Exhorto = 7,
    Otro = 99
}

public enum TipoPromocion
{
    Demanda = 0,
    Contestacion = 1,
    Recurso = 2,
    Ofrecimiento = 3,
    DesahogoPrueba = 4,
    Alegatos = 5,
    Amparo = 6,
    Otro = 99
}

public enum EstadoAudiencia
{
    Programada = 0,
    Diferida = 1,
    Celebrada = 2,
    Cancelada = 3
}

public enum EstadoPlazo
{
    Abierto = 0,
    Cumplido = 1,
    Vencido = 2,
    Suspendido = 3
}

public static class JuicioLabels
{
    public static string Label(this EstadoJuicio e) => e switch
    {
        EstadoJuicio.Iniciado => "Iniciado",
        EstadoJuicio.EnPruebas => "En pruebas",
        EstadoJuicio.Alegatos => "Alegatos",
        EstadoJuicio.Sentencia => "Sentencia",
        EstadoJuicio.Apelacion => "Apelación",
        EstadoJuicio.Amparo => "Amparo",
        EstadoJuicio.Ejecucion => "Ejecución",
        EstadoJuicio.Concluido => "Concluido",
        EstadoJuicio.Sobreseido => "Sobreseído",
        _ => e.ToString()
    };

    public static string BadgeClass(this EstadoJuicio e) => e switch
    {
        EstadoJuicio.Concluido or EstadoJuicio.Sobreseido => "badge-neutral",
        EstadoJuicio.Sentencia or EstadoJuicio.Ejecucion => "badge-success",
        EstadoJuicio.Apelacion or EstadoJuicio.Amparo => "badge-warning",
        _ => "badge-info"
    };

    public static string Label(this RolProcesal r) => r switch
    {
        RolProcesal.Actor => "Actor",
        RolProcesal.Demandado => "Demandado",
        RolProcesal.Tercero => "Tercero",
        RolProcesal.Denunciante => "Denunciante",
        RolProcesal.Imputado => "Imputado",
        RolProcesal.Quejoso => "Quejoso",
        RolProcesal.AutoridadResponsable => "Autoridad responsable",
        _ => r.ToString()
    };

    public static string Label(this TipoActuacion t) => t switch
    {
        TipoActuacion.Acuerdo => "Acuerdo",
        TipoActuacion.Notificacion => "Notificación",
        TipoActuacion.Audiencia => "Audiencia",
        TipoActuacion.Escrito => "Escrito",
        TipoActuacion.Sentencia => "Sentencia",
        TipoActuacion.Resolucion => "Resolución",
        TipoActuacion.Diligencia => "Diligencia",
        TipoActuacion.Exhorto => "Exhorto",
        TipoActuacion.Otro => "Otro",
        _ => t.ToString()
    };

    public static string Label(this TipoPromocion t) => t switch
    {
        TipoPromocion.Demanda => "Demanda",
        TipoPromocion.Contestacion => "Contestación",
        TipoPromocion.Recurso => "Recurso",
        TipoPromocion.Ofrecimiento => "Ofrecimiento de pruebas",
        TipoPromocion.DesahogoPrueba => "Desahogo de prueba",
        TipoPromocion.Alegatos => "Alegatos",
        TipoPromocion.Amparo => "Amparo",
        TipoPromocion.Otro => "Otro",
        _ => t.ToString()
    };

    public static string Label(this EstadoAudiencia e) => e switch
    {
        EstadoAudiencia.Programada => "Programada",
        EstadoAudiencia.Diferida => "Diferida",
        EstadoAudiencia.Celebrada => "Celebrada",
        EstadoAudiencia.Cancelada => "Cancelada",
        _ => e.ToString()
    };

    public static string BadgeClass(this EstadoAudiencia e) => e switch
    {
        EstadoAudiencia.Celebrada => "badge-success",
        EstadoAudiencia.Diferida => "badge-warning",
        EstadoAudiencia.Cancelada => "badge-danger",
        _ => "badge-info"
    };

    public static string Label(this EstadoPlazo e) => e switch
    {
        EstadoPlazo.Abierto => "Abierto",
        EstadoPlazo.Cumplido => "Cumplido",
        EstadoPlazo.Vencido => "Vencido",
        EstadoPlazo.Suspendido => "Suspendido",
        _ => e.ToString()
    };

    public static string BadgeClass(this EstadoPlazo e) => e switch
    {
        EstadoPlazo.Cumplido => "badge-success",
        EstadoPlazo.Vencido => "badge-danger",
        EstadoPlazo.Suspendido => "badge-warning",
        _ => "badge-info"
    };
}
