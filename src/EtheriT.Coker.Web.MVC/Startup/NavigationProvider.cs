using DevExpress.DataProcessing.ExtractStorage;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Permissions;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using EtheriT.Coker.Web.MVC.Common;
using EtheriT.Coker.Web.MVC.Security.Permissions;

namespace EtheriT.Coker.Web.MVC.Startup
{
    public class NavigationProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoginUserData loginUserData;
        private readonly PermissionStateStore permissionStateStore;
        private readonly IPermissionsAppService permissionsAppService;
        private readonly IWebsiteApplication websiteApplication;
        private readonly IMemoryCache memoryCache;
        public NavigationProvider(LoginUserData loginUserData,
            IPermissionsAppService permissionsAppService,
            IHttpContextAccessor httpContextAccessor,
            IWebsiteApplication websiteApplication,
            PermissionStateStore permissionStateStore,
            IMemoryCache memoryCache
        )
        {
            this.loginUserData = loginUserData;
            this.permissionsAppService = permissionsAppService;
            _httpContextAccessor = httpContextAccessor;
            this.websiteApplication = websiteApplication;
            this.memoryCache = memoryCache;
            this.permissionStateStore = permissionStateStore;
        }
        public async Task<Site> getMenus()
        {
            Site site = new Site
            {
                Title = await loginUserData.GetWebsiteName(),
                WebRootLink = await loginUserData.GetWebsiteUrl(),
                OrgName = await loginUserData.GetWebsiteOrgName(),
                Jobs = new List<JobMenu> {
                    new JobMenu{
                        PageName="Dashboard",
                        Title="儀表板",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="public",
                        CollapseId=""
                    },
                    new JobMenu{
                        PageName="OrderManagement",
                        Title="訂單管理",
                        Icon="receipt_long",
                        CollapseId="#OrderManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="OrderList",
                                Title="訂單列表",
                                Controller="OrderManagement",
                                Action="Index",
                                Icon="",
                            },new JobMenu{
                                PageName="Report",
                                Title="列印檢貨單",
                                Controller="Report",
                                Action="R001",
                                Icon="",
                                IsView=false
                            }
                        }
                    },
                    new JobMenu{
                        PageName="ProductManagement",
                        Title="商品管理",
                        Icon="local_mall",
                        CollapseId="#ProductManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="ProductList",
                                Title="商品列表",
                                Controller="ProductManagement",
                                Action="ProductList",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="TechnicalCertificate",
                                Title="技術證照",
                                Controller="ProductManagement",
                                Action="TechnicalCertificate",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="SpecSetting",
                                Title="規格設定",
                                Controller="ProductManagement",
                                Action="SpecSetting",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="SaleQuantityStaging",
                                Title="可銷售量警示",
                                Controller="ProductManagement",
                                Action="SaleQuantityStaging",
                                Icon="",
                            }
                        }
                    },
                    new JobMenu{
                        PageName="PageManagement",
                        Title="頁面管理",
                        Icon="web",
                        CollapseId="#PageManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="CustomPage",
                                Title="自訂頁面",
                                Controller="Page",
                                Action="Index",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="ComponerManager",
                                Title="元件管理",
                                Controller="Page",
                                Action="ComponerManager",
                                Icon="",
                            }
                        }
                    },
                    new JobMenu{
                        PageName="ContentManagement",
                        Title="內容管理",
                        Icon="sticky_note_2",
                        CollapseId="#ContentManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="Tag",
                                Title="標籤設定",
                                Controller="ContentManagement",
                                Action="Tag",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="Directory",
                                Title="目錄管理",
                                Controller="ContentManagement",
                                Action="Directory",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="Article",
                                Title="文章管理",
                                Controller="ContentManagement",
                                Action="Article",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="Marquee",
                                Title="跑馬燈訊息",
                                Controller="ContentManagement",
                                Action="Marquee",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="ContactUs",
                                Title="聯絡我們",
                                Controller="ContentManagement",
                                Action="ContactUs",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="SettingCSS",
                                Title="自訂CSS",
                                Controller="ContentManagement",
                                Action="SettingCSS",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="FileManagement",
                                Title="檔案管理",
                                Controller="FileManagement",
                                Action="Index",
                                Icon="",
                            }
                        }
                    },
                    new JobMenu{
                        PageName="AdvertisementManagement",
                        Title="廣告管理",
                        Icon="campaign",
                        CollapseId="#AdvertisementManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="CustomAd",
                                Title="自訂廣告",
                                Controller="AdvertisementManagement",
                                Action="CustomAd",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="EnterAd",
                                Title="進入廣告",
                                Controller="AdvertisementManagement",
                                Action="EnterAd",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="RightSideAd",
                                Title="右側浮動廣告",
                                Controller="AdvertisementManagement",
                                Action="RightSideAd",
                                Icon="",
                            },
                        }
                    },
                    new JobMenu{
                        PageName="NewsletterManagement",
                        Title="電子報管理",
                        Icon="mail",
                        CollapseId="#NewsletterManagement",
                        jobItemModels= new List<JobMenu>{
                            new JobMenu{
                                PageName="Recipient",
                                Title="收件人設定",
                                Controller="Newsletter",
                                Action="Recipient",
                                Icon=""
                            },new JobMenu{
                                PageName="Newsletter",
                                Title="電子報內容設定",
                                Controller="Newsletter",
                                Action="Index",
                                Icon=""
                            }
                        }
                    },
                    new JobMenu{
                        PageName="MemberManagement",
                        Title="管理者管理",
                        Icon="people_alt",
                        CollapseId="#MemberManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="ManagerList",
                                Title="管理者名單",
                                Controller="MemberManagement",
                                Action="ManagerList",
                                Icon=""
                            },new JobMenu{
                                PageName="UserData",
                                Title="個人資料修改",
                                Controller="MemberManagement",
                                Action="SelfData",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="PowerCtrl",
                                Title="權限控制",
                                Controller="PowerManagement",
                                Action="Index",
                                Icon=""
                            }
                        }
                    },
                    new JobMenu{
                        PageName="StoreSettings",
                        Title="商店管理",
                        Icon="store",
                        CollapseId="#StoreSettings",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="StoreSet",
                                Title="商店設定",
                                Controller="StoreSettings",
                                Action="Index",
                                Icon="shopping_cart",
                            },
                            new JobMenu{
                                PageName="FreightSettings",
                                Title="運費設定",
                                Controller="StoreSettings",
                                Action="FreightSettings",
                                Icon="local_shipping",
                            },
                            new JobMenu{
                                PageName="PaymentSettings",
                                Title="付款設定",
                                Controller="StoreSettings",
                                Action="PaymentSettings",
                                Icon="credit_card",
                            },
                            new JobMenu{
                                PageName="MarketingSettings",
                                Title="行銷活動",
                                Controller="StoreSettings",
                                Action="MarketingSettings",
                                Icon="confirmation_number",
                            },
                            new JobMenu{
                                PageName="LogisticsSettings",
                                Title="物流設定",
                                Controller="StoreSettings",
                                Action="LogisticsSettings",
                                Icon="hand_package",
                            }
                        }
                    },
                    new JobMenu{ 
                        PageName="Bonus",
                        Title="紅利管理",
                        Icon="redeem",
                        CollapseId="#BonusManagement",
                        Enable=false,
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="BonusSettings",
                                Title="紅利設定",
                                Controller="BonusManagement",
                                Action="Settings",
                                Icon="savings",
                                Enable=false
                            },
                            new JobMenu{
                                PageName="BonusTransaction",
                                Title="紅利異動",
                                Controller="BonusManagement",
                                Action="Transaction",
                                Icon="savings",
                                Enable=false
                            },new JobMenu{
                                PageName="BonusRecord",
                                Title="歷史紅利查詢",
                                Controller="BonusManagement",
                                Action="Record",
                                Icon="savings",
                                Enable=false
                            }
                        }
                    },
                    new JobMenu{
                        PageName="MemberData",
                        Title="會員管理",
                        Icon="diversity_1",
                        CollapseId="#MemberManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu
                            {
                                PageName="MemberSet",
                                Title="會員設定",
                                Controller="MemberManagement",
                                Action="Index",
                                Icon="",
                            },
                            new JobMenu {
                                PageName="MemberList",
                                Title="會員清單",
                                Controller="MemberManagement",
                                Action="MemberList",
                                Icon=""
                            },
                            new JobMenu {
                                PageName="MemberType",
                                Title="會員角色管理",
                                Controller="MemberManagement",
                                Action="MemberType",
                                Icon=""
                            },
                            new JobMenu {
                                PageName="UserType",
                                Title="使用者分群",
                                Controller="MemberManagement",
                                Action="UserType",
                                Icon=""
                            }
                        }
                    },
                    new JobMenu{
                        PageName="CustSearch",
                        Title="搜尋設定",
                        Controller="SearchManagement",
                        Action="CustSearch",
                        Icon="search"
                    },

                    new JobMenu{
                        PageName="Layout",
                        Title="版型設定",
                        Controller="Template",
                        Icon="palette",
                        CollapseId="#Layout",
                        jobItemModels = new List<JobMenu>{
                            new JobMenu{
                                PageName="HeaderSettings",
                                Title="版頭設定",
                                Controller="Template",
                                Action="HeaderSettings",
                                Icon="toolbar"
                            },new JobMenu{
                                PageName="FooterSettings",
                                Title="頁尾設定",
                                Controller="Template",
                                Action="FooterSettings",
                                Icon="bottom_navigation"
                            }
                        }
                    },
                    new JobMenu{
                        PageName="TypographyTheme",
                        Title="版型主題",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="palette",
                        CollapseId="",
                    },
                    new JobMenu{
                        PageName="SystemManagement",
                        Title="系統管理",
                        Icon="settings",
                        CollapseId="#SystemManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="WebData",
                                Title="網站資料",
                                Controller="SystemManagement",
                                Action="Index",
                                Icon="wysiwyg",
                            },
                            new JobMenu{
                                PageName="SEO",
                                Title="SEO設定",
                                Controller="SystemManagement",
                                Action="SEO",
                                Icon="manage_search",
                            },
                            new JobMenu{
                                PageName="Theme",
                                Title="後台配色設定",
                                Controller="Theme",
                                Action="Index",
                                Icon="web"
                            },
                            new JobMenu{
                                PageName="AuditLogs",
                                Title="操作紀錄",
                                Controller="AuditLogs",
                                Action="Index",
                                Icon="psychology"
                            },
                            new JobMenu{
                                PageName="SMTP",
                                Title="SMTPServer",
                                Controller="StoreSettings",
                                Action="SMTPSettings",
                                Icon="forward_to_inbox"
                            }
                        }
                    },new JobMenu{
                        PageName="RemoteManagement",
                        Title="網站瀏覽報告",
                        Icon="manage_search",
                        CollapseId="#RemoteManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="RemoteAll",
                                Title="全站瀏覽數量",
                                Controller="Remote",
                                Action="Page",
                                Icon=""
                            },
                            new JobMenu{
                                PageName="RemotePage",
                                Title="頁面瀏覽數量",
                                Controller="Remote",
                                Action="Index",
                                Icon=""
                            },
                            new JobMenu
                            {
                                PageName="FlowSizesList",
                                Title="使用流量統計",
                                Controller="FlowSize",
                                Action="Index",
                                Icon=""
                            }
                        }
                    }
                }
            };
            return site;
        }
        public async Task SetPower(Site site)
        {
            WebsiteLevelEnum level = await loginUserData.GetWebsiteLevel();
            List<JobMenu> seting = new List<JobMenu>();
            switch (level)
            {
                case WebsiteLevelEnum.簡約:
                    seting.AddRange(new List<JobMenu>
                    {
                        new JobMenu{
                            PageName="NewsletterManagement",
                            Enable=false,
                        },new JobMenu
                        {
                            PageName="ProductManagement",
                            Enable=false
                        },new JobMenu{
                            PageName="OrderManagement",
                            Enable=false,
                        },new JobMenu{
                                PageName="OrderList",
                                Enable=false,
                        },new JobMenu{
                            PageName="ManagerList",
                            Enable=false
                        },new JobMenu
                        {
                            PageName="MemberData",
                            Enable=false
                        }, new JobMenu
                        {
                            PageName="StoreSettings",
                            Enable=false,
                        },new JobMenu{
                            PageName="CustSearch",
                            Enable=false,
                        },
                        new JobMenu{
                            PageName="TypographyTheme",
                            Enable=false
                        },
                        new JobMenu{
                            PageName="MemberSet",
                            Enable=false
                        },new JobMenu
                        {
                            PageName = "Report",
                            Enable = false
                        },new JobMenu
                        {
                            PageName = "Bonus",
                            Enable = false
                        },new JobMenu
                        {
                            PageName = "SaleQuantityStaging",
                            Enable = false
                        }
                    });
                    break;
                case WebsiteLevelEnum.形象:
                    seting.AddRange(new List<JobMenu> {
                        new JobMenu{
                            PageName="OrderManagement",
                            Enable=false,
                        },new JobMenu{
                                PageName="OrderList",
                                Enable=false,
                        },new JobMenu{
                            PageName="ManagerList",
                            Enable=false
                        },new JobMenu
                        {
                            PageName="MemberData",
                            Enable=false
                        }, new JobMenu
                        {
                            PageName="FreightSettings",
                            Enable=false,
                        }, new JobMenu
                        {
                            PageName="PaymentSettings",
                            Enable=false,
                        }, new JobMenu
                        {
                            PageName="BonusSettings",
                            Enable=false,
                        }, new JobMenu
                        {
                            PageName="MarketingSettings",
                            Enable=false,
                        },new JobMenu{
                            PageName="CustSearch",
                            Enable=false,
                        },
                        new JobMenu{
                            PageName="TypographyTheme",
                            Enable=false
                        },
                        new JobMenu{
                            PageName="MemberSet",
                            Enable=false
                        },new JobMenu
                        {
                            PageName = "Report",
                            Enable = false
                        },new JobMenu
                        {
                            PageName = "Bonus",
                            Enable = false
                        },new JobMenu
                        {
                            PageName = "SaleQuantityStaging",
                            Enable = false
                        }
                    });
                    break;
                case WebsiteLevelEnum.會員:
                    seting.AddRange(new List<JobMenu> {
                        new JobMenu
                        {
                            PageName="OrderManagement",
                            Enable=false
                        },new JobMenu{
                                PageName="OrderList",
                                Enable=false,
                        },new JobMenu{
                            PageName="ManagerList",
                            Enable=false
                        },new JobMenu
                        {
                            PageName="StoreSettings",
                            Enable=false
                        },new JobMenu{
                            PageName="CustSearch",
                            Enable=false,
                        },
                        new JobMenu{
                            PageName="TypographyTheme",
                            Enable=false
                        },new JobMenu
                        {
                            PageName = "Report",
                            Enable = false
                        },new JobMenu
                        {
                            PageName = "Bonus",
                            Enable = false
                        },new JobMenu
                        {
                            PageName = "SaleQuantityStaging",
                            Enable = false
                        }
                    });
                    break;
                case WebsiteLevelEnum.購物:
                    seting.AddRange(new List<JobMenu> {
                        new JobMenu
                        {
                            PageName = "TypographyTheme",
                            Enable = false
                        }
                    });
                    break;
            }
            SetJobs(site.Jobs, seting);
        }
        public async Task SetWebsite(Site site) {
            var data = await permissionsAppService.GetWebsitePermissions();
            if (data != null) {
                List<JobMenu> jobs = new List<JobMenu>();
                data.ForEach(x => {
                    string name = x.Name;
                    JobMenu? job = jobs.Find(e => e.PageName == name);
                    if (job == null)
                    {
                        job = new JobMenu
                        {
                            PageName = name,
                            Enable = x.IsGranted,
                        };
                        jobs.Add(job);
                    }
                });
                SetJobs(site.Jobs, jobs);
            }
        }
        public async Task setUserJob(Site site)
        {
            var userId = await loginUserData.GetUserId();
            var websiteId = await loginUserData.GetWebsiteId();
            ThePermission ThePermission = new ThePermission();
            BonusPermission bonusPermission = new BonusPermission();
            ThePermission.Initable = false;
            ThePermission.superManager = await permissionsAppService.IsPowerUserPermissions();
            ThePermission.systemManager = await loginUserData.isSystemUser();
            if (ThePermission.superManager)
            {
                site.Jobs.ForEach(x =>
                {
                    if (x.Enable)
                    {
                        x.CanRemove = true;
                        x.CanUpdate = true;
                        x.CanVisble = true;
                        x.CanCreate = true;
                        if (x.jobItemModels != null)
                        {
                            x.jobItemModels.ForEach(s =>
                            {
                                if (s.Enable)
                                {
                                    s.CanRemove = true;
                                    s.CanUpdate = true;
                                    s.CanVisble = true;
                                    s.CanCreate = true;
                                }
                            });
                        }
                    }
                });

            }
            else
            {
                var data = await permissionsAppService.GetLoginUserPermissions();
                if (data != null)
                {
                    List<JobMenu> jobs = new List<JobMenu>();
                    data.ForEach(x =>
                    {
                        var jobSplit = x.Name.Split(".");
                        if (jobSplit.Length <= 1) return;
                        string name = jobSplit[0];
                        string type = jobSplit[1];
                        JobMenu? job = jobs.Find(e => e.PageName == name);
                        if (job == null)
                        {
                            job = new JobMenu
                            {
                                PageName = name,
                                Enable = true,
                            };
                            jobs.Add(job);
                        }
                        switch (type)
                        {
                            case "View":
                                job.CanVisble = x.IsGranted;
                                break;
                            case "Edit":
                                job.CanUpdate = x.IsGranted;
                                break;
                            case "Create":
                                job.CanCreate = x.IsGranted;
                                break;
                            case "Delete":
                                job.CanRemove = x.IsGranted;
                                break;
                            default:
                                job.Enable = false;
                                break;
                        }
                    });
                    SetJobs(site.Jobs, jobs);
                }
            }
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request.RouteValues.ContainsKey("action"))
            {
                string ControllerName = (_httpContextAccessor.HttpContext.Request.RouteValues["controller"] ?? "").ToString();
                string ActionName = (_httpContextAccessor.HttpContext.Request.RouteValues["action"] ?? "").ToString();
                JobMenu? item = null;
                JobMenu? Bonus = FindJob(site.Jobs, "BonusManagement", "Settings");
                if (Bonus != null)
                {
                    bonusPermission.CanExe = Bonus.Enable;
                    bonusPermission.CanEdit = Bonus.CanUpdate;
                }
                else {
                    bonusPermission.CanExe = false;
                    bonusPermission.CanEdit = false;
                }

                site.Jobs.ForEach(e =>
                {
                    if (e.Controller == ControllerName && e.Action == ActionName) item = e;
                    else if (e.jobItemModels != null)
                    {
                        var n = e.jobItemModels.Find(x => x.Controller == ControllerName && x.Action == ActionName);
                        if (n != null) item = n;
                    }
                    if (item != null) return;
                });
                if (item != null)
                {
                    ThePermission.CanVisble = item.CanVisble;
                    ThePermission.CanUpdate = item.CanUpdate;
                    ThePermission.CanRemove = item.CanRemove;
                    ThePermission.CanCreate = item.CanCreate;
                }
                else
                {
                    ThePermission.CanVisble = false;
                    ThePermission.CanUpdate = false;
                    ThePermission.CanRemove = false;
                    ThePermission.CanCreate = false;
                }
            }
            ThePermission.Initable = true;

            permissionStateStore.Set(websiteId,userId, ThePermission);
            permissionStateStore.Set(websiteId, userId, bonusPermission);
            var ctx = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("PermissionStateStore requires an active HttpContext.");
                ctx.Items[CokerContextKeys.HasManySystem] = await websiteApplication.hasManySystem();
        }
        public JobMenu? FindJob(List<JobMenu> jobs, string Controller, string Action)
        {
            JobMenu? m = null;

            jobs.ForEach(e =>
            {
                if (m != null) return;
                if (e.Controller == Controller && e.Action == Action) m = e;
                if (e.jobItemModels != null)
                {
                    m = FindJob(e.jobItemModels, Controller, Action);
                }
            });
            return m;
        }
        private void SetJobs(List<JobMenu> jobs, List<JobMenu> seting)
        {
            if (seting.Count() == 0) return;
            jobs.ForEach(job =>
            {
                JobMenu? menu = seting.Find(e => e.PageName == job.PageName);
                if (menu != null)
                {
                    if (job.Enable || menu.Enable)
                    {
                        job.Enable = menu.Enable;
                        job.CanVisble = menu.CanVisble;
                        job.CanUpdate = menu.CanUpdate;
                        job.CanRemove = menu.CanRemove;
                        job.CanCreate = menu.CanCreate;
                    }
                    seting.Remove(menu);
                }
                if (job.jobItemModels != null && job.jobItemModels.Count() > 0)
                    SetJobs(job.jobItemModels, seting);
            });
        }
    }
}
