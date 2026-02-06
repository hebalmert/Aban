using Aban.Domain.EntitiesSoft;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aban.AppInfra.ModelConfig.EntitiesSoft
{
    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(e => e.ProductId);
            //Esta es una manera de garantizar un consecutivo en los GUIDs para este modelo NEWSEQUENTIALID
            builder.Property(x => x.ProductId).HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.HasIndex(e => new { e.CorporationId, e.ProductName }).IsUnique();
            builder.Property(e => e.Price).HasPrecision(18, 2);
            builder.Property(e => e.Quantity).HasPrecision(18, 2);
        }
    }
}
