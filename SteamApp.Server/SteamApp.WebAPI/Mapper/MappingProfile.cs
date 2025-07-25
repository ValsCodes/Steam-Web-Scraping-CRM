﻿using AutoMapper;
using SteamApp.Models.DTOs.Item;
using SteamApp.Models.DTOs.Product;
using SteamApp.Models.Entities;

namespace SteamApp.WebAPI.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        CreateMap<Product, ProductDto>();

        CreateMap<ProductDto, Product>();

        CreateMap<CreateProductDto, Product>().ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<UpdateProductDto, Product>();

        CreateMap<UpdateProductDto, Product>().ReverseMap();     

        CreateMap<ItemDto, Item>();

        CreateMap<Item, ItemDto>();

        CreateMap<CreateItemDto, Item>().ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
