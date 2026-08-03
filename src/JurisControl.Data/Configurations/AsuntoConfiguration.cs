using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisControl.Data.Configurations;

public class AsuntoConfiguration : IEntityTypeConfiguration<Asunto>
{
    public void Configure(EntityTypeBuilder<Asunto> b)
    {
        b.ToTable("Asuntos");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho)
            .WithMany()
            .HasForeignKey(x => x.DespachoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Responsable)
            .WithMany()
            .HasForeignKey(x => x.ResponsableId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.Folio).HasMaxLength(30).IsRequired();
        b.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
        b.Property(x => x.MateriaKey).HasMaxLength(30).IsRequired();
        b.Property(x => x.Estado).HasConversion<int>();

        b.Property(x => x.Descripcion).HasMaxLength(4000);
        b.Property(x => x.NotasPrivadas).HasMaxLength(4000);
        b.Property(x => x.Etiquetas).HasMaxLength(500);

        b.Property(x => x.Cuantia).HasPrecision(18, 2);

        b.HasIndex(x => new { x.DespachoId, x.Folio }).IsUnique();
        b.HasIndex(x => new { x.DespachoId, x.Estado });
        b.HasIndex(x => new { x.DespachoId, x.ClienteId });
        b.HasIndex(x => new { x.DespachoId, x.ResponsableId });
        b.HasIndex(x => new { x.DespachoId, x.MateriaKey });
    }
}
