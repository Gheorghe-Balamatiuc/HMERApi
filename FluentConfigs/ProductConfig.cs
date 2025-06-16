using HMERApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMERApi.FluentConfigs;

/// <summary>
/// Entity Framework Core Fluent API configuration for the Product entity
/// </summary>
class ProductConfig : IEntityTypeConfiguration<Product>
{
    /// <summary>
    /// Configures the entity properties and relationships
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Configure primary key
        builder.HasKey(p => p.Id);
        
        // Configure property constraints
        builder.Property(p => p.Image).IsRequired().HasMaxLength(50);
        builder.Property(p => p.ImagePrediction).IsRequired().HasMaxLength(8000);
        builder.Property(p => p.PredictionDescription).IsRequired().HasColumnType("nvarchar(max)");
    }
}