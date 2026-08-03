namespace JurisControl.Domain.Enums;

public enum EstadoAsunto
{
    Recibido = 0,
    Asignado = 1,
    Activo = 2,
    EnEspera = 3,
    Cerrado = 4,
    Cancelado = 5
}

public static class EstadoAsuntoLabels
{
    public static string Label(this EstadoAsunto e) => e switch
    {
        EstadoAsunto.Recibido => "Recibido",
        EstadoAsunto.Asignado => "Asignado",
        EstadoAsunto.Activo => "Activo",
        EstadoAsunto.EnEspera => "En espera",
        EstadoAsunto.Cerrado => "Cerrado",
        EstadoAsunto.Cancelado => "Cancelado",
        _ => e.ToString()
    };

    public static string BadgeClass(this EstadoAsunto e) => e switch
    {
        EstadoAsunto.Recibido => "badge-info",
        EstadoAsunto.Asignado => "badge-info",
        EstadoAsunto.Activo => "badge-success",
        EstadoAsunto.EnEspera => "badge-warning",
        EstadoAsunto.Cerrado => "badge-neutral",
        EstadoAsunto.Cancelado => "badge-danger",
        _ => "badge-neutral"
    };
}
