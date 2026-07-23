using AutoMapper;
using ProductManagement.Application.DTOs;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Mappings
{
    /// <summary>
    /// AutoMapper Profile containing entity-to-DTO mappings.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<Product, ProductSummaryDto>();

            CreateMap<Item, ItemDto>().ReverseMap();
        }
    }
}
