using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Application.Shared.Templates;
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
        private readonly ITemplatesApplicationService templatesApplicationService;
        public Header(
            IMarqueeAppService marqueeAppService,
            IWebsiteApplication websiteApplication,
            IWebMenuApplication webMenuApplication,
            IConfiguration Configuration,
            ITemplatesApplicationService templatesApplicationService
        )
        {
            this.marqueeAppService = marqueeAppService;
            this.websiteApplication = websiteApplication;
            this.webMenuApplication = webMenuApplication;
            this.Configuration = Configuration;
            this.templatesApplicationService = templatesApplicationService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            List<string> childOrgNames = new List<string>();
            Configuration.GetSection("WebConfig:childSiteOrgName").Bind(childOrgNames);
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
                default:
                    webmenus_data = webmenus_data.ToList();
                    string uploadPath = childOrgNames.Count > 0 ? $"/upload/{website_data[0].OrgName}" : "/upload";
                    string pathReplace = childOrgNames.Count > 0 ? "/upload" : $"/upload/{website_data[0].OrgName}";
                    string uploadDirectory = Configuration.GetValue<string>("VirtualDirectory:upload") ?? "";
                    headerViewModel = new HeaderViewModel
                    {
                        Title = website_data[0].Title,
                        UploadPath = uploadPath + "/",
                        LogoImageUrl = website_data[0].Logo?.Replace("/upload", uploadPath),
                        HomeLink = childOrgNames.Count > 0 ? $"/{website_data[0].OrgName}/" : "/",
                        HomeTarget = false,
                        menuItemModels = new List<MenuItem.MenuItemModel> { },
                        marqueeModels = new List<MarqueeDisplayDto> { },
                        templates = await templatesApplicationService.GetDefaultTemplatesAsync()
                    };
                    if (File.Exists(Path.Combine(uploadDirectory, "marqueeblockbig.png")))
                    {
                        headerViewModel.marqueeBagroundImage = $"{uploadPath}/marqueeblockbig.png";
                    }
                    if (File.Exists(Path.Combine(uploadDirectory, "marqueeblocksmall.png")))
                    {
                        headerViewModel.marqueeIcon = $"{uploadPath}/marqueeblocksmall.png";
                    }
                    if (string.IsNullOrEmpty(headerViewModel.LogoImageUrl))
                    {
                        headerViewModel.LogoImageUrl = $"{uploadPath}/logo.png";
                    }
                    switch (defaultData.Id)
                    {
                        default:
                            if (headerViewModel.templates != null && headerViewModel.templates.templateSections.Exists(e => e.sectionType == SectionTypeEnum.表頭))
                            {
                                var bannerSection = headerViewModel.templates.templateSections.FirstOrDefault(e => e.sectionType == SectionTypeEnum.表頭);
                                if (bannerSection != null) {
                                    var list = JsonConvert.DeserializeObject<HeaderContentConfigDto>(bannerSection.ContentConfig);
                                    if (list != null )
                                    {
                                        list.ShowMarquee = headerViewModel.ShowMarquee;
                                        list.ShowPagePath = headerViewModel.ShowPagePath;
                                        if (list.Sliders.Count > 0)
                                        {
                                            foreach (var item in list.Sliders)
                                            {
                                                if (item.DesktopImage != null)
                                                {
                                                    item.DesktopImage = item.DesktopImage.Replace(pathReplace, uploadPath);
                                                }
                                                if (item.MobileImage != null)
                                                {
                                                    item.MobileImage = item.MobileImage.Replace(pathReplace, uploadPath);
                                                }else if(!string.IsNullOrEmpty(item.DesktopImage)) item.MobileImage = item.DesktopImage;
                                            }
                                            headerViewModel.Bannners = list.Sliders;
                                        }
                                    }
                                }
                            } 
                            if(!headerViewModel.Bannners.Any()) {
                                if (!string.IsNullOrEmpty(headerViewModel.LogoImageUrl))
                                {
                                    string LogoImage = Path.Combine(uploadDirectory, headerViewModel.LogoImageUrl.Replace("/upload/", ""));
                                    if (!File.Exists(LogoImage))
                                    {
                                        headerViewModel.LogoImageUrl = "";
                                    }
                                }
                                var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".avif", ".gif" };
                                var bannerDir = Path.Combine(uploadDirectory, "banner");
                                var allFiles = new List<FileInfo>();

                                if (Directory.Exists(uploadDirectory))
                                {
                                    allFiles.AddRange(
                                        Directory.GetFiles(uploadDirectory, "headertitile*.*", SearchOption.TopDirectoryOnly)
                                            .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                                            .Select(file => new FileInfo(file))
                                    );
                                }

                                if (Directory.Exists(bannerDir))
                                {
                                    allFiles.AddRange(
                                        Directory.GetFiles(bannerDir, "banner*.*", SearchOption.TopDirectoryOnly)
                                            .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                                            .Select(file => new FileInfo(file))
                                    );
                                }
                                var sortedFiles = allFiles.OrderByDescending(f => f.Name);
                                var fileDict = allFiles.ToDictionary(
                                    f => f.Name,
                                    f =>
                                    {
                                        var virtualPath = f.FullName.Contains("banner" + Path.DirectorySeparatorChar)
                                            ? "/upload/banner/" + f.Name
                                            : "/upload/" + f.Name;

                                        return virtualPath.Replace("\\", "/");
                                    }
                                );

                                var handledSet = new HashSet<string>();
                                foreach (var file in sortedFiles)
                                {
                                    var fileName = file.Name;
                                    if (handledSet.Contains(fileName)) continue;
                                    string baseName;
                                    bool isPhone = fileName.Contains("_phone.");
                                    if (isPhone) baseName = fileName.Replace("_phone", "");
                                    else baseName = fileName;

                                    var phoneVersion = baseName.Replace(".", "_phone.");
                                    handledSet.Add(fileName);
                                    handledSet.Add(phoneVersion);
                                    var banner = new SliderDto();
                                    if (!isPhone && fileDict.ContainsKey(fileName))
                                    {
                                        banner.DesktopImage = fileDict[fileName];
                                    }

                                    if (fileDict.ContainsKey(phoneVersion))
                                    {
                                        banner.MobileImage = fileDict[phoneVersion];
                                    }

                                    if (banner.DesktopImage != null || banner.MobileImage != null)
                                    {
                                        headerViewModel.Bannners.Add(banner);
                                    }
                                }
                            }
                            break;
                    }

                    if (marquee != null && marquee.Count > 0)
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
                                SubTitle = data_f.SubTitle,
                                Link = data_f.hasContan ?
                                    data_f.RouterName != "" ? $"/{website_data[0].OrgName}/{data_f.RouterName}" : data_f.LinkUrl != "" ? data_f.LinkUrl : "" :
                                    "javascript:void(0)",
                                menuItemModels = secitemModels,
                                Length = length,
                                imageUrl = (data_f.ImgUrl ?? "").Replace("/upload", uploadPath),
                                hoverImageUrl = (data_f.OverImgUrl ?? "").Replace("/upload", uploadPath),
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
                                    SubTitle = data_f.SubTitle,
                                    Target = data_f.Target,
                                    Link = data_f.RouterName != "" ? $"/{website_data[0].OrgName}/{data_f.RouterName}" : data_f.LinkUrl != "" ? data_f.LinkUrl : "",
                                    imageUrl = (data_f.ImgUrl ?? ""),
                                    hoverImageUrl = (data_f.OverImgUrl ?? ""),
                                });
                            }
                        }
                    });
                    break;
            }
            if (defaultData.Layout_Type == 1 && siteId == 2)
            {
                headerViewModel.LogoImageUrl = website_data[0].Logo?.Replace($"/{website_data[0].OrgName}/", "/");
                if (string.IsNullOrEmpty(headerViewModel.LogoImageUrl)) headerViewModel.LogoImageUrl = "/upload/logo.svg";
            }
            headerViewModel.SearchPath = $"/{website_data[0].OrgName}/Search";
            var view = defaultData.View;
            if (headerViewModel.templates != null)
            {
                switch (headerViewModel.templates.HeadType)
                {
                    case HeadTypeEnum.logo在左選單在右:
                    case HeadTypeEnum.logo與Banner重疊:
                        view = "Layout_8";
                        break;
                    default:
                        view = "Layout_7";
                        break;
                }
            }
            return View(view, headerViewModel);
        }
    }
}
