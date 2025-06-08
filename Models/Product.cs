using HMERApi.FluentConfigs;
using Microsoft.EntityFrameworkCore;

namespace HMERApi.Models;

[EntityTypeConfiguration(typeof(ProductConfig))]
public class Product
{
    public int Id { get; set; }
    public string Image { get; set; } = null!;
    public string ImagePrediction { get; set; } = null!;
}