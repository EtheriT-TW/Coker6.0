using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Header
{
    public class Header : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            HeaderViewModel headerViewModel = new HeaderViewModel
            {
                Title = "德瑞克",
                LogoImageUrl = "/images/derek_logo.png",
                menuItemModels = new List<MenuItem.MenuItemModel> {
                    new MenuItem.MenuItemModel {
                        Title = "關於Derek",
                        menuItemModels = new List<MenuItem.MenuItemModel>{
                            new MenuItem.MenuItemModel {Title="關於Derek", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="關於Derek", Link=""},
                                    new MenuItem.MenuItemModel {Title="企業沿革", Link=""},
                                    new MenuItem.MenuItemModel {Title="企業設備", Link=""},
                                    new MenuItem.MenuItemModel {Title="品牌故事", Link=""},
                                }
                            },
                            new MenuItem.MenuItemModel {Title="實績列舉", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="近期建案實績", Link=""},
                                    new MenuItem.MenuItemModel {Title="公共工程", Link=""},
                                }
                            },
                            new MenuItem.MenuItemModel {Title="標章認證", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="省水標章", Link=""},
                                    new MenuItem.MenuItemModel {Title="環保標章", Link=""},
                                    new MenuItem.MenuItemModel {Title="能源分級", Link=""},
                                    new MenuItem.MenuItemModel {Title="ISO/CNS", Link=""},
                                    new MenuItem.MenuItemModel {Title="應施檢驗", Link=""},
                                    new MenuItem.MenuItemModel {Title="MIT/LF無鉛", Link=""},
                                }
                            },
                        }
                    },new MenuItem.MenuItemModel {
                        Title = "Derek商品",
                        Description = "線上型錄",
                        imageUrl = "/images/mu_0.jpg",
                        imageLink = "/Catalog",
                        menuItemModels = new List<MenuItem.MenuItemModel>{
                            new MenuItem.MenuItemModel {Title="商品分類", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="微電腦馬桶座", Link=""},
                                    new MenuItem.MenuItemModel {Title="馬桶", Link="Toilet"},
                                    new MenuItem.MenuItemModel {Title="面盆", Link=""},
                                    new MenuItem.MenuItemModel {Title="便斗", Link=""},
                                    new MenuItem.MenuItemModel {Title="龍頭", Link=""},
                                    new MenuItem.MenuItemModel {Title="配件", Link=""},
                                    new MenuItem.MenuItemModel {Title="浴缸", Link=""},
                                    new MenuItem.MenuItemModel {Title="三機", Link=""},
                                    new MenuItem.MenuItemModel {Title="無障礙設施", Link=""},
                                    new MenuItem.MenuItemModel {Title="線上型錄", Link="Catalog"},
                                    new MenuItem.MenuItemModel {Title="清倉品", Link=""},
                                }
                            },
                            new MenuItem.MenuItemModel {Title="使用須知", menuItemModels = new List<MenuItem.MenuItemModel>{}},
                            new MenuItem.MenuItemModel {Title="購買諮詢服務", menuItemModels = new List<MenuItem.MenuItemModel>{}},
                        }
                    },new MenuItem.MenuItemModel {
                        Title = "最新消息",
                        Description = "年度精彩事",
                        imageUrl = "/images/mu_1.jpg",
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
                          imageUrl = "/images/mu_2.jpg",
                          menuItemModels = new List<MenuItem.MenuItemModel>{
                            new MenuItem.MenuItemModel {Title="銷售據點", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="龍頭分公司", Link=""},
                                    new MenuItem.MenuItemModel {Title="花東總經銷-百健行", Link=""},
                                    new MenuItem.MenuItemModel {Title="經銷據點", Link=""},
                                }
                            },
                            new MenuItem.MenuItemModel {Title="展示中心", Link="ExhibitionCenter", menuItemModels = new List<MenuItem.MenuItemModel>{
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
                          imageUrl = "/images/mu_3.jpg",
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
                                    new MenuItem.MenuItemModel {Title="聯絡我們", Link="Contact"},
                                }
                            },
                        }
                      }
                }
            };
            return View(headerViewModel);
        }
    }
}
