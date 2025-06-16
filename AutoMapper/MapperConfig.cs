using AutoMapper;
using HMERApi.Models;
using HMERApi.Models.DTO;

namespace HMERApi.AutoMapper;

/// <summary>
/// AutoMapper configuration profile for mapping between entities and DTOs
/// </summary>
public class MapperConfig : Profile
{
    /// <summary>
    /// Constructor that configures the mapping profiles
    /// </summary>
    public MapperConfig()
    {
        // Map from Product entity to ProductNoIdDTO for API responses
        CreateMap<Product, ProductNoIdDTO>();
    }
}