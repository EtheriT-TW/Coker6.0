using AutoMapper;
using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.EnterAd;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public class CustomDtoMapper : Profile
    {
        // Mapping
        // 第一個參數是來源，第二個參數是目標
        public CustomDtoMapper() {
            //Users
            CreateMap<UserDto, User>()
                .ForMember(e => e.Name, option => option.MapFrom(c => c.UserName))
                .ReverseMap();

            //WebMenu
            CreateMap<SiteMapDto, WebMenu>().ReverseMap();
            CreateMap<MenuItemDto, WebMenu>().ReverseMap();
            CreateMap<MenuContenDto, WebMenu>().ReverseMap();
            CreateMap<MenuSaveContenDto, WebMenu>().ReverseMap();
            CreateMap<UpdateSerNoDto, WebMenu>().ReverseMap();
            CreateMap<MenuSaveContenDto, MenuContenDto>()
                .ForMember(e => e.Html, option => option.MapFrom(c => c.SaveHtml))
                .ForMember(e => e.Css, option => option.MapFrom(c => c.SaveCss))
                .ReverseMap();

            //Html_Content
            CreateMap<HtmlContentDto, Html_Content>()
                .ReverseMap()
                .ForMember(e => e.TypeName, option => option.MapFrom(c => ((ObjectTypeEnum)c.Type).ToString()));
        }
    }
}
