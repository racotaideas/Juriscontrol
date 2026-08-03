using JurisControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisControl.Data.Configurations;

public class ContadorFolioConfiguration : IEntityTypeConfiguration<ContadorFolio>
{
    public void Configure(EntityTypeBuilder<ContadorFolio> b)
    {
        b.ToTable("ContadoresFolio");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        b.HasIndex(x => new { x.DespachoId, x.Anio }).IsUnique();
    }
}
