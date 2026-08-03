namespace JurisControl.Data.TenantContext;

/// <summary>
/// Servicio scoped que expone el <c>DespachoId</c> del usuario autenticado a cualquier
/// capa (repositorios, filtros globales de EF Core, interceptores). Nunca acepta el
/// ID desde parámetros de request — siempre lo toma del ClaimsPrincipal.
/// </summary>
public interface ITenantContext
{
    /// <summary>DespachoId del usuario actual, o <c>null</c> si aún no está autenticado.</summary>
    Guid? DespachoId { get; }

    /// <summary>True cuando la operación corre bajo un contexto de plataforma (jobs,
    /// migraciones, seed) donde no hay tenant. Solo permitido en el rol PlatformAdmin.</summary>
    bool IsPlatformScope { get; }

    /// <summary>
    /// Activa temporalmente el modo plataforma en este scope. Devuelve un IDisposable
    /// que restaura el estado anterior al Dispose. Úsalo solo desde el DbSeeder,
    /// migraciones o jobs administrativos. Afecta tanto al query filter global
    /// (EF Core) como al SESSION_CONTEXT que consume la RLS de SQL Server.
    /// </summary>
    IDisposable EnterPlatformScope();
}
