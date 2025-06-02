using AutoMapper;
using HMERApi.Models;
using HMERApi.Models.DTO;

namespace HMERApi.AutoMapper;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<Product, ProductNoIdDTO>();
    }
}