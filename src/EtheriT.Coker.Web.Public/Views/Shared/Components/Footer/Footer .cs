using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.Public.Views.Shared.Components.Header;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Footer
{
    public class Footer : ViewComponent
    {
        private readonly IWebsiteApplication websiteApplication;
        private readonly IConfiguration Configuration;
        public Footer(
            IWebsiteApplication websiteApplication,
            IConfiguration Configuration
            )
        {
            this.websiteApplication = websiteApplication;
            this.Configuration = Configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var website = HttpContext.GetRouteData().Values["website"];
            if (website == null)
            {
                website = HttpContext.GetRouteData().Values["key"];
            }
            var website_str = website == null ? "" : website.ToString();
            var defaultData = await websiteApplication.GetDefaultData(siteId, website_str);

            FooterViewModel footerViewModel = new FooterViewModel();

            switch (defaultData.Id)
            {
                case 1:
                    break;
                case 2:
                    footerViewModel = new FooterViewModel
                    {
                        footerViewModels = new List<FooterViewModel> {
                            new FooterViewModel { Title = "最新消息", Link = "/ksp/home", footerViewModels = new List<FooterViewModel> {
                                new FooterViewModel { Title = "園區公告", Link = "/ksp/home" },
                                new FooterViewModel { Title = "活動列表", Link = "/ksp/home" },
                                new FooterViewModel { Title = "影音專區", Link = "/ksp/home" },
                                new FooterViewModel { Title = "防疫紓困專區", Link = "/ksp/home" },
                            },
                            },
                            new FooterViewModel { Title = "園區資訊", Link = "/ksp/home", footerViewModels = new List<FooterViewModel> {
                                new FooterViewModel { Title = "園區介紹", Link = "/ksp/home" },
                                new FooterViewModel { Title = "交通資訊", Link = "/ksp/home" },
                                new FooterViewModel { Title = "休閒設施", Link = "/ksp/home" },
                                new FooterViewModel { Title = "會議室租借", Link = "/ksp/home" },
                            },
                            },
                            new FooterViewModel { Title = "園區資訊", Link = "/ksp/home", footerViewModels = new List<FooterViewModel> {
                                new FooterViewModel { Title = "園區介紹", Link = "/ksp/home" },
                                new FooterViewModel { Title = "交通資訊", Link = "/ksp/home" },
                                new FooterViewModel { Title = "休閒設施", Link = "/ksp/home" },
                                new FooterViewModel { Title = "會議室租借", Link = "/ksp/home" },
                            },
                            },
                            new FooterViewModel { Title = "園區資訊", Link = "/ksp/home", footerViewModels = new List<FooterViewModel> {
                                new FooterViewModel { Title = "園區介紹", Link = "/ksp/home" },
                                new FooterViewModel { Title = "交通資訊", Link = "/ksp/home" },
                                new FooterViewModel { Title = "休閒設施", Link = "/ksp/home" },
                                new FooterViewModel { Title = "會議室租借", Link = "/ksp/home" },
                            },
                            },
                            new FooterViewModel { Title = "園區資訊", Link = "/ksp/home", footerViewModels = new List<FooterViewModel> {
                                new FooterViewModel { Title = "園區介紹", Link = "/ksp/home" },
                                new FooterViewModel { Title = "交通資訊", Link = "/ksp/home" },
                                new FooterViewModel { Title = "休閒設施", Link = "/ksp/home" },
                                new FooterViewModel { Title = "會議室租借", Link = "/ksp/home" },
                            },
                            }
                        },
                        line_qr = "/upload/ksp/line_qr.jpg",
                        line_title = "KSP高雄軟體園區\r\n官方LINE@",
                        line_describe = "即時的掌握園區第一手資訊\r\n快點加入高軟官方Line@",
                        Privacy_Link = $"/{defaultData.OrgName}/Privacy",
                        Accessibility_Link = "#",
                        Accessibility_Badge = "/upload/accessibility_badge.png",
                        Content = new List<string>
                        {
                            "經濟部加工出口區管理處 版權所有© 2020 EPZA ALL Rights Reserved."
                        }
                    };
                    break;
                case 3:
                    footerViewModel = new FooterViewModel
                    {
                        Sitemap_Link = "#",
                        Privacy_Link = $"/{defaultData.OrgName}/Privacy",
                        Accessibility_Link = "#",
                        Accessibility_Badge = "/upload/accessibility_badge.png",
                        Content = new List<string>
                        {
                            "Tel：07-3611212 分機 317 / Fax：07-3612751 地址：811636高雄市楠梓區加昌路600號",
                            "經濟部加工出口區管理處 版權所有 © 2020 EPZA ALL Rights Reserved"
                        }
                    };
                    break;
                default:
                    footerViewModel = new FooterViewModel
                    {
                        footerViewModels = new List<FooterViewModel>{
                            new FooterViewModel {Title="商品分類",Link="", footerViewModels = new List<FooterViewModel>{
                                    new FooterViewModel {Title="微電腦馬桶座", Link=""},
                                    new FooterViewModel {Title="馬桶", Link=$"{defaultData.OrgName}/Toilet"},
                                    new FooterViewModel {Title="面盆", Link=""},
                                    new FooterViewModel {Title="便斗", Link=""},
                                    new FooterViewModel {Title="龍頭", Link=""},
                                    new FooterViewModel {Title="配件", Link=""},
                                    new FooterViewModel {Title="浴缸", Link=""},
                                    new FooterViewModel {Title="三機", Link=""},
                                    new FooterViewModel {Title="無障礙設備", Link=""},
                                    new FooterViewModel {Title="線上型錄", Link=$"{defaultData.OrgName}/Catalog"},
                                    new FooterViewModel {Title="清倉品", Link=""},
                                },
                            },
                            new FooterViewModel {Title="關於Derek",Link="", footerViewModels = new List<FooterViewModel>{
                                    new FooterViewModel {Title="企業理念", Link=""},
                                    new FooterViewModel {Title="企業沿革", Link=""},
                                    new FooterViewModel {Title="企業設備", Link=""},
                                    new FooterViewModel {Title="媒體專區", Link=""},
                                    new FooterViewModel {Title="品牌故事", Link=""},
                                    new FooterViewModel {Title="展示中心", Link=$"{defaultData.OrgName}/ExhibitionCenter"},
                                    new FooterViewModel {Title="實績列舉", Link=""},
                                }
                            },
                            new FooterViewModel {Title="標章認證",Link="", footerViewModels = new List<FooterViewModel>{
                                    new FooterViewModel {Title="省水標章", Link=""},
                                    new FooterViewModel {Title="環保標章", Link=""},
                                    new FooterViewModel {Title="能源分級", Link=""},
                                    new FooterViewModel {Title="ISO/CNS", Link=""},
                                    new FooterViewModel {Title="其他", Link=""},
                                }
                            },
                            new FooterViewModel {Title="銷售據點",Link="", footerViewModels = new List<FooterViewModel>{
                                    new FooterViewModel {Title="分公司", Link=""},
                                    new FooterViewModel {Title="花東總經銷-百健行", Link=""},
                                    new FooterViewModel {Title="經銷據點", Link=""},
                                    new FooterViewModel {Title="公共專案經銷", Link=""},
                                }
                            },
                            new FooterViewModel {Title="我們的服務",Link="", footerViewModels = new List<FooterViewModel>{
                                    new FooterViewModel {Title="清潔小幫手", Link=""},
                                    new FooterViewModel {Title="維修服務", Link=""},
                                    new FooterViewModel {Title="常見問題", Link=""},
                                    new FooterViewModel {Title="使用須知", Link=""},
                                    new FooterViewModel {Title="聯絡我們", Link=$"{defaultData.OrgName}/Contact"},
                                }
                            }
                        }
                    };
                    break;
            }

            return View(defaultData.View, footerViewModel);
        }
    }
}
