using AutoMapper;
using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.ObjectType;
using EtheriT.Coker.Application.Shared.Dto.EnterAd;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
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
            CreateMap<GetFrontContenOutputDto, WebMenu>()
                .ReverseMap()
                .ForMember(e => e.Html, option => option.MapFrom(c => c.Html??""));
            CreateMap<UpdateSerNoDto, WebMenu>().ReverseMap();
            CreateMap<MenuSaveContenDto, MenuContenDto>()
                .ForMember(e => e.Html, option => option.MapFrom(c => c.SaveHtml))
                .ForMember(e => e.Css, option => option.MapFrom(c => c.SaveCss))
                .ReverseMap();

            //Html_Content
            CreateMap<HtmlContentDto, Html_Content>()
                .ReverseMap()
                .ForMember(e => e.TypeName, option => option.MapFrom(c => ((ObjectTypeEnum)c.Type).ToString()));
            CreateMap<ObjectTypeItemDto, Html_Content>()
                .ReverseMap()
                .ForMember(e => e.FK_TopNodeId, option => option.MapFrom(c => c.Type))
                .ForMember(e => e.CanAdd, option => option.MapFrom(c => false))
                .ForMember(e => e.MinLevel, option => option.MapFrom(c => 1));
            CreateMap<UpdateSerNoDto, Html_Content>()
                .ForMember(e => e.Ser_no, option => option.MapFrom(c => c.SerNO))
                .ForMember(e => e.Type, option => option.MapFrom(c => c.FK_TopNodeId))
                .ReverseMap();
            CreateMap<HtmlContentDetailDto, Html_Content>().ReverseMap();


            //ObjectType
            CreateMap<UpdateSerNoDto, ObjectType>()
                .ReverseMap();
            CreateMap<ObjectTypeItemDto, ObjectType>()
                .ReverseMap()
                .ForMember(e => e.FK_TopNodeId, option => option.MapFrom(c => 0))
                .ForMember(e => e.CanEdit, option => option.MapFrom(c => false))
                .ForMember(e => e.CanDel, option => option.MapFrom(c => false))
                .ForMember(e => e.CanAdd, option => option.MapFrom(c => false))
                .ForMember(e => e.MaxLevel, option => option.MapFrom(c => 0));
        }
    }
}
