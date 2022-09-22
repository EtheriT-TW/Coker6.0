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
                LogoImageUrl = "images/derek_logo.png",
                menuItemModels = new List<MenuItem.MenuItemModel> {
                    new MenuItem.MenuItemModel {
                        Title = "關於Derek",
                        menuItemModels = new List<MenuItem.MenuItemModel>{
                            new MenuItem.MenuItemModel {Title="關於Derek", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="關於Derek"},
                                    new MenuItem.MenuItemModel {Title="企業沿革"},
                                    new MenuItem.MenuItemModel {Title="企業設備"},
                                    new MenuItem.MenuItemModel {Title="品牌故事"},
                                } 
                            },
                            new MenuItem.MenuItemModel {Title="實績列舉", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="近期建案實績"},
                                    new MenuItem.MenuItemModel {Title="公共工程"},
                                }
                            },
                            new MenuItem.MenuItemModel {Title="標章認證", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="省水標章"},
                                    new MenuItem.MenuItemModel {Title="環保標章"},
                                    new MenuItem.MenuItemModel {Title="能源分級"},
                                    new MenuItem.MenuItemModel {Title="ISO/CNS"},
                                    new MenuItem.MenuItemModel {Title="應施檢驗"},
                                    new MenuItem.MenuItemModel {Title="MIT/LF無鉛"},
                                }
                            },
                        }
                    },new MenuItem.MenuItemModel {
                        Title = "Derek商品",
                        Description = "圖片說明文字",
                        imageUrl = "images/bpic-03.jpg",
                        menuItemModels = new List<MenuItem.MenuItemModel>{
                            new MenuItem.MenuItemModel {Title="商品分類", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="微電腦馬桶座"},
                                    new MenuItem.MenuItemModel {Title="馬桶"},
                                    new MenuItem.MenuItemModel {Title="面盆"},
                                    new MenuItem.MenuItemModel {Title="便斗"},
                                    new MenuItem.MenuItemModel {Title="龍頭"},
                                }
                            },
                            new MenuItem.MenuItemModel {Title="", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="配件"},
                                    new MenuItem.MenuItemModel {Title="浴缸"},
                                    new MenuItem.MenuItemModel {Title="三機"},
                                    new MenuItem.MenuItemModel {Title="無障礙設施"},
                                    new MenuItem.MenuItemModel {Title="線上型錄"},
                                    new MenuItem.MenuItemModel {Title="清倉品"},
                                }
                            },
                            new MenuItem.MenuItemModel {Title="", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="使用須知"},
                                    new MenuItem.MenuItemModel {Title="購買諮詢服務"}
                                }
                            },
                        }
                    },new MenuItem.MenuItemModel {
                        Title = "最新消息",
                        Description = "圖片說明文字",
                        imageUrl = "images/bpic-03.jpg",
                        menuItemModels = new List<MenuItem.MenuItemModel>{
                            new MenuItem.MenuItemModel {Title="最新消息", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="人才招募"},
                                    new MenuItem.MenuItemModel {Title="媒體專區"},
                                    new MenuItem.MenuItemModel {Title="粉絲專業"},
                                }
                            },
                        }
                    },new MenuItem.MenuItemModel {
                        Title = "銷售據點",
                        Description = "圖片說明文字",
                        imageUrl = "images/bpic-03.jpg",
                        menuItemModels = new List<MenuItem.MenuItemModel>{
                            new MenuItem.MenuItemModel {Title="銷售據點", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="龍頭分公司"},
                                    new MenuItem.MenuItemModel {Title="花東總經銷-百健行"},
                                    new MenuItem.MenuItemModel {Title="經銷據點"},
                                }
                            },
                            new MenuItem.MenuItemModel {Title="展示中心", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="台北"},
                                    new MenuItem.MenuItemModel {Title="新竹"},
                                    new MenuItem.MenuItemModel {Title="台中"},
                                    new MenuItem.MenuItemModel {Title="台南"},
                                    new MenuItem.MenuItemModel {Title="高雄"},
                                }
                            },
                        }
                    },new MenuItem.MenuItemModel {
                        Title = "客戶服務",
                        Description = "圖片說明文字",
                        imageUrl = "images/bpic-03.jpg",
                        menuItemModels = new List<MenuItem.MenuItemModel>{
                            new MenuItem.MenuItemModel {Title="售前服務", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="購買諮詢服務"},
                                }
                            },
                            new MenuItem.MenuItemModel {Title="更多服務", menuItemModels = new List<MenuItem.MenuItemModel>{
                                    new MenuItem.MenuItemModel {Title="清潔小幫手"},
                                    new MenuItem.MenuItemModel {Title="維修服務"},
                                    new MenuItem.MenuItemModel {Title="常見問題"},
                                    new MenuItem.MenuItemModel {Title="使用須知"},
                                    new MenuItem.MenuItemModel {Title="聯絡我們"},
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
