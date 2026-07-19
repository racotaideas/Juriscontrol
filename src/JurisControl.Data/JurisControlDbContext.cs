using System.Linq.Expressions;
using JurisControl.Data.TenantContext;
using JurisControl.Domain.Common;
using JurisControl.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JurisControl.Data;

/// <summary>
/// Contexto principal. Combina ASP.NET Core Identity con las entidades de negocio,
/// y aplica los Global Query Filters de multi-tenant en <see cref="OnModelCreating"/>.
/// </summary>
public class JurisControlDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private readonly ITenantContext _tenant;
    private bool _forcePlatformScope;

    public JurisControlDbContext(
        DbContextOptions<JurisControlDbContext> options,
        ITenantContext tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    public DbSet<Despacho> Despachos => Set<Despacho>();
    public DbSet<Cliente> Clientes => Set<Cliente>();

    /// <summary>
    /// Encendido: el DbContext ignora el ITenantContext y trata como si estuviera en modo plataforma.
    /// Solo debe usarse desde el DbSeeder o desde migraciones. No exponer a request-handlers.
    /// </summary>
    public void SetPlatformScope() => _forcePlatformScope = true;

    private bool CurrentIsPlatformScope() => _forcePlatformScope || _tenant.IsPlatformScope;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones fluent por convención (asambly-scan de las clases IEntityTypeConfiguration<>)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JurisControlDbContext).Assembly);

        // Global Query Filter: cualquier entidad ITenantEntity queda auto-filtrada por DespachoId
        // salvo que la operación corra en modo plataforma (jobs, seed, migraciones).
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(JurisControlDbContext)
                    .GetMethod(nameof(ApplyTenantFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        Expression<Func<TEntity, bool>> filter = e =>
            _forcePlatformScope
            || _tenant.IsPlatformScope
            || e.DespachoId == _tenant.DespachoId!.Value;
        modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
    }

    /// <summary>
    /// Al guardar, si la entidad es <see cref="ITenantEntity"/> y no tiene DespachoId,
    /// se le asigna el del tenant actual. Falla si no hay tenant y no estamos en plataforma —
    /// prevenimos el bug clásico de guardar sin tenant y contaminar la base.
    /// </summary>
    public override int SaveChanges()
    {
        StampTenantOnAdd();
        StampAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTenantOnAdd();
        StampAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampTenantOnAdd()
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.DespachoId == Guid.Empty)
            {
                if (_tenant.DespachoId is null && !CurrentIsPlatformScope())
                    throw new InvalidOperationException(
                        $"Se intentó insertar {entry.Entity.GetType().Name} sin DespachoId " +
                        "fuera de contexto de plataforma. Multi-tenant violation.");
                if (_tenant.DespachoId is Guid id)
                    entry.Entity.DespachoId = id;
            }
        }
    }

    private void StampAuditFields()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.CreatedAt == default)
                entry.Entity.CreatedAt = now;
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }
}
