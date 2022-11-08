using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar
{
    public class Sidebar : ViewComponent
    {
        public Sidebar()
        {

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            Site site = new Site
            {
                Title = "德瑞克",
                Jobs = new List<JobMenu> {
                    new JobMenu{
                        PageName="Dashboard",
                        Title="儀表板",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="space_dashboard",
                        CollapseId=""
                    },
                    new JobMenu{
                        PageName="OrderManagement",
                        Title="訂單管理",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="receipt_long",
                        CollapseId="#OrderManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="SingleOrder",
                                Title="單筆訂單畫面",
                                Controller="Dashboard",
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
                                Controller="Dashboard",
                                Action="Index",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="ProductTag",
                                Title="商品標籤",
                                Controller="Dashboard",
                                Action="Index",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="TechnicalCertificate",
                                Title="技術證照",
                                Controller="Dashboard",
                                Action="Index",
                                Icon="",
                            }
                        }
                    },
                    new JobMenu{
                        PageName="MemberManagement",
                        Title="會員管理",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="people_alt",
                        CollapseId="#MemberManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="MemberData",
                                Title="會員資料",
                                Controller="Dashboard",
                                Action="Index",
                                Icon="",
                            }
                        }
                    },
                    new JobMenu{
                        PageName="PageManagement",
                        Title="頁面管理",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="web",
                        CollapseId="#PageManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="CustomPage",
                                Title="自訂頁面",
                                Controller="Dashboard",
                                Action="Index",
                                Icon="",
                            }
                        }
                    },
                    new JobMenu{
                        PageName="ContentManagement",
                        Title="內容管理",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="sticky_note_2",
                        CollapseId="#ContentManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="LatestNews",
                                Title="最新消息",
                                Controller="Dashboard",
                                Action="Index",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="MarqueeMessage",
                                Title="跑馬燈訊息",
                                Controller="Dashboard",
                                Action="Index",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="EnterAd",
                                Title="進入廣告",
                                Controller="Dashboard",
                                Action="Index",
                                Icon="",
                            },
                            new JobMenu{
                                PageName="RightSideAd",
                                Title="右側浮動廣告",
                                Controller="Dashboard",
                                Action="Index",
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
                                Controller="Dashboard",
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
                        Controller="Dashboard",
                        Action="Index",
                        Icon="settings",
                        CollapseId="#SystemManagement",
                        jobItemModels= new List<JobMenu> {
                            new JobMenu{
                                PageName="WebData",
                                Title="網站資料",
                                Controller="Dashboard",
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
