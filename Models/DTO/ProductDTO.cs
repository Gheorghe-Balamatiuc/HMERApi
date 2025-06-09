using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMERApi.Models.DTO;

public class ProductDTO
{
    [Required]
    public IFormFile? ImageFile { get; set; }
}

public class ProductNoIdDTO
{
    [Required]
    [MaxLength(50)]
    public string? Image { get; set; }
    [Required]
    [MaxLength(8000)]
    public string? ImagePrediction { get; set; }
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string? PredictionDescription { get; set; }
}