using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisControl.Data.Configurations;

public class CreditoConfiguration : IEntityTypeConfiguration<Credito>
{
    public void Configure(EntityTypeBuilder<Credito> b)
    {
        b.ToTable("Creditos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany().HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Asunto).WithMany().HasForeignKey(x => x.AsuntoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.DeudorCliente).WithMany().HasForeignKey(x => x.DeudorClienteId).OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.NumeroCredito).HasMaxLength(80).IsRequired();
        b.Property(x => x.Acreedor).HasMaxLength(200).IsRequired();
        b.Property(x => x.NombreDeudor).HasMaxLength(300);
        b.Property(x => x.Tipo).HasConversion<int>();
        b.Property(x => x.Estado).HasConversion<int>();
        b.Property(x => x.MontoOriginal).HasPrecision(18, 2);
        b.Property(x => x.SaldoActual).HasPrecision(18, 2);
        b.Property(x => x.TasaInteres).HasPrecision(9, 4);
        b.Property(x => x.Garantia).HasMaxLength(1000);
        b.Property(x => x.Observaciones).HasMaxLength(2000);

        b.HasIndex(x => new { x.DespachoId, x.NumeroCredito });
        b.HasIndex(x => new { x.DespachoId, x.Estado });
        b.HasIndex(x => new { x.DespachoId, x.Acreedor });
    }
}

public class PagoCobranzaConfiguration : IEntityTypeConfiguration<PagoCobranza>
{
    public void Configure(EntityTypeBuilder<PagoCobranza> b)
    {
        b.ToTable("PagosCobranza");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany().HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Credito).WithMany().HasForeignKey(x => x.CreditoId).OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.Monto).HasPrecision(18, 2);
        b.Property(x => x.AplicadoCapital).HasPrecision(18, 2);
        b.Property(x => x.AplicadoInteres).HasPrecision(18, 2);
        b.Property(x => x.AplicadoGastos).HasPrecision(18, 2);
        b.Property(x => x.MedioPago).HasMaxLength(50);
        b.Property(x => x.Referencia).HasMaxLength(100);
        b.Property(x => x.Notas).HasMaxLength(1000);

        b.HasIndex(x => new { x.DespachoId, x.CreditoId, x.Fecha });
    }
}

public class GestionCobranzaConfiguration : IEntityTypeConfiguration<GestionCobranza>
{
    public void Configure(EntityTypeBuilder<GestionCobranza> b)
    {
        b.ToTable("GestionesCobranza");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany().HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Credito).WithMany().HasForeignKey(x => x.CreditoId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Gestor).WithMany().HasForeignKey(x => x.GestorId).OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.Canal).HasMaxLength(30);
        b.Property(x => x.Resultado).HasConversion<int>();
        b.Property(x => x.PersonaContactada).HasMaxLength(200);
        b.Property(x => x.Descripcion).HasMaxLength(2000).IsRequired();
        b.Property(x => x.PromesaMonto).HasPrecision(18, 2);

        b.HasIndex(x => new { x.DespachoId, x.CreditoId, x.Fecha });
    }
}

public class RemateConfiguration : IEntityTypeConfiguration<Remate>
{
    public void Configure(EntityTypeBuilder<Remate> b)
    {
        b.ToTable("Remates");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasOne(x => x.Despacho).WithMany().HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Credito).WithMany().HasForeignKey(x => x.CreditoId).OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.Lugar).HasMaxLength(300);
        b.Property(x => x.Postor).HasMaxLength(300);
        b.Property(x => x.Estado).HasConversion<int>();
        b.Property(x => x.ValorAvaluoBase).HasPrecision(18, 2);
        b.Property(x => x.PosturaLegal).HasPrecision(18, 2);
        b.Property(x => x.MontoFincado).HasPrecision(18, 2);
        b.Property(x => x.Observaciones).HasMaxLength(2000);

        b.HasIndex(x => new { x.DespachoId, x.CreditoId, x.Almoneda });
        b.HasIndex(x => new { x.DespachoId, x.FechaHora });
    }
}
