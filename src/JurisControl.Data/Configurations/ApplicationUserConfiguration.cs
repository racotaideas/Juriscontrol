using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisControl.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        // Identity ya define ToTable("AspNetUsers"); dejamos el nombre estándar por ahora.
        b.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
        b.Property(x => x.WhatsApp).HasMaxLength(20);
        b.Property(x => x.CedulaProfesional).HasMaxLength(20);
        b.Property(x => x.Especialidad).HasMaxLength(200);

        b.HasOne(x => x.Despacho)
            .WithMany()
            .HasForeignKey(x => x.DespachoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.DespachoId, x.Activo });
    }
}
