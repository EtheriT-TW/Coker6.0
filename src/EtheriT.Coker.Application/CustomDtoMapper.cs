using AutoMapper;
using DevExpress.CodeParser;
using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Company;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.AuditLog;
using EtheriT.Coker.Application.Dto.Newsletter;
using EtheriT.Coker.Application.Dto.ObjectType;
using EtheriT.Coker.Application.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.Contact;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.Dto.MailTemplate;
using EtheriT.Coker.Application.Shared.Dto.Member;
using EtheriT.Coker.Application.Shared.Dto.Newsletter;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.Permissions;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Recipients;
using EtheriT.Coker.Application.Shared.Dto.Remote;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.Dto.Token;
using EtheriT.Coker.Application.Shared.Dto.UserHabits;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.Core.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Common;

namespace EtheriT.Coker.Application
{
    public class CustomDtoMapper : Profile
    {
        private double? ParseDouble(string? value)
        {
            if (double.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }
        public static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var s = input;

            // 常見控制/換行 → 空白
            s = s.Replace('\r', ' ')
                 .Replace('\n', ' ')
                 .Replace('\t', ' ');

            // 看似空白 → 正常空白；零寬/BOM → 直接移除
            s = s.Replace('\u00A0', ' ') // NBSP
                 .Replace('\u2007', ' ') // Figure Space
                 .Replace('\u202F', ' ') // Narrow NBSP
                 .Replace("\u200B", "")  // Zero Width Space
                 .Replace("\u200C", "")  // ZWNJ
                 .Replace("\u200D", "")  // ZWJ
                 .Replace("\u2060", "")  // Word Joiner
                 .Replace("\uFEFF", ""); // BOM

            // 壓縮連續空白
            while (s.Contains("  "))
                s = s.Replace("  ", " ");

            return s.Trim();
        }
        public static bool HasContent(WebMenu s)
        {
            // 文字有值
            if (!string.IsNullOrWhiteSpace(s.PageText))
                return true;

            if (string.IsNullOrEmpty(s.Html))
                return false;

            // decode 後再檢查
            var html = WebUtility.HtmlDecode(s.Html);
            return Regex.IsMatch(html, @"<(img|iframe|video)\b", RegexOptions.IgnoreCase);
        }
        // Mapping
        // 第一個參數是來源，第二個參數是目標
        public CustomDtoMapper()
        {
            //全域字串Trim
            ValueTransformers.Add<string?>(s => s == null ? null : s.Trim());

            //Token
            CreateMap<TokenResponseDto, Core.Models.Token>()
                .ForMember(e => e.StartTime, option => option.MapFrom(c => DateTime.Now))
                .ForMember(e => e.EndTime, option => option.MapFrom(c => DateTime.Now.AddDays(30)))
                .ReverseMap()
                .ForMember(e => e.IsLogin, option => option.MapFrom(t => t.UserID != null && DateTime.Now > t.EndTime));
            //Role
            CreateMap<AddRoleDto, Role>().ReverseMap();
            //Users
            CreateMap<UserSimplifyDto, User>()
                .ForMember(e => e.Name, option => option.MapFrom(c => c.UserName))
                .ReverseMap();
            CreateMap<MemberGetAllDataDto, User>().ReverseMap();
            CreateMap<MemberUpdateDto, User>().ReverseMap();
            CreateMap<MemberGetAllDataDto, FrontUser>().ReverseMap();
            CreateMap<MemberUpdateDto, FrontUser>().ReverseMap();
            CreateMap<ManagerAllListDto, User>().ReverseMap();
            CreateMap<EditUserDto, User>().ReverseMap();
            CreateMap<FrontUser, EditUserDto>().ReverseMap();
            CreateMap<FrontEditUserDto, FrontUser>().ReverseMap();
            CreateMap<FrontAddUserDto, FrontUser>().ReverseMap();
            CreateMap<FrontAddUserDto, User>().ReverseMap();
            CreateMap<FrontUser, User>().ReverseMap();
            CreateMap<FrontAddUserDto, MappingFrontUserAndWebsite>().ReverseMap();
            CreateMap<FrontAddUserDto, SendOpeningDto>().ReverseMap();
            CreateMap<FrontUser, BackendTemplateResuleDto>()
                .ForMember(e => e.ExpireTime, option => option.MapFrom(c => (c.ForgeIDSendDate ?? DateTime.Now).AddDays(1)))
                .ForMember(e => e.SetPasswordUrl, option => option.MapFrom(c => $"/?useraction=passwordforget&forgetid={c.ForgetID}"))
                .ReverseMap();

            //UserGroup
            CreateMap<UserGroupAddUpDto, UserGrouping>().ReverseMap();
            CreateMap<UserGroupAddUpInputDto, UserGrouping>().ReverseMap();
            CreateMap<UserGroupListDto, UserGrouping>().ReverseMap();

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

            CreateMap<MenuItemDto, WebMenu>()
                .ForMember(d => d.RemovedFromShelves, o => o.MapFrom(s => !s.IsFromShelves))
                .ReverseMap()
                .ForMember(d => d.IsFromShelves, o => o.MapFrom(s => !s.RemovedFromShelves))
                .ForMember(d => d.OrgName, o => o.MapFrom(s => s.Website != null ? s.Website.OrgName : null))
                .ForMember(d => d.hasContan, o => o.MapFrom(s => HasContent(s)));


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
            CreateMap<SelectDto, WebMenu>()
                .ForMember(e => e.Title, option => option.MapFrom(c => c.Name))
                .ForMember(e => e.Visible, option => option.MapFrom(c => true))
                .ForMember(e => e.SerNO, option => option.MapFrom(c => 500))
                .ForMember(e => e.Popular, option => option.MapFrom(c => 0))
                .ForMember(e => e.PopularVisible, option => option.MapFrom(c => false))
                .ForMember(e => e.LanBar, option => option.MapFrom(c => false))
                .ReverseMap();

            //Html_Content
            CreateMap<HtmlContentDto, Html_Content>()
                .ReverseMap()
                .ForMember(e => e.TypeName, option => option.MapFrom(c => c.ObjectClassify.Title));
            CreateMap<ObjectTypeItemDto, Html_Content>()
               .ForMember(e => e.Type, option => option.MapFrom(c => c.FK_TopNodeId))
               .ForMember(e => e.Disp_opt, option => option.MapFrom(c => c.Visible))
               .ReverseMap()
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
                .ForMember(e => e.Title, option => option.MapFrom(c => c.ProdName ?? ""))
                .ForMember(e => e.Description, option => option.MapFrom(c => c.Description ?? ""))
                .ForMember(e => e.Introduction, option => option.MapFrom(c => c.Introduction ?? ""))
                .ForMember(e => e.Ser_No, option => option.MapFrom(c => 500))
                .ForMember(e => e.Status, option => option.MapFrom(c => (int)Enum.Parse(typeof(ProdStatusEnum), c.Status)))
                .ForMember(e => e.permanent, option => option.MapFrom(c => true))
                .ReverseMap();
            CreateMap<ProductImportUpateDto, Prod>()
                .ForMember(e => e.Title, option => option.MapFrom(c => c.ProdName))
                .ForMember(e => e.Description, option => option.MapFrom(c => c.Description ?? ""))
                .ForMember(e => e.Introduction, option => option.MapFrom(c => c.Introduction ?? ""))
                .ForMember(e => e.Ser_No, option => option.MapFrom(c => 500))
                .ForMember(e => e.permanent, option => option.MapFrom(c => true))
                .ForMember(e => e.Status, option => option.MapFrom(c => 0))
                .ForMember(e => e.Visible, option => option.MapFrom(c => true))
                .ForMember(e => e.RemovedFromShelves, option => option.MapFrom(c => false))
                .ReverseMap();
            CreateMap<ProdAddUpDto, Prod>()
                .ForMember(e => e.permanent, option => option.MapFrom(c => c.Permanent))
                .ReverseMap();
            CreateMap<ProductImportDto, ProdAddUpDto>()
                .ForMember(e => e.Id, option => option.MapFrom(c => 0))
                .ForMember(e => e.Ser_No, option => option.MapFrom(c => 500))
                .ForMember(e => e.Permanent, option => option.MapFrom(c => true))
                .ReverseMap();
            CreateMap<ProdGetDataDto, Prod>()
                .ForMember(e => e.permanent, option => option.MapFrom(c => c.Permanent))
                .ReverseMap();
            CreateMap<ProdGetDataDto, DirectoryReleInfoDto>()
                .ForMember(e => e.Description, option => option.MapFrom(c => c.Introduction))
                .ReverseMap();
            CreateMap<DirectoryPriceResultDto, DirectoryReleInfoDto>();
            CreateMap<ProductStockDto, ProductImportDto>()
                .ReverseMap()
                .ForMember(e => e.S1_Name, option => option.MapFrom(c => c.Spec1Name))
                .ForMember(e => e.S2_Name, option => option.MapFrom(c => c.Spec2Name))
                .ForMember(e => e.S1_Title, option => option.MapFrom(c => c.Spec1))
                .ForMember(e => e.S2_Title, option => option.MapFrom(c => c.Spec2))
                .ForMember(e => e.TimePrice, option => option.MapFrom(c => c.Price < 0))
                .ForMember(e => e.Prices, option => option.MapFrom(c => new List<ProductPriceDto> { new ProductPriceDto { Price = c.Price < 0 ? 0 : c.Price, FK_RId = 1 } }));

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
            CreateMap<ProductImportUpateRegDto, ProductImportDto>()
                .ForMember(e => e.ProdName, option => option.MapFrom(p => Normalize(p.ProdName)))
                .ForMember(e => e.Price, option => option.MapFrom(p => ParseDouble(p.Price)??-1))
                .ReverseMap()
                .ForMember(e => e.ProdName, option => option.MapFrom(p => Normalize(p.ProdName)));


            //FrontUser
            CreateMap<OrderHeaderAddDto, FrontEditUserDto>()
                .ForMember(e => e.Name, option => option.MapFrom(c => c.Orderer))
                .ForMember(e => e.Sex, option => option.MapFrom(c => c.OrdererSex))
                .ForMember(e => e.TelPhone, option => option.MapFrom(c => c.OrdererTelePhone))
                .ForMember(e => e.CellPhone, option => option.MapFrom(c => c.OrdererCellPhone))
                .ForMember(e => e.Address, option => option.MapFrom(c => c.OrdererAddress))
                .ReverseMap();

            //Order
            CreateMap<OrderHeaderDataDto, Order_Header>()
                .ReverseMap();
            CreateMap<Order_Header, OrderHeaderAddDto>()
                .ReverseMap();
            CreateMap<Order_Header, OrderHeaderDisplayDto>()
                .ForMember(e => e.RefundTransactionId, option => option.MapFrom(c => c.refundTransactionId))
                .ForMember(e => e.PaymentCode, option => option.MapFrom(c => c.Payment))
                .ReverseMap();
            CreateMap<ShoppingCartDisplayDto, OrderDetailDisplayDto>()
                 .ForMember(e => e.ProdId, option => option.MapFrom(c => c.PId))
                 .ForMember(e => e.ProdStockId, option => option.MapFrom(c => c.PSId))
                .ReverseMap();
            CreateMap<ShoppingCartGetAllDto, OrderDetailDisplayDto>()
                 .ForMember(e => e.ProdId, option => option.MapFrom(c => c.PId))
                 .ForMember(e => e.ProdStockId, option => option.MapFrom(c => c.PSId))
                 .ForMember(e => e.Describe, option => option.MapFrom(c => c.Description))
                .ReverseMap();

            //ThirdParty
            CreateMap<PayOrderItem, LinePayProductsDto>()
                .ForMember(d => d.id, o => o.MapFrom(s => s.ItemId ?? ""))
                .ForMember(d => d.name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.imageUrl, o => o.MapFrom(s => s.ImageUrl ?? ""))
                .ForMember(d => d.quantity, o => o.MapFrom(s => s.Quantity.ToString()))
                .ForMember(d => d.price, o => o.MapFrom(s => s.PayUnitPrice.ToString()));

            CreateMap<PayOrderData, LinePayPackageDto>()
                .ForMember(d => d.id, o => o.MapFrom(s => s.OrderId))
                .ForMember(d => d.amount, o => o.MapFrom(s => s.PayableAmount.ToString()))
                .ForMember(d => d.userFee, o => o.MapFrom(s => "0"))
                .ForMember(d => d.name, o => o.MapFrom(s => $"訂單編號：{s.OrderId}"))
                .ForMember(d => d.products, o => o.MapFrom(s => s.Items));

            CreateMap<PayOrderData, LinePayRequestBodyDto>()
                .ForMember(d => d.amount, o => o.MapFrom(s => s.PayableAmount.ToString()))
                .ForMember(d => d.currency, o => o.MapFrom(s => s.Currency))
                .ForMember(d => d.orderId, o => o.MapFrom(s => $"{s.PaymentTime:yyyyMMdd}{s.OrderId}"))
                .ForMember(d => d.packages, o => o.MapFrom(s => new List<PayOrderData> { s }))
                .ForMember(d => d.redirectUrls, o => o.Ignore())
                .ForMember(d => d.options, o => o.Ignore());

            CreateMap<ECPayLogisticsCreateCVSRequestDto, ECPayLogisticsCreateRequestDto>()
                .ReverseMap();

            //ShoppingCart
            CreateMap<Core.Models.ShoppingCart, Core.Models.Prod_Stock>()
                 .ForMember(e => e.Id, option => option.MapFrom(c => c.FK_PSid))
                .ReverseMap();
            CreateMap<Core.Models.ShoppingCart, ShoppingCartAddUpDto>()
                .ReverseMap();
            CreateMap<Core.Models.ShoppingCart, ShoppingCartAddUpOldDto>()
                .ReverseMap();
            CreateMap<Core.Models.ShoppingCart, ShoppingCartDisplayDto>()
                .ForMember(d => d.SCId, m => m.MapFrom(s => s.Id))
                .ForMember(d => d.PSId, m => m.MapFrom(s => s.FK_PSid))
                .ForMember(d => d.PPId, m => m.MapFrom(s => s.FK_PriceId))

                // 先只抓必要資料（避免在 SQL 做複雜字串）
                .ForMember(d => d.Freight, m => m.MapFrom(s =>
                    (s.Prod_Stock != null && s.Prod_Stock.Prod != null)
                        ? s.Prod_Stock.Prod.MappingLogisticsSettingAndProds
                            .Where(x =>
                                x.FK_ProdId == s.Prod_Stock.FK_Pid &&
                                x.LogisticsSetting != null &&
                                !x.LogisticsSetting.IsDeleted &&
                                x.LogisticsSetting.FreightStatusType != FreightStatusTypeEnum.停用
                            )
                            .OrderBy(x => x.LogisticsSetting!.FreightStatusType)
                            .ThenBy(x => x.LogisticsSetting!.LogisticsType)
                            .Select(x => new FreightGetAllListDto
                            {
                                Id = x.LogisticsSetting!.Id,
                                Title = x.LogisticsSetting.Title,

                                // 先只放基本資訊，Describe 之後補
                                Describe = ""
                            })
                            .FirstOrDefault()
                        : null
                ))

                // ⭐ 關鍵：這裡補完整 Describe
                .AfterMap((src, dest) =>
                {
                    if (src.Prod_Stock?.Prod == null || dest.Freight == null) return;

                    var mapping = src.Prod_Stock.Prod.MappingLogisticsSettingAndProds
                        .Where(x =>
                            x.FK_ProdId == src.Prod_Stock.FK_Pid &&
                            x.LogisticsSetting != null &&
                            !x.LogisticsSetting.IsDeleted &&
                            x.LogisticsSetting.FreightStatusType != FreightStatusTypeEnum.停用
                        )
                        .OrderBy(x => x.LogisticsSetting!.FreightStatusType)
                        .ThenBy(x => x.LogisticsSetting!.LogisticsType)
                        .FirstOrDefault();

                    var ls = mapping?.LogisticsSetting;
                    if (ls == null) return;

                    dest.Freight.Describe = DisplayTextFormatter.Freight(
                        ls,
                        includeStatus: false,
                        detailedBoxText: true,
                        includePackingHint: true
                    );
                })

                .ReverseMap();


            //Tags
            CreateMap<TagSelectedDto, Core.Models.Tag>()
                .ForMember(e => e.Id, option => option.MapFrom(c => c.FK_TId))
                .ReverseMap();
            CreateMap<SelectDto, Core.Models.Tag>()
                .ForMember(e => e.Title, option => option.MapFrom(c => c.Name))
                .ReverseMap();


            //Article
            CreateMap<ArticleDto, Core.Models.Article>()
                .ReverseMap();
            CreateMap<ArticleGetDataDto, Core.Models.Article>()
                .ReverseMap()
                .ForMember(e => e.NodeDate, option => option.MapFrom(c => c.NodeDate == null ? null : c.NodeDate.Value.ToString("yyyy/MM/dd")));
            CreateMap<ArticleGetDataDto, DirectoryReleInfoDto>().ReverseMap();
            CreateMap<ArticleListGetDto, DirectoryReleInfoDto>().ReverseMap();
            CreateMap<ArticleListGetDto, Core.Models.Article>()
                .ReverseMap()
                .ForMember(e => e.NodeDate, option => option.MapFrom(c => c.NodeDate == null ? null : c.NodeDate.Value.ToString("yyyy/MM/dd")));
            CreateMap<NewslatterContenDto, DirectoryReleInfoDto>()
                .ForMember(e => e.Title, option => option.MapFrom(c => c.Title))
                .ForMember(e => e.Description, option => option.MapFrom(c => c.Conten))
                .ForMember(e => e.MainImage, option => option.MapFrom(c => c.image == null ? "" : c.image.Path))
                .ReverseMap();

            //Advertise
            CreateMap<AdvertiseDto, Core.Models.Advertise>()
                .ForMember(e => e.StartDate, option => option.MapFrom(c => c.StartTime))
                .ForMember(e => e.EndDate, option => option.MapFrom(c => c.EndTime))
                .ReverseMap();
            CreateMap<Core.Models.Advertise, AdvertiseGetDataDto>()
                .ForMember(e => e.StartTime, option => option.MapFrom(c => c.StartDate))
                .ForMember(e => e.EndTime, option => option.MapFrom(c => c.EndDate))
                .ReverseMap();

            //Directory
            CreateMap<DirectoryAddUpDto, Core.Models.Directory>().ReverseMap();
            CreateMap<ProdGetDataDto, DirectoryReleInfoDto>()
                .ForMember(e => e.Description, option => option.MapFrom(c => c.Introduction))
                .ReverseMap();

            CreateMap<DirectoryFacetRange, DirectoryFacetRangeDto>();
            CreateMap<DirectoryFacetRangeDto, DirectoryFacetRange>()
                .ForMember(d => d.Id, opt => opt.Ignore()) 
                .ForMember(d => d.FK_DirectoryId, opt => opt.Ignore())
                .ForMember(d => d.Directory, opt => opt.Ignore());
            CreateMap<Core.Models.Directory, DirectoryFacetConfigDto>()
                .ForMember(d => d.DirectoryId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Ranges, opt => opt.MapFrom(s => s.DirectoryFacetRanges));

            //StoreSet
            CreateMap<StoreSetOutputDto, Core.Models.StoreSet>()
                .ForMember(e => e.type, option => option.MapFrom(c => c.type))
                .ReverseMap();
            CreateMap<StoreSetDetailOutputDto, Core.Models.StoreSetDetail>()
                .ForMember(e => e.IsDeleted, option => option.MapFrom(c => false))
                .ForMember(e => e.CreationTime, option => option.MapFrom(c => DateTime.Now))
                .ReverseMap()
                .ForMember(e => e.value, option => option.MapFrom(c => new List<string>()))
                .ForMember(e => e.key, option => option.MapFrom(c => c.StoreSet == null ? "" : c.StoreSet.key));
            CreateMap<StoreSetGroupOutputDto, StoreSetGroup>().ReverseMap();
            CreateMap<StoreSetItemOutputDto, storeSetItem>().ReverseMap();

            //CustSearch
            CreateMap<CuseSearchListDto, CustSearch>().ReverseMap();

            //Recipients
            CreateMap<RecipientsDto, Recipient>()
                .ReverseMap();
            CreateMap<RecipientsListDto, Recipient>()
                .ForMember(e => e.Address, option => option.MapFrom(c => ""))
                .ForMember(e => e.CellPhone, option => option.MapFrom(c => ""))
                .ForMember(e => e.TelePhone, option => option.MapFrom(c => ""))
                .ForMember(e => e.Sex, option => option.MapFrom(c => SexEnum.其他))
                .ReverseMap();
            CreateMap<MailUserDataDto, Recipient>().ReverseMap();
            CreateMap<OrderHeaderAddDto, RecipientsDto>()
                .ForMember(e => e.Id, option => option.MapFrom(c => c.RecipientId))
                .ForMember(e => e.Name, option => option.MapFrom(c => c.Recipient))
                .ForMember(e => e.Email, option => option.MapFrom(c => c.RecipientEmail))
                .ForMember(e => e.Address, option => option.MapFrom(c => c.RecipientAddress))
                .ForMember(e => e.CellPhone, option => option.MapFrom(c => c.RecipientCellPhone))
                .ForMember(e => e.TelePhone, option => option.MapFrom(c => c.RecipientTelePhone))
                .ForMember(e => e.Sex, option => option.MapFrom(c => c.RecipientSex))
                .ReverseMap();

            //LogisticsSetting
            CreateMap<FreightDto, LogisticsSetting>()
                .ForMember(e => e.Low_Con, option => option.MapFrom(c => c.Low_Con ?? 0))
                .ForMember(e => e.Dis_Freight, option => option.MapFrom(c => c.Dis_Freight ?? 0))
                .ForMember(e => e.logisticsBoxFees, option => option.Ignore())
                .ForMember(e => e.MappingLogisticsSettingAndProds, option => option.Ignore());

            CreateMap<LogisticsSetting, FreightDto>()
                .ForMember(e => e.ProdIds, option => option.MapFrom(c =>
                    c.MappingLogisticsSettingAndProds == null
                        ? new List<ProdSelectedDto>()
                        : c.MappingLogisticsSettingAndProds.Select(e => new ProdSelectedDto
                        {
                            Id = e.FK_LogisticsSettingId,
                            FK_ProdId = e.FK_ProdId,
                            IsDeleted = false,
                            prod_Name = e.Prod.Title
                        }).ToList()))
                .ForMember(e => e.LogisticsBoxFees, option => option.MapFrom(c =>
                    c.logisticsBoxFees == null
                        ? new List<LogisticsBoxFeeDto>()
                        : c.logisticsBoxFees
                            .Where(x => !x.IsDeleted)
                            .Select(x => new LogisticsBoxFeeDto
                            {
                                Id = x.Id,
                                FK_LogisticsBoxId = x.FK_LogisticsBoxId,
                                Fee = x.Fee,
                                Name = x.logisticsBox != null ? x.logisticsBox.Name : ""
                            }).ToList()));

            CreateMap<LogisticsSetting, FreightGetAllListDto>()
                .ForMember(d => d.Describe, m => m.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.Describe = DisplayTextFormatter.Freight(
                        src,
                        includeStatus: true,
                        detailedBoxText: false,
                        includePackingHint: false
                    );
                });

            CreateMap<LogisticsBoxFee, LogisticsBoxFeeDisplayDto>()
                .ForMember(d => d.LogisticsBoxId, m => m.MapFrom(s => s.FK_LogisticsBoxId))
                .ForMember(d => d.Name, m => m.MapFrom(s => s.logisticsBox != null ? s.logisticsBox.Name : ""))
                .ForMember(d => d.CapacityPoint, m => m.MapFrom(s => s.logisticsBox != null ? s.logisticsBox.CapacityPoint : 0))
                .ForMember(d => d.Fee, m => m.MapFrom(s => s.Fee));

            CreateMap<LogisticsSetting, FreightDisplayDto>()
                .ForMember(d => d.Freight, m => m.MapFrom(s => s.Freight ?? 0))
                .ForMember(d => d.FreightStatusType, m => m.MapFrom(s => (int)s.FreightStatusType))
                .ForMember(d => d.FreightType, m => m.MapFrom(s => (int)s.FreightType))
                .ForMember(d => d.GetMap, m => m.MapFrom(s => (int)s.LogisticsType >= 8 && (int)s.LogisticsType <= 15))
                .ForMember(d => d.LogisticsSubType, m => m.Ignore())
                .ForMember(d => d.Describe, m => m.Ignore())
                .ForMember(d => d.LogisticsBoxFees, m => m.MapFrom(s => s.logisticsBoxFees))
                .AfterMap((src, dest) =>
                {
                    dest.Describe = DisplayTextFormatter.Freight(
                        src,
                        includeStatus: false,
                        detailedBoxText: true,
                        includePackingHint: true
                    );

                    dest.LogisticsSubType = DisplayTextFormatter.LogisticsSubType(src.LogisticsType);

                    dest.LogisticsBoxFees = (dest.LogisticsBoxFees ?? new List<LogisticsBoxFeeDisplayDto>())
                        .OrderBy(x => x.CapacityPoint)
                        .ThenBy(x => x.LogisticsBoxId)
                        .ToList();
                });

            CreateMap<GetLogisticsBoxAllListInputDto, LogisticsBox>().ReverseMap();

            //Permissions
            CreateMap<SavePermissionsItem, Core.Models.Permissions>().ReverseMap();
            //FileUpload
            //CreateMap<FileYTLinkUploadDto, Core.Models.FileUpload>()
            //    .ForMember(e => e.OriginalFileName, option => option.MapFrom(c => c.File))
            //    .ForMember(e => e.DownloadFileName, option => option.MapFrom(c => c.File))
            //    .ReverseMap();

            //remote
            CreateMap<RemoteInputDto, Core.Models.Remote>()
                .ForMember(e => e.ExecutionTime, option => option.MapFrom(c => DateTime.Now))
                .ReverseMap();

            //Contact
            CreateMap<ContactListDto, Core.Models.Contact>().ReverseMap();
            CreateMap<AsrFormDataDto, Core.Models.Contact>().ReverseMap();

            //MappingFrontUserAndWebsite
            CreateMap<FrontAddUserDto, Core.Models.MappingFrontUserAndWebsite>()
                .ForMember(e => e.FK_WebsiteId, option => option.MapFrom(c => c.WebsiteId))
                .ReverseMap();

            //Template
            CreateMap<TemplatesDto, Template>()
                .ReverseMap();
            CreateMap<FooterTemplate, FooterTemplateDto>()
                .ReverseMap();
            CreateMap<TemplateSections, TemplateSectionsDto>()
            .ForMember(dest => dest.footerTemplateDto, opt =>
                opt.MapFrom(src => src.sectionType == SectionTypeEnum.頁尾
                    ? new FooterTemplateDto
                    {
                        id = src.footerTemplates != null ? src.footerTemplates.Id : null,
                        html = src.footerTemplates != null ? src.footerTemplates.saveHtml??"" : "",
                        css = src.footerTemplates != null ? src.footerTemplates.saveCss??"" : ""
                    } : null
                ));
            CreateMap<TemplateSections, HeaderTemplateDto>()
                .ForMember(dest => dest.HeadType, opt => opt.MapFrom(src => src.template.HeadType))
                .ForMember(dest => dest.ContentConfig, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ContentConfig)
                        ? new HeaderContentConfigDto()
                        : JsonConvert.DeserializeObject<HeaderContentConfigDto>(src.ContentConfig)
                ));

            CreateMap<HeaderTemplateDto, TemplateSections>()
                .ForMember(dest => dest.ContentConfig, opt => opt.MapFrom(src =>
                    JsonConvert.SerializeObject(src.ContentConfig)
                ))
                .ForMember(dest => dest.FK_TemplateID, opt => opt.Ignore())  // 這個視情況而定
                .ForMember(dest => dest.template, opt => opt.Ignore());


            //綠界
            CreateMap<ECPayCreditDetailDataDto, ECPayQueryTradeDataDto>()
                .ReverseMap();
        }
    }
}
