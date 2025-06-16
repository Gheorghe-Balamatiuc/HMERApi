using HMERApi.FluentConfigs;
using Microsoft.EntityFrameworkCore;

namespace HMERApi.Models;

/// <summary>
/// Represents a product entity that contains processed handwritten math expressions
/// </summary>
[EntityTypeConfiguration(typeof(ProductConfig))]
public class Product
{
    /// <summary>
    /// Primary key for the product
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The filename of the uploaded image
    /// </summary>
    public string Image { get; set; } = null!;
    
    /// <summary>
    /// The LaTeX prediction extracted from the image
    /// </summary>
    public string ImagePrediction { get; set; } = null!;

    /// <summary>
    /// The natural language description of the math expression
    /// </summary>
    public string PredictionDescription { get; set; } = null!;
}