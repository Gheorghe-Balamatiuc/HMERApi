using HMERApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMERApi.FluentConfigs;

class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Image).IsRequired().HasMaxLength(50);
        builder.Property(p => p.ImagePrediction).IsRequired().HasMaxLength(8000);
    }
}