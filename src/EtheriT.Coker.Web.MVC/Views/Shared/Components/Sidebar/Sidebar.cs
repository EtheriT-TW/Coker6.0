using EtheriT.Coker.Application;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar
{
    public class Sidebar : ViewComponent
    {
        private readonly LoginUserData loginUserData;
        public Sidebar(LoginUserData loginUserData)
        {
            this.loginUserData = loginUserData;
        }
        public async Task<IViewComponentResult> InvokeAsync()
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
                        CollapseId=""
                    },
                    new JobMenu{
                        PageName="OrderManagement",
                        Title="訂單管理",
                        Controller="OrderManagement",
                        Action="Index",
                        Icon="receipt_long",
                        CollapseId="#OrderManagement",
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
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="ProductList",
                                Title="商品列表",
                                Controller="ProductManagement",
                                Action="ProductList",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="ProductTag",
                                Title="商品標籤",
                                Controller="ProductManagement",
                                Action="ProductTag",
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
                        PageName="MemberManagement",
                        Title="會員管理",
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
                                PageName="NewArticle",
                                Title="新增文章",
                                Controller="ContentManagement",
                                Action="NewArticle",
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
                        PageName="TypographyTheme",
                        Title="版型主題",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="palette",
                        CollapseId=""
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
                                Icon="",
                            },
                            new JobMenu{
                                PageName="Theme",
                                Title="後台配色設定",
                                Controller="Theme",
                                Action="Index",
                                Icon=""
                            }
                        }
                    }
                }
            };
            return View(site);
        }
    }
}
