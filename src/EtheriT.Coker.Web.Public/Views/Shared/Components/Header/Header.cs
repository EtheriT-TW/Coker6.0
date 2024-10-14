using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Application.Shared.Marquee;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Header
{
    public class Header : ViewComponent
    {

        private readonly IMarqueeAppService marqueeAppService;
        private readonly IWebsiteApplication websiteApplication;
        private readonly IWebMenuApplication webMenuApplication;
        private readonly IConfiguration Configuration;
        public Header(
            IMarqueeAppService marqueeAppService,
            IWebsiteApplication websiteApplication,
            IWebMenuApplication webMenuApplication,
            IConfiguration Configuration
            )
        {
            this.marqueeAppService = marqueeAppService;
            this.websiteApplication = websiteApplication;
            this.webMenuApplication = webMenuApplication;
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
            var website_data = await websiteApplication.GetAllData(defaultData.Id);
			var webmenus_data = (await webMenuApplication.GetDisplayAll(defaultData.Id)).Maps.ToList();

            var marquee = JsonConvert.DeserializeObject<List<MarqueeDisplayDto>>(JsonConvert.SerializeObject((await marqueeAppService.GetAll(website_data[0].Id, "Top")).Value));
            HeaderViewModel headerViewModel = new HeaderViewModel();
            switch (defaultData.Layout_Type)
            {
                case 1:
                case 5:
                case 7:
                    webmenus_data = webmenus_data.ToList();
                    headerViewModel = new HeaderViewModel
                    {
                        Title = website_data[0].Title,
                        LogoImageUrl = "/upload/logo.png",
                        menuItemModels = new List<MenuItem.MenuItemModel> { },
                        marqueeModels = new List<MarqueeDisplayDto> { },

                    };
					if (marquee.Count > 0)
                    {
                        marquee.ForEach(data =>
                        {
                            headerViewModel.marqueeModels.Add(new MarqueeDisplayDto
                            {
                                title = data.title,
                                link = data.link,
                                target = data.target,
                            });
                        });
                    }
                    webmenus_data.ForEach(data_f =>
                    {
                        if (data_f.PageType == (int)PageTypeEnum.首頁)
                        {
                            headerViewModel.HomeLink = $"/{website_data[0].OrgName}/home";
                            headerViewModel.HomeTarget = false;
                        }
                        else
                        {
                            if (data_f.Children != null)
                            {
                                var secitemModels = new List<MenuItem.MenuItemModel> { };
                                int length = 0;
                                data_f.Children.ForEach(data_s =>
                                {
                                    if (data_s.Children != null)
                                    {
                                        var thirditemModels = new List<MenuItem.MenuItemModel> { };
                                        length += data_s.Children.Count();
                                        data_s.Children.ForEach(data_t =>
                                        {
                                            thirditemModels.Add(new MenuItem.MenuItemModel
                                            {
                                                Title = data_t.Title,
                                                Link = data_t.RouterName != "" ? $"/{website_data[0].OrgName}/{data_t.RouterName}" : data_t.LinkUrl != "" ? data_t.LinkUrl : "",
                                                Target = data_t.Target,
                                                Icon = data_t.icon != "empty" ? data_t.icon.StartsWith("IconId", true, null) ? "" : data_t.icon.Split(' ')[1] : "",
                                                IconClass = data_t.icon != "empty" ? data_t.icon.StartsWith("IconId", true, null) ? "" : data_t.icon.Split(' ')[0] : "",
                                                ImageIcon = data_t.IconImage != null ? siteId == defaultData.Id ? data_t.IconImage : data_t.IconImage.Replace("upload", $"upload/{defaultData.OrgName}") : "",
                                            }); ;
                                        });
                                        secitemModels.Add(new MenuItem.MenuItemModel
                                        {
                                            Title = data_s.Title,
                                            Link = data_s.RouterName != "" ? $"/{website_data[0].OrgName}/{data_s.RouterName}" : data_s.LinkUrl != "" ? data_s.LinkUrl : "",
                                            Target = data_s.Target,
                                            Icon = data_s.icon != "empty" ? data_s.icon.StartsWith("IconId", true, null) ? "" : data_s.icon.Split(' ')[1] : "",
                                            IconClass = data_s.icon != "empty" ? data_s.icon.StartsWith("IconId", true, null) ? "" : data_s.icon.Split(' ')[0] : "",
                                            ImageIcon = data_s.IconImage != null ? siteId == defaultData.Id ? data_s.IconImage : data_s.IconImage.Replace("upload", $"upload/{defaultData.OrgName}") : "",
                                            menuItemModels = thirditemModels,
                                        });
                                    }
                                    else
                                    {
                                        secitemModels.Add(new MenuItem.MenuItemModel
                                        {
                                            Title = data_s.Title,
                                            Link = data_s.RouterName != "" ? $"/{website_data[0].OrgName}/{data_s.RouterName}" : data_s.LinkUrl != "" ? data_s.LinkUrl : "",
                                            Target = data_s.Target,
                                            Icon = data_s.icon != "empty" ? data_s.icon.StartsWith("IconId", true, null) ? "" : data_s.icon.Split(' ')[1] : "",
                                            IconClass = data_s.icon != "empty" ? data_s.icon.StartsWith("IconId", true, null) ? "" : data_s.icon.Split(' ')[0] : "",
                                            ImageIcon = data_s.IconImage != null ? siteId == defaultData.Id ? data_s.IconImage : data_s.IconImage.Replace("upload", $"upload/{defaultData.OrgName}") : "",
                                        });
                                    }
                                });
                                headerViewModel.menuItemModels.Add(new MenuItem.MenuItemModel
                                {
                                    Title = data_f.Title,
                                    Link = data_f.hasContan ?
                                        data_f.RouterName != "" ? $"/{website_data[0].OrgName}/{data_f.RouterName}" : data_f.LinkUrl != "" ? data_f.LinkUrl : "" :
                                        "javascript:void(0)",
                                    menuItemModels = secitemModels,
                                    Length = length,
                                    imageUrl = (data_f.ImgUrl ?? ""),
                                    hoverImageUrl = (data_f.OverImgUrl ?? ""),
                                });
                            }
                            else
                            {
                                if (data_f.LanBar)
                                {
                                    headerViewModel.langMenuItemModels.Add(new MenuItem.MenuItemModel
                                    {
                                        Title = data_f.Title,
                                        Target = data_f.Target,
                                        Link = data_f.RouterName != "" ? $"/{website_data[0].OrgName}/{data_f.RouterName}" : data_f.LinkUrl != "" ? data_f.LinkUrl : "",
                                        imageUrl = (data_f.ImgUrl ?? ""),
                                        hoverImageUrl = (data_f.OverImgUrl ?? ""),
                                    });
                                }
                                else
                                {
                                    headerViewModel.menuItemModels.Add(new MenuItem.MenuItemModel
                                    {
                                        Title = data_f.Title,
                                        Target = data_f.Target,
                                        Link = data_f.RouterName != "" ? $"/{website_data[0].OrgName}/{data_f.RouterName}" : data_f.LinkUrl != "" ? data_f.LinkUrl : "",
                                        imageUrl = (data_f.ImgUrl ?? ""),
                                        hoverImageUrl = (data_f.OverImgUrl ?? ""),
                                    });
                                }
                            }
                        }
                    });
                    break;
                case 2:
                    headerViewModel = new HeaderViewModel
                    {
                        Title = "入口網站",
                        HomeLink = $"/{website_data[0].OrgName}/home",
                        HomeTarget = false,
                        menuItemModels = new List<MenuItem.MenuItemModel> {
                            new MenuItem.MenuItemModel {Title="高雄軟體園區資訊服務網", Link="/ksp/home"},
                            new MenuItem.MenuItemModel {Title="e++產業服務平台", Link="/eplus/home"},
                        },
                    };
                    break;
                case 3:
                    webmenus_data = webmenus_data.ToList();
                    headerViewModel = new HeaderViewModel
                    {
                        Title = website_data[0].Title,
                        LogoImageUrl = $"/upload/{website_data[0].OrgName}/logo.png",
                        menuItemModels = new List<MenuItem.MenuItemModel> { },
                    };
                    webmenus_data.ForEach(data_f =>
                    {
                        if (data_f.PageType == (int)PageTypeEnum.首頁)
                        {
                            headerViewModel.HomeLink = $"/{website_data[0].OrgName}/{data_f.RouterName}";
                            headerViewModel.HomeTarget = data_f.Target;
                        }
                        else if (data_f.Children != null)
                        {
                            var tempmenuItemModels = new List<MenuItem.MenuItemModel> { };
                            data_f.Children.ForEach(data_s =>
                            {
                                tempmenuItemModels.Add(new MenuItem.MenuItemModel
                                {
                                    Title = data_s.Title,
                                    Link = data_s.RouterName != "" ? $"/{website_data[0].OrgName}/{data_s.RouterName}" : data_s.LinkUrl != "" ? data_s.LinkUrl : "",
                                    Target = data_s.Target,
                                });
                            });
                            headerViewModel.menuItemModels.Add(new MenuItem.MenuItemModel
                            {
                                Title = data_f.Title,
                                menuItemModels = tempmenuItemModels,
                            });
                        }
                        else
                        {
                            headerViewModel.menuItemModels.Add(new MenuItem.MenuItemModel
                            {
                                Title = data_f.Title,
                                Target = data_f.Target,
                                Link = data_f.RouterName != "" ? $"/{website_data[0].OrgName}/{data_f.RouterName}" : data_f.LinkUrl != "" ? data_f.LinkUrl : "",
                            });
                        }
                    });
                    break;
                case 4:
                    webmenus_data = webmenus_data.ToList();
                    headerViewModel = new HeaderViewModel
                    {
                        Title = website_data[0].Title,
                        LogoImageUrl = $"/upload/{website_data[0].OrgName}/logo.png",
                        menuItemModels = new List<MenuItem.MenuItemModel> { },
                    };
                    webmenus_data.ForEach(data =>
                    {
                        if (data.PageType == (int)PageTypeEnum.首頁)
                        {
                            headerViewModel.HomeLink = $"/{website_data[0].OrgName}/{data.RouterName}";
                            headerViewModel.HomeTarget = data.Target;
                        }
                        else if (data.LanBar)
                        {
                            headerViewModel.Sitemap_Link = data.RouterName != "" ? $"/{website_data[0].OrgName}/{data.RouterName}" : data.LinkUrl != "" ? data.LinkUrl : "";
                            headerViewModel.Sitemap_Target = data.Target;
                        }
                        else
                        {
                            headerViewModel.menuItemModels.Add(new MenuItem.MenuItemModel
                            {
                                Title = data.Title,
                                Link = data.RouterName != "" ? $"/{website_data[0].OrgName}/{data.RouterName}" : data.LinkUrl != "" ? data.LinkUrl : "",
                                Target = data.Target,
                                imageUrl = data.ImgUrl != null ? data.ImgUrl.Replace("upload", $"upload/{website_data[0].OrgName}") : "",
                            });
                        }
                    });
                    break;
                case 6:
                    webmenus_data = webmenus_data.ToList();
                    headerViewModel = new HeaderViewModel
                    {
                        Title = website_data[0].Title,
                        LogoImageUrl = $"/upload/{website_data[0].OrgName}/logo.png",
                        menuItemModels = new List<MenuItem.MenuItemModel> { },
                        marqueeModels = new List<MarqueeDisplayDto> { },

                    };
                    webmenus_data.ForEach(data =>
                    {
                        if (data.PageType == (int)PageTypeEnum.首頁)
                        {
                            headerViewModel.HomeLink = $"/{website_data[0].OrgName}/{data.RouterName}";
                            headerViewModel.HomeTarget = data.Target;
                        }
                        else if (data.LanBar)
                        {
                            headerViewModel.Sitemap_Link = data.RouterName != "" ? $"/{website_data[0].OrgName}/{data.RouterName}" : data.LinkUrl != "" ? data.LinkUrl : "";
                            headerViewModel.Sitemap_Target = data.Target;
                        }
                        else
                        {
                            headerViewModel.menuItemModels.Add(new MenuItem.MenuItemModel
                            {
                                Title = data.Title,
                                Link = data.RouterName != "" ? $"/{website_data[0].OrgName}/{data.RouterName}" : data.LinkUrl != "" ? data.LinkUrl : "",
                                Target = data.Target,
                                imageUrl = data.ImgUrl != null ? data.ImgUrl.Replace("upload", $"upload/{website_data[0].OrgName}") : "",
                            });
                        }
                    });

                    if (marquee.Count > 0)
                    {
                        marquee.ForEach(data =>
                        {
                            headerViewModel.marqueeModels.Add(new MarqueeDisplayDto
                            {
                                title = data.title,
                                link = data.link,
                                target = data.target,
                            });
                        });
                    }
                    break;
                default:
                    break;
            }
            if (defaultData.Layout_Type == 1)
            {
                headerViewModel.LogoImageUrl = "/upload/logo.svg";
            }
            headerViewModel.SearchPath = $"/{website_data[0].OrgName}/Search";
            return View(defaultData.View, headerViewModel);
        }
    }
}
