using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisControl.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Clientes");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho)
            .WithMany()
            .HasForeignKey(x => x.DespachoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.Tipo).HasConversion<int>();

        b.Property(x => x.Nombre).HasMaxLength(100);
        b.Property(x => x.ApellidoPaterno).HasMaxLength(100);
        b.Property(x => x.ApellidoMaterno).HasMaxLength(100);
        b.Property(x => x.Curp).HasMaxLength(18);
        b.Property(x => x.Rfc).HasMaxLength(13);

        b.Property(x => x.RazonSocial).HasMaxLength(200);
        b.Property(x => x.NombreComercial).HasMaxLength(200);
        b.Property(x => x.RepresentanteLegal).HasMaxLength(200);

        b.Property(x => x.CorreoPrincipal).HasMaxLength(200);
        b.Property(x => x.TelefonoPrincipal).HasMaxLength(20);
        b.Property(x => x.WhatsApp).HasMaxLength(20);
        b.Property(x => x.Direccion).HasMaxLength(300);
        b.Property(x => x.Ciudad).HasMaxLength(100);
        b.Property(x => x.Estado).HasMaxLength(100);
        b.Property(x => x.CodigoPostal).HasMaxLength(10);

        b.Property(x => x.Etiquetas).HasMaxLength(500);

        // Ignorar propiedad calculada
        b.Ignore(x => x.DisplayName);

        // Índices multi-tenant compuestos: (DespachoId, X) - así el RLS + query filter aprovechan
        b.HasIndex(x => new { x.DespachoId, x.Rfc })
            .HasFilter("[Rfc] IS NOT NULL");
        b.HasIndex(x => new { x.DespachoId, x.CorreoPrincipal });
        b.HasIndex(x => new { x.DespachoId, x.Activo });
    }
}
