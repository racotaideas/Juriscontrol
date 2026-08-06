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

    public JurisControlDbContext(
        DbContextOptions<JurisControlDbContext> options,
        ITenantContext tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    public DbSet<Despacho> Despachos => Set<Despacho>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Asunto> Asuntos => Set<Asunto>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<ContadorFolio> ContadoresFolio => Set<ContadorFolio>();
    public DbSet<Juicio> Juicios => Set<Juicio>();
    public DbSet<ParteJuicio> PartesJuicio => Set<ParteJuicio>();
    public DbSet<Actuacion> Actuaciones => Set<Actuacion>();
    public DbSet<Promocion> Promociones => Set<Promocion>();
    public DbSet<Audiencia> Audiencias => Set<Audiencia>();
    public DbSet<Plazo> Plazos => Set<Plazo>();
    public DbSet<Credito> Creditos => Set<Credito>();
    public DbSet<PagoCobranza> PagosCobranza => Set<PagoCobranza>();
    public DbSet<GestionCobranza> GestionesCobranza => Set<GestionCobranza>();
    public DbSet<Remate> Remates => Set<Remate>();
    public DbSet<Plantilla> Plantillas => Set<Plantilla>();
    public DbSet<Gasto> Gastos => Set<Gasto>();

    private bool CurrentIsPlatformScope() => _tenant.IsPlatformScope;

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
        // Guid.Empty nunca matchea un despacho real (los IDs se generan con NEWSEQUENTIALID).
        // El coalesce evita NRE cuando no hay tenant en scope y no estamos en plataforma:
        // en ese caso el filtro descarta todo, que es el comportamiento seguro.
        Expression<Func<TEntity, bool>> filter = e =>
            _tenant.IsPlatformScope
            || e.DespachoId == (_tenant.DespachoId ?? Guid.Empty);
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
