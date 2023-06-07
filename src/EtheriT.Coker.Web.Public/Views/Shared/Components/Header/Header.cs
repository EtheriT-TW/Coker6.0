using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Marquee;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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


            var marquee = JsonConvert.DeserializeObject<List<MarqueeDisplayDto>>(JsonConvert.SerializeObject((await marqueeAppService.GetAll(siteId, "Top")).Value));
            HeaderViewModel headerViewModel = new HeaderViewModel();
            switch (defaultData.Layout_Type)
            {
                case 1:
                    webmenus_data = webmenus_data.ToList();
                    headerViewModel = new HeaderViewModel
                    {
                        Title = website_data[0].Title,
                        LogoImageUrl = "/upload/logo.png",
                        menuItemModels = new List<MenuItem.MenuItemModel> { },
                    };
                    webmenus_data.ForEach(data_f =>
                    {
                        if (data_f.PageType != (int)PageTypeEnum.首頁)
                        {
                            if (data_f.Children != null)
                            {
                                var tempmenuItemModels = new List<MenuItem.MenuItemModel> { };
                                data_f.Children.ForEach(data_s =>
                                {
                                    tempmenuItemModels.Add(new MenuItem.MenuItemModel
                                    {
                                        Title = data_s.Title,
                                        Link = $"/{website_data[0].OrgName}/{data_s.RouterName}",
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
                                    Link = $"/{website_data[0].OrgName}/{data_f.RouterName}"
                                });
                            }
                        }
                    });
                    break;
                case 2:
                    headerViewModel = new HeaderViewModel
                    {
                        Title = "入口網站",
                        menuItemModels = new List<MenuItem.MenuItemModel> {
                            new MenuItem.MenuItemModel {Title="高雄軟體園區資訊服務網", Link="/ksp/home"},
                            new MenuItem.MenuItemModel {Title="經濟部加工出口區管理處", Link="/eplus/home"},
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
                        if (data_f.Children != null)
                        {
                            var tempmenuItemModels = new List<MenuItem.MenuItemModel> { };
                            data_f.Children.ForEach(data_s =>
                            {
                                tempmenuItemModels.Add(new MenuItem.MenuItemModel
                                {
                                    Title = data_s.Title,
                                    Link = $"/{website_data[0].OrgName}/{data_s.RouterName}",
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
                                Link = $"/{website_data[0].OrgName}/{data_f.RouterName}"
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
                        if (data.LanBar)
                        {
                            headerViewModel.Sitemap_Link = $"/{website_data[0].OrgName}/{data.RouterName}";
                            headerViewModel.Sitemap_Target = data.Target;
                        }
                        else if (data.PageType != (int)PageTypeEnum.首頁)
                        {
                            var imageUrl = "";
                            var imageLink = "";
                            if (data.ImgUrl != null)
                            {
                                imageUrl = data.ImgUrl.Replace("upload", "upload/eplus");
                                imageLink = $"/{website_data[0].OrgName}/{data.RouterName}";
                            }
                            headerViewModel.menuItemModels.Add(new MenuItem.MenuItemModel
                            {
                                Title = data.Title,
                                Target = data.Target,
                                imageUrl = imageUrl,
                                imageLink = imageLink,
                            });
                        }
                    });
                    break;
                default:
                    break;
            }

            return View(defaultData.View, headerViewModel);
        }
    }
}
