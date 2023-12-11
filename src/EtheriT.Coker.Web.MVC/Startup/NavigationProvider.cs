using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Permissions;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EtheriT.Coker.Web.MVC.Startup
{
    public class NavigationProvider
    {
        private readonly LoginUserData loginUserData;
        private readonly IPermissionsAppService permissionsAppService;
        public NavigationProvider(LoginUserData loginUserData, IPermissionsAppService permissionsAppService)
        {
            this.loginUserData = loginUserData;
            this.permissionsAppService = permissionsAppService;
        }
        public async Task<Site> getMenus()
        {
            Site site = new Site
            {
                Title = await loginUserData.GetWebsiteName(),
                Jobs = new List<JobMenu> {
                    new JobMenu{
                        PageName="Dashboard",
                        Title="儀表板",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="public",
                        CollapseId="",
                        Enable=false
                    },
                    new JobMenu{
                        PageName="OrderManagement",
                        Title="訂單管理",
                        Controller="OrderManagement",
                        Action="Index",
                        Icon="receipt_long",
                        CollapseId="#OrderManagement",
                        Enable=false,
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="OrderList",
                                Title="訂單列表",
                                Controller="OrderManagement",
                                Action="Index",
                                Icon="",
                            }
                        }
                    },
                    new JobMenu{
                        PageName="ProductManagement",
                        Title="商品管理",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="local_mall",
                        CollapseId="#ProductManagement",
                        Enable=false,
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
                            }
                        }
                    },
                    new JobMenu{
                        PageName="PageManagement",
                        Title="頁面管理",
                        Controller="Page",
                        Action="Index",
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
                        Controller="ContentManagement",
                        Action="Index",
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
                                PageName="EnterAd",
                                Title="進入廣告",
                                Controller="ContentManagement",
                                Action="EnterAd",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="RightSideAd",
                                Title="右側浮動廣告",
                                Controller="ContentManagement",
                                Action="RightSideAd",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="ContactUs",
                                Title="聯絡我們",
                                Controller="ContentManagement",
                                Action="ContactUs",
                                Icon="",
                                Enable=false
                            }
                        }
                    },
                    new JobMenu{
                        PageName="NewsletterManagement",
                        Title="電子報管理",
                        Controller="Newsletter",
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
                        Title="使用者管理",
                        Controller="MemberManagement",
                        Action="Index",
                        Icon="people_alt",
                        CollapseId="#MemberManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="MemberData",
                                Title="會員管理",
                                Controller="MemberManagement",
                                Action="Index",
                                Icon="",
                                Enable=false
                            },new JobMenu{
                                PageName="ManagerList",
                                Title="使用者名單",
                                Controller="MemberManagement",
                                Action="ManagerList",
                                Icon="",
                                Enable=false
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
                        Title="商店設定",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="store",
                        CollapseId="#StoreSettings",
                        Enable=false,
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="FreightSettings",
                                Title="運費設定",
                                Controller="StoreSettings",
                                Action="Index",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="PaymentSettings",
                                Title="付款設定",
                                Controller="Dashboard",
                                Action="Index",
                                Icon="",
                            }
                        }
                    },
                    new JobMenu{
                        PageName="CustSearch",
                        Title="搜尋設定",
                        Controller="SearchManagement",
                        Action="CustSearch",
                        Icon="search",
                        Enable=false,
                    },
                    new JobMenu{
                        PageName="TypographyTheme",
                        Title="版型主題",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="palette",
                        CollapseId="",
                        Enable=false
                    },
                    new JobMenu{
                        PageName="SystemManagement",
                        Title="系統管理",
                        Controller="SystemManagement",
                        Action="Index",
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
                case WebsiteLevelEnum.會員:
                case WebsiteLevelEnum.購物:
                    seting.AddRange(new List<JobMenu> {
                        new JobMenu{
                            PageName="Dashboard",
                            Enable=true
                        },
                        new JobMenu{
                            PageName="OrderManagement",
                            Enable=true,
                        },
                        new JobMenu
                        {
                            PageName="ProductManagement",
                            Enable=true
                        },new JobMenu{
                            PageName="OrderManagement",
                            Enable=true,
                        }, new JobMenu
                        {
                            PageName="ProductManagement",
                            Enable=true
                        },new JobMenu{
                            PageName="ContactUs",
                            Enable=true
                        }, new JobMenu{
                             PageName="MemberData",
                             Enable=true
                        },new JobMenu{
                            PageName="ManagerList",
                            Enable=true
                        }, new JobMenu
                        {
                            PageName="StoreSettings",
                            Enable=true,
                        },new JobMenu{
                            PageName="CustSearch",
                            Enable=true,
                        },
                        new JobMenu{
                            PageName="TypographyTheme",
                            Enable=true
                        }
                    });
                    break;
            }
            SetJobs(site.Jobs, seting);
        }
        public async Task setUserJob(Site site) {
            var data = await permissionsAppService.GetLoginUserPermissionsByView();
            if (data != null)
            {
                List<JobMenu> jobs= new List<JobMenu>();
                data.ForEach(x => { 
                    string name = x.Name.Split(".")[0];
                    jobs.Add(new JobMenu {
                        PageName = name,
                        Enable = x.IsGranted
                    });
                });
                SetJobs(site.Jobs, jobs);
            }
        }
        private void SetJobs(List<JobMenu> jobs, List<JobMenu> seting)
        {
            if (seting.Count() == 0) return;
            jobs.ForEach(job =>
            {
                JobMenu? menu = seting.Find(e => e.PageName == job.PageName);
                if (menu != null) { 
                    job.Enable = menu.Enable;
                    seting.Remove(menu);
                }
                if (job.jobItemModels != null && job.jobItemModels.Count() > 0)
                    SetJobs(job.jobItemModels, seting);
            });
        }
    }
}
