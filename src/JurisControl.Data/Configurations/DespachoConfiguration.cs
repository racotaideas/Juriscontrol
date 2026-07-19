using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisControl.Data.Configurations;

public class DespachoConfiguration : IEntityTypeConfiguration<Despacho>
{
    public void Configure(EntityTypeBuilder<Despacho> b)
    {
        b.ToTable("Despachos");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.Property(x => x.RazonSocial).HasMaxLength(200).IsRequired();
        b.Property(x => x.NombreComercial).HasMaxLength(200);
        b.Property(x => x.Rfc).HasMaxLength(13);

        b.Property(x => x.CorreoInstitucional).HasMaxLength(200);
        b.Property(x => x.WhatsApp).HasMaxLength(20);
        b.Property(x => x.SitioWeb).HasMaxLength(200);

        b.Property(x => x.ZonaHoraria).HasMaxLength(50);
        b.Property(x => x.MateriasAtiende).HasMaxLength(500);

        b.Property(x => x.Estado_)
            .HasColumnName("Estado")
            .HasConversion<int>();

        b.HasIndex(x => x.Rfc).IsUnique().HasFilter("[Rfc] IS NOT NULL");
    }
}
