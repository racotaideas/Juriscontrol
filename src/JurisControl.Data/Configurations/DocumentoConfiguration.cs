using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisControl.Data.Configurations;

public class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> b)
    {
        b.ToTable("Documentos");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany()
            .HasForeignKey(x => x.DespachoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Cliente).WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Asunto).WithMany()
            .HasForeignKey(x => x.AsuntoId)
            .OnDelete(DeleteBehavior.Cascade);

        // NoAction en lugar de SetNull para evitar SQL Server error 1785
        // ("multiple cascade paths"): Documentos ya cascadea con Cliente/Asunto
        // y Actuacion/Promocion también llegan a Cliente por otro camino.
        // Con NoAction se corta el ciclo; borrar una Actuacion/Promocion no
        // toca la fila de Documentos (queda con FK huérfana, no visible en UI).
        b.HasOne(x => x.Actuacion).WithMany()
            .HasForeignKey(x => x.ActuacionId)
            .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.Promocion).WithMany()
            .HasForeignKey(x => x.PromocionId)
            .OnDelete(DeleteBehavior.NoAction);

        b.Property(x => x.Nombre).HasMaxLength(300).IsRequired();
        b.Property(x => x.Categoria).HasMaxLength(50).IsRequired();
        b.Property(x => x.StorageRef).HasMaxLength(500).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(120);
        b.Property(x => x.Notas).HasMaxLength(1000);

        b.HasIndex(x => new { x.DespachoId, x.AsuntoId });
        b.HasIndex(x => new { x.DespachoId, x.ClienteId });
        b.HasIndex(x => new { x.DespachoId, x.ActuacionId });
        b.HasIndex(x => new { x.DespachoId, x.PromocionId });
    }
}
