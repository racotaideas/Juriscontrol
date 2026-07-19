using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JurisControl.Data.TenantContext;

/// <summary>
/// Antes de cada comando SQL, fija el SESSION_CONTEXT con el TenantId del usuario actual.
/// Ese contexto es el que consume la política de Row Level Security del SQL Server
/// (ver <c>001_RowLevelSecurity.sql</c>). Si el ITenantContext dice que estamos en modo
/// plataforma, se prende el flag PlatformScope y se saltan los filtros.
/// </summary>
public sealed class TenantSessionInterceptor : DbCommandInterceptor
{
    private readonly ITenantContext _tenant;

    public TenantSessionInterceptor(ITenantContext tenant)
    {
        _tenant = tenant;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        StampSessionContext(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        StampSessionContext(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        StampSessionContext(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampSessionContext(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        StampSessionContext(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        StampSessionContext(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void StampSessionContext(DbCommand command)
    {
        // Comando compuesto: primero fija SESSION_CONTEXT, luego ejecuta el SQL original.
        // Los sp_set_session_context son idempotentes por comando.
        var platform = _tenant.IsPlatformScope ? "1" : "0";
        var tenantSql = _tenant.DespachoId is Guid id
            ? $"EXEC sp_set_session_context @key = N'TenantId', @value = '{id}'; "
            : "EXEC sp_set_session_context @key = N'TenantId', @value = NULL; ";
        var platformSql = $"EXEC sp_set_session_context @key = N'PlatformScope', @value = {platform}; ";

        command.CommandText = tenantSql + platformSql + command.CommandText;
    }
}
