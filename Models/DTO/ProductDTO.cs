using System.ComponentModel.DataAnnotations;

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
}