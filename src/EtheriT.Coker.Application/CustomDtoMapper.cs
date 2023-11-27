using AutoMapper;
using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.ObjectType;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Member;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Application.Company;
using System.Security.AccessControl;
using EtheriT.Coker.Application.Dto.AuditLog;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Dto.Newsletter;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.Dto.Permissions;

namespace EtheriT.Coker.Application
{
	public class CustomDtoMapper : Profile
	{
		// Mapping
		// 第一個參數是來源，第二個參數是目標
		public CustomDtoMapper()
		{
			//Users
			CreateMap<UserSimplifyDto, User>()
				.ForMember(e => e.Name, option => option.MapFrom(c => c.UserName))
				.ReverseMap();
			CreateMap<MemberGetAllDataDto, User>()
				.ReverseMap();
			CreateMap<MemberUpdateDto, User>()
				.ReverseMap();
			CreateMap<ManagerAllListDto, User>()
				.ReverseMap();
            CreateMap<EditUserDto, User>()
                .ReverseMap();

            //Website
            CreateMap<Website, WebsiteEditDto>()
                .ReverseMap();

            //Company
            CreateMap<Core.Models.Company, CompanyDto>()
                .ReverseMap();

			//AuditLog
			CreateMap<Core.Models.AuditLog, AuditLogListDto>()
				.ReverseMap();

			//WebMenu
			CreateMap<SiteMapDto, WebMenu>().ReverseMap();
			CreateMap<MenuItemDto, WebMenu>().ReverseMap();
			CreateMap<MenuContenDto, WebMenu>().ReverseMap();
			CreateMap<MenuSaveContenDto, WebMenu>().ReverseMap();
			CreateMap<GetFrontContenOutputDto, WebMenu>()
				.ReverseMap()
				.ForMember(e => e.Html, option => option.MapFrom(c => c.Html ?? ""));
			CreateMap<UpdateSerNoDto, WebMenu>().ReverseMap();
			CreateMap<MenuSaveContenDto, MenuContenDto>()
				.ForMember(e => e.Html, option => option.MapFrom(c => c.SaveHtml))
				.ForMember(e => e.Css, option => option.MapFrom(c => c.SaveCss))
				.ReverseMap();
			CreateMap<WebMenu, MenuGetAllListDto>()
				.ForMember(e => e.Link, option => option.MapFrom(c => c.RouterName))
				.ReverseMap();
			CreateMap<MenuItemDto, MenuGetAllListDto>()
				.ForMember(e => e.Link, option => option.MapFrom(c => c.RouterName))
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

			//Product
			CreateMap<ProductImportDto, Prod>()
				.ForMember(e => e.Description, option => option.MapFrom(c => c.Description ?? ""))
				.ForMember(e => e.Introduction, option => option.MapFrom(c => c.Introduction ?? ""))
				.ForMember(e => e.Ser_No, option => option.MapFrom(c => 500))
				.ForMember(e => e.permanent, option => option.MapFrom(c => true))
				.ReverseMap();
			CreateMap<ProductImportUpateDto, Prod>()
				.ForMember(e => e.Description, option => option.MapFrom(c => c.Description ?? ""))
				.ForMember(e => e.Introduction, option => option.MapFrom(c => c.Introduction ?? ""))
				.ForMember(e => e.Ser_No, option => option.MapFrom(c => 500))
				.ForMember(e => e.permanent, option => option.MapFrom(c => true))
				.ReverseMap();
			CreateMap<ProdAddUpDto, Prod>()
				.ReverseMap();
			CreateMap<ProductImportDto, ProdAddUpDto>()
				.ForMember(e => e.Id, option => option.MapFrom(c => 0))
				.ForMember(e => e.Ser_No, option => option.MapFrom(c => 500))
				.ForMember(e => e.Permanent, option => option.MapFrom(c => true))
				.ReverseMap();
			CreateMap<ProdGetDataDto, Prod>().ReverseMap();
			CreateMap<ProdGetDataDto, DirectoryReleInfoDto>()
				.ForMember(e => e.Description, option => option.MapFrom(c => c.Introduction))
				.ReverseMap();
			CreateMap<ProductStockDto, ProductImportDto>()
				.ReverseMap()
				.ForMember(e => e.S1_Name, option => option.MapFrom(c => c.Spec1Name))
				.ForMember(e => e.S2_Name, option => option.MapFrom(c => c.Spec2Name))
				.ForMember(e => e.S1_Title, option => option.MapFrom(c => c.Spec1))
				.ForMember(e => e.S2_Title, option => option.MapFrom(c => c.Spec2))
				.ForMember(e => e.Prices, option => option.MapFrom(c => new List<ProductPriceDto> { new ProductPriceDto { Price = c.Price, FK_RId = 1 } }));

			CreateMap<Prod_Stock, ProductStockDto>()
				.ReverseMap()
				.ForMember(e => e.Alert_Qty, option => option.MapFrom(c => (c.Stock ?? 0) / 10))
				.ForMember(e => e.Min_Qty, option => option.MapFrom(c => 1))
				.ForMember(e => e.Ser_No, option => option.MapFrom(c => 500));

			CreateMap<ProductImportDto, ProductImportUpateDto>()
				.ReverseMap();
			CreateMap<Prod_Price, ProductPriceDto>()
				.ReverseMap();
			CreateMap<TechCertDto, TechCertImportDto>()
				.ForMember(e => e.Image1, option => option.MapFrom(c => c.Img))
				.ReverseMap();
			

			//Tags
			CreateMap<TagSelectedDto, Core.Models.Tag>()
				.ForMember(e => e.Id, option => option.MapFrom(c => c.FK_TId))
				.ReverseMap();

			//Article
			CreateMap<ArticleDto, Core.Models.Article>()
				.ReverseMap();
            CreateMap<ArticleGetDataDto, Core.Models.Article>()
                .ReverseMap()
                .ForMember(e => e.NodeDate, option => option.MapFrom(c => c.NodeDate==null?null: c.NodeDate.Value.ToString("yyyy/MM/dd")));
			CreateMap<ArticleGetDataDto, DirectoryReleInfoDto>().ReverseMap();
            CreateMap<ArticleListGetDto, DirectoryReleInfoDto>().ReverseMap();
            CreateMap<ArticleListGetDto, Core.Models.Article>()
                .ReverseMap()
                .ForMember(e => e.NodeDate, option => option.MapFrom(c => c.NodeDate == null ? null : c.NodeDate.Value.ToString("yyyy/MM/dd")));

            //Directory
            CreateMap<DirectoryAddUpDto, Core.Models.Directory>().ReverseMap();
			CreateMap<ProdGetDataDto, DirectoryReleInfoDto>()
				.ForMember(e => e.Description, option => option.MapFrom(c => c.Introduction))
				.ReverseMap();

			//StoreSet
			CreateMap<StoreSetOutputDto, Core.Models.StoreSet>()
                .ForMember(e => e.type, option => option.MapFrom(c => c.type))
				.ReverseMap();
			CreateMap<StoreSetDetailOutputDto, Core.Models.StoreSetDetail>()
                .ForMember(e => e.IsDeleted, option => option.MapFrom(c => false))
                .ForMember(e => e.CreationTime, option => option.MapFrom(c => DateTime.Now))
                .ReverseMap()
				.ForMember(e => e.key, option => option.MapFrom(c => c.StoreSet == null ? "" : c.StoreSet.key));
            
            CreateMap<StoreSetGroupOutputDto, StoreSetGroup>().ReverseMap();

            //CustSearch
            CreateMap<CuseSearchListDto, CustSearch>().ReverseMap();

            //Recipients
            CreateMap<RecipientsListDto, Recipient>().ReverseMap();
            CreateMap<MailUserDataDto, Recipient>().ReverseMap();

            //Permissions
            CreateMap<SavePermissionsItem, Core.Models.Permissions>().ReverseMap();
            //FileUpload
            //CreateMap<FileYTLinkUploadDto, Core.Models.FileUpload>()
            //    .ForMember(e => e.OriginalFileName, option => option.MapFrom(c => c.File))
            //    .ForMember(e => e.DownloadFileName, option => option.MapFrom(c => c.File))
            //    .ReverseMap();
        }
    }
}
