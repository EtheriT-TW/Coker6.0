using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Freight;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Header
{
    public class Header : ViewComponent
    {

        private readonly IMarqueeAppService marqueeAppService;
        private readonly IWebsiteApplication websiteApplication;
        private readonly IConfiguration Configuration;
        public Header(
            IMarqueeAppService marqueeAppService,
            IWebsiteApplication websiteApplication,
            IConfiguration Configuration
            )
        {
            this.marqueeAppService = marqueeAppService;
            this.websiteApplication = websiteApplication;
            this.Configuration = Configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var website = HttpContext.GetRouteData().Values["website"];
            if (website != null && !website.ToString().Equals("upload"))
            {
                var tempid = await websiteApplication.GetSiteId(siteId, website.ToString());
                if (tempid != 0)
                {
                    siteId = await websiteApplication.GetSiteId(siteId, website.ToString());
                }
            }
            var orgname = await websiteApplication.GetOrgName(siteId);
            orgname = (orgname == null || orgname == "") ? "Page" : orgname;
            var Layout_Type = await websiteApplication.GetLayoutType(siteId);
            var view = Layout_Type == 0 ? "Default" : $"Layout_{Layout_Type}";

            var marquee = JsonConvert.DeserializeObject<List<MarqueeDisplayDto>>(JsonConvert.SerializeObject((await marqueeAppService.GetAll(siteId, "Top")).Value));
            HeaderViewModel headerViewModel = new HeaderViewModel();
            switch (siteId)
            {
                case 4:
                    headerViewModel = new HeaderViewModel
                    {
                        Title = "入口網站",
                        menuItemModels = new List<MenuItem.MenuItemModel> {
                            new MenuItem.MenuItemModel {Title="高雄軟體園區資訊服務網", Link="/ksp/home"},
                            new MenuItem.MenuItemModel {Title="經濟部加工出口區管理處", Link="/eplus/home"},
                        },
                    };
                    break;
                case 5:
                    headerViewModel = new HeaderViewModel
                    {
                        Title = "高雄軟體園區資訊服務網",
                        LogoImageUrl = "/upload/ksp/logo.png",
                        menuItemModels = new List<MenuItem.MenuItemModel> {
                            new MenuItem.MenuItemModel {Title="網站導覽", Link="/ksp/home"},
                            new MenuItem.MenuItemModel {
                                Title="最新消息", menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="園區公告", Link="/ksp/home"},
                                        new MenuItem.MenuItemModel {Title="活動列表", Link="/ksp/home"},
                                        new MenuItem.MenuItemModel { Title = "影音專區", Link = "/ksp/home" },
                                        new MenuItem.MenuItemModel { Title = "防疫紓困專區", Link = "/ksp/home" },
                                }
                            },
                            new MenuItem.MenuItemModel {
                                Title="園區資訊", menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="園區介紹", Link="/ksp/home"},
                                        new MenuItem.MenuItemModel {Title="交通資訊", Link="/ksp/home"},
                                        new MenuItem.MenuItemModel { Title = "休閒設施", Link = "/ksp/home" },
                                        new MenuItem.MenuItemModel { Title = "會議室租借", Link = "/ksp/home" },
                                }
                            },
                        },
                    };
                    break;
                case 6:
                    headerViewModel = new HeaderViewModel
                    {
                        Title = "經濟部加工出口區管理處",
                        LogoImageUrl = "/upload/eplus/logo.png",
                        Sitemap_Link = "#",
                        menuItemModels = new List<MenuItem.MenuItemModel> {
                            new MenuItem.MenuItemModel {
                                Title = "關於平台",
                                imageUrl = "/upload/eplus/btn_about.png",
                                imageLink = "/eplus/home",
                            },
                            new MenuItem.MenuItemModel{
                                Title = "最新消息",
                                imageUrl = "/upload/eplus/btn_news.png",
                                imageLink = "/eplus/home",
                            },
                            new MenuItem.MenuItemModel{
                                Title = "資源手冊",
                                imageUrl = "/upload/eplus/btn_manual.png",
                                imageLink = "/eplus/home",
                            },
                            new MenuItem.MenuItemModel{
                                Title = "電子報",
                                imageUrl = "/upload/eplus/btn_newsletter.png",
                                imageLink = "/eplus/home",
                            },
                        },
                    };
                    break;
                default:
                    headerViewModel = new HeaderViewModel
                    {
                        Title = "德瑞克",
                        LogoImageUrl = "/upload/derek_logo.png",
                        menuItemModels = new List<MenuItem.MenuItemModel> {
                        new MenuItem.MenuItemModel {
                            Title = "關於Derek",
                            menuItemModels = new List<MenuItem.MenuItemModel>{
                                new MenuItem.MenuItemModel {Title="關於Derek", menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="關於Derek", Link=""},
                                        new MenuItem.MenuItemModel {Title="企業沿革", Link=""},
                                        new MenuItem.MenuItemModel { Title = "企業設備", Link = "" },
                                        new MenuItem.MenuItemModel { Title = "品牌故事", Link = "" },
                                    }
                                },
                                new MenuItem.MenuItemModel
                                {
                                    Title = "實績列舉",
                                    menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="近期建案實績", Link=""},
                                        new MenuItem.MenuItemModel {Title="公共工程", Link=""},
                                    }
                                },
                                new MenuItem.MenuItemModel
                                {
                                    Title = "標章認證",
                                    menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="省水標章", Link=""},
                                        new MenuItem.MenuItemModel {Title="環保標章", Link=""},
                                        new MenuItem.MenuItemModel {Title="能源分級", Link=""},
                                        new MenuItem.MenuItemModel {Title="ISO/CNS", Link=""},
                                        new MenuItem.MenuItemModel {Title="應施檢驗", Link=""},
                                        new MenuItem.MenuItemModel {Title="MIT/LF無鉛", Link=""},
                                    }
                                },
                            }
                        },new MenuItem.MenuItemModel
                          {
                              Title = "Derek商品",
                              Description = "線上型錄",
                              imageUrl = "/upload/mu_0.jpg",
                              imageLink = "/Catalog",
                              menuItemModels = new List<MenuItem.MenuItemModel>{
                                new MenuItem.MenuItemModel {Title="商品分類", menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="微電腦馬桶座", Link=""},
                                        new MenuItem.MenuItemModel {Title="馬桶", Link=$"{orgname}/Toilet"},
                                        new MenuItem.MenuItemModel {Title="面盆", Link=""},
                                        new MenuItem.MenuItemModel {Title="便斗", Link=""},
                                        new MenuItem.MenuItemModel {Title="龍頭", Link=""},
                                        new MenuItem.MenuItemModel {Title="配件", Link=""},
                                        new MenuItem.MenuItemModel {Title="浴缸", Link=""},
                                        new MenuItem.MenuItemModel {Title="三機", Link=""},
                                        new MenuItem.MenuItemModel {Title="無障礙設施", Link=""},
                                        new MenuItem.MenuItemModel {Title="線上型錄", Link=$"{orgname}/Catalog"},
                                        new MenuItem.MenuItemModel {Title="清倉品", Link=""},
                                    }
                                },
                                new MenuItem.MenuItemModel {Title="使用須知", menuItemModels = new List<MenuItem.MenuItemModel>{}},
                                new MenuItem.MenuItemModel {Title="購買諮詢服務", menuItemModels = new List<MenuItem.MenuItemModel>{}},
                            }
                          },new MenuItem.MenuItemModel
                          {
                              Title = "最新消息",
                              Description = "年度精彩事",
                              imageUrl = "/upload/mu_1.jpg",
                              menuItemModels = new List<MenuItem.MenuItemModel>{
                                new MenuItem.MenuItemModel {Title="最新消息", menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="人才招募", Link=""},
                                        new MenuItem.MenuItemModel {Title="媒體專區", Link=""},
                                        new MenuItem.MenuItemModel { Title = "粉絲專頁", Link = "" },
                                    }
                                },
                            }
                          },new MenuItem.MenuItemModel
                          {
                              Title = "銷售據點",
                              Description = "新竹旗艦店",
                              imageUrl = "/upload/mu_2.jpg",
                              menuItemModels = new List<MenuItem.MenuItemModel>{
                                new MenuItem.MenuItemModel {Title="銷售據點", menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="龍頭分公司", Link=""},
                                        new MenuItem.MenuItemModel {Title="花東總經銷-百健行", Link=""},
                                        new MenuItem.MenuItemModel {Title="經銷據點", Link=""},
                                    }
                                },
                                new MenuItem.MenuItemModel {Title="展示中心", Link=$"{orgname}/ExhibitionCenter", menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="台北", Link=""},
                                        new MenuItem.MenuItemModel {Title="新竹", Link=""},
                                        new MenuItem.MenuItemModel {Title="台中", Link=""},
                                        new MenuItem.MenuItemModel {Title="台南", Link=""},
                                        new MenuItem.MenuItemModel {Title="高雄", Link=""},
                                    }
                                },
                            }
                          },new MenuItem.MenuItemModel
                          {
                              Title = "客戶服務",
                              Description = "預約參觀",
                              imageUrl = "/upload/mu_3.jpg",
                              menuItemModels = new List<MenuItem.MenuItemModel>{
                                new MenuItem.MenuItemModel {Title="售前服務", menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="購買諮詢服務", Link=""},
                                    }
                                },
                                new MenuItem.MenuItemModel {Title="更多服務", menuItemModels = new List<MenuItem.MenuItemModel>{
                                        new MenuItem.MenuItemModel {Title="清潔小幫手", Link=""},
                                        new MenuItem.MenuItemModel {Title="維修服務", Link=""},
                                        new MenuItem.MenuItemModel {Title="常見問題", Link=""},
                                        new MenuItem.MenuItemModel {Title="使用須知", Link=""},
                                        new MenuItem.MenuItemModel {Title="聯絡我們", Link=$"{orgname}/Contact"},
                                    }
                                },
                            }
                          }
                    },
                        marqueeModels = marquee
                    };
                    break;
            }

            return View(view, headerViewModel);
        }
    }
}
