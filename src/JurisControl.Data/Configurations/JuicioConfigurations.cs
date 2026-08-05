using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisControl.Data.Configurations;

public class JuicioConfiguration : IEntityTypeConfiguration<Juicio>
{
    public void Configure(EntityTypeBuilder<Juicio> b)
    {
        b.ToTable("Juicios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany()
            .HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Asunto).WithMany()
            .HasForeignKey(x => x.AsuntoId).OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.NumeroExpediente).HasMaxLength(50).IsRequired();
        b.Property(x => x.Juzgado).HasMaxLength(200).IsRequired();
        b.Property(x => x.TipoJuicio).HasMaxLength(150).IsRequired();
        b.Property(x => x.MateriaKey).HasMaxLength(30).IsRequired();
        b.Property(x => x.Estado).HasConversion<int>();
        b.Property(x => x.Descripcion).HasMaxLength(4000);
        b.Property(x => x.Observaciones).HasMaxLength(4000);
        b.Property(x => x.Cuantia).HasPrecision(18, 2);

        b.HasIndex(x => new { x.DespachoId, x.AsuntoId });
        b.HasIndex(x => new { x.DespachoId, x.NumeroExpediente });
        b.HasIndex(x => new { x.DespachoId, x.Estado });
    }
}

public class ParteJuicioConfiguration : IEntityTypeConfiguration<ParteJuicio>
{
    public void Configure(EntityTypeBuilder<ParteJuicio> b)
    {
        b.ToTable("PartesJuicio");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany().HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Juicio).WithMany().HasForeignKey(x => x.JuicioId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.Rol).HasConversion<int>();
        b.Property(x => x.NombreLibre).HasMaxLength(300);
        b.Property(x => x.Representante).HasMaxLength(200);
        b.Property(x => x.Notas).HasMaxLength(1000);

        b.HasIndex(x => new { x.DespachoId, x.JuicioId });
    }
}

public class ActuacionConfiguration : IEntityTypeConfiguration<Actuacion>
{
    public void Configure(EntityTypeBuilder<Actuacion> b)
    {
        b.ToTable("Actuaciones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany().HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Juicio).WithMany().HasForeignKey(x => x.JuicioId).OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.Tipo).HasConversion<int>();
        b.Property(x => x.Resumen).HasMaxLength(500).IsRequired();
        b.Property(x => x.Detalle).HasMaxLength(4000);

        b.HasIndex(x => new { x.DespachoId, x.JuicioId, x.Fecha });
    }
}

public class PromocionConfiguration : IEntityTypeConfiguration<Promocion>
{
    public void Configure(EntityTypeBuilder<Promocion> b)
    {
        b.ToTable("Promociones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany().HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Juicio).WithMany().HasForeignKey(x => x.JuicioId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Firmante).WithMany().HasForeignKey(x => x.FirmanteId).OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.Tipo).HasConversion<int>();
        b.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
        b.Property(x => x.Contenido).HasMaxLength(4000);
        b.Property(x => x.NumeroAcuse).HasMaxLength(50);

        b.HasIndex(x => new { x.DespachoId, x.JuicioId, x.FechaPresentacion });
    }
}

public class AudienciaConfiguration : IEntityTypeConfiguration<Audiencia>
{
    public void Configure(EntityTypeBuilder<Audiencia> b)
    {
        b.ToTable("Audiencias");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany().HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Juicio).WithMany().HasForeignKey(x => x.JuicioId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.AsignadoA).WithMany().HasForeignKey(x => x.AsignadoAId).OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.Tipo).HasMaxLength(150).IsRequired();
        b.Property(x => x.Lugar).HasMaxLength(300);
        b.Property(x => x.Estado).HasConversion<int>();
        b.Property(x => x.Resultado).HasMaxLength(2000);
        b.Property(x => x.Observaciones).HasMaxLength(2000);

        b.HasIndex(x => new { x.DespachoId, x.FechaHora });
        b.HasIndex(x => new { x.DespachoId, x.JuicioId });
    }
}

public class PlazoConfiguration : IEntityTypeConfiguration<Plazo>
{
    public void Configure(EntityTypeBuilder<Plazo> b)
    {
        b.ToTable("Plazos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany().HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Juicio).WithMany().HasForeignKey(x => x.JuicioId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Responsable).WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.Descripcion).HasMaxLength(300).IsRequired();
        b.Property(x => x.Estado).HasConversion<int>();
        b.Property(x => x.NotasCumplimiento).HasMaxLength(1000);

        b.HasIndex(x => new { x.DespachoId, x.FechaVencimiento });
        b.HasIndex(x => new { x.DespachoId, x.Estado });
    }
}
