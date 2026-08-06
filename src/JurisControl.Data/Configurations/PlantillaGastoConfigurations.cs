using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisControl.Data.Configurations;

public class PlantillaConfiguration : IEntityTypeConfiguration<Plantilla>
{
    public void Configure(EntityTypeBuilder<Plantilla> b)
    {
        b.ToTable("Plantillas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany()
            .HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.Clave).HasMaxLength(50).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Categoria).HasMaxLength(50).IsRequired();
        b.Property(x => x.Cuerpo).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(1000);

        b.HasIndex(x => new { x.DespachoId, x.Clave }).IsUnique();
        b.HasIndex(x => new { x.DespachoId, x.Categoria });
        b.HasIndex(x => new { x.DespachoId, x.Activa });
    }
}

public class GastoConfiguration : IEntityTypeConfiguration<Gasto>
{
    public void Configure(EntityTypeBuilder<Gasto> b)
    {
        b.ToTable("Gastos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany()
            .HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Juicio).WithMany()
            .HasForeignKey(x => x.JuicioId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Asunto).WithMany()
            .HasForeignKey(x => x.AsuntoId).OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.Categoria).HasMaxLength(50).IsRequired();
        b.Property(x => x.Concepto).HasMaxLength(300).IsRequired();
        b.Property(x => x.Monto).HasPrecision(18, 2);
        b.Property(x => x.Estado).HasMaxLength(30);
        b.Property(x => x.Comprobante).HasMaxLength(200);
        b.Property(x => x.Notas).HasMaxLength(1000);

        b.HasIndex(x => new { x.DespachoId, x.JuicioId, x.Fecha });
        b.HasIndex(x => new { x.DespachoId, x.AsuntoId });
    }
}
