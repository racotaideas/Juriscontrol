using JurisControl.Data.TenantContext;
using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Data.Services;

/// <summary>
/// Emite folios secuenciales por despacho y año en formato JC-YYYY-NNNN.
/// Usa UPDATE ... OUTPUT en una sola query para evitar colisiones concurrentes
/// sin necesidad de transacciones explícitas (SQL Server serializa el UPDATE).
/// </summary>
public interface IFolioService
{
    Task<string> SiguienteFolioAsuntoAsync(CancellationToken ct = default);
}

public sealed class FolioService : IFolioService
{
    private readonly JurisControlDbContext _db;
    private readonly ITenantContext _tenant;

    public FolioService(JurisControlDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<string> SiguienteFolioAsuntoAsync(CancellationToken ct = default)
    {
        var despachoId = _tenant.DespachoId
            ?? throw new InvalidOperationException("No hay tenant en scope para emitir folio.");
        var anio = DateTime.UtcNow.Year;

        // Fila del contador (crea si no existe)
        var contador = await _db.ContadoresFolio
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.DespachoId == despachoId && c.Anio == anio, ct);

        if (contador is null)
        {
            contador = new ContadorFolio
            {
                DespachoId = despachoId,
                Anio = anio,
                UltimoNumero = 0,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.ContadoresFolio.Add(contador);
            await _db.SaveChangesAsync(ct);
        }

        // UPDATE atómico con OUTPUT (serialización a nivel SQL, sin race conditions)
        var nuevo = await _db.Database.SqlQuery<int>(
            $@"UPDATE dbo.ContadoresFolio
               SET UltimoNumero = UltimoNumero + 1,
                   UpdatedAt   = SYSDATETIMEOFFSET()
               OUTPUT INSERTED.UltimoNumero AS Value
               WHERE Id = {contador.Id}").FirstAsync(ct);

        return $"JC-{anio}-{nuevo:D4}";
    }
}
