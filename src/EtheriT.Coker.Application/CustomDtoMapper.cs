using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public class CustomDtoMapper : Profile
    {
        public CustomDtoMapper() {
            CreateMap<SiteMapDto, WebMenu>().ReverseMap();
            CreateMap<MenuItemDto, WebMenu>().ReverseMap();
            CreateMap<MenuContenDto, WebMenu>().ReverseMap();
            CreateMap<MenuSaveContenDto, WebMenu>().ReverseMap();
        }
    }
}
