using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMERApi.Models.DTO;

/// <summary>
/// Data Transfer Object for creating or updating products
/// Contains the image file to be processed
/// </summary>
public class ProductDTO
{
    /// <summary>
    /// The image file containing a handwritten math expression
    /// </summary>
    [Required]
    public IFormFile? ImageFile { get; set; }
}

/// <summary>
/// Data Transfer Object for returning product data without the ID
/// Used for API responses
/// </summary>
public class ProductNoIdDTO
{
    /// <summary>
    /// The filename of the uploaded image
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string? Image { get; set; }
    
    /// <summary>
    /// The LaTeX prediction extracted from the image
    /// </summary>
    [Required]
    [MaxLength(8000)]
    public string? ImagePrediction { get; set; }
    
    /// <summary>
    /// The natural language description of the math expression
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string? PredictionDescription { get; set; }
}