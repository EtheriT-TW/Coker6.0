using EtheriT.Coker.Application.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Web.Public.Views.Shared.Components.Header;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Footer
{
    public class Footer : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            FooterViewModel footerViewModel = new FooterViewModel
            {
                footerViewModels = new List<FooterViewModel>
                {
                    new FooterViewModel {Title="商品分類",Link="", footerViewModels = new List<FooterViewModel>{
                            new FooterViewModel {Title="微電腦馬桶座", Link=""},
                            new FooterViewModel {Title="馬桶", Link="Toilet"},
                            new FooterViewModel {Title="面盆", Link=""},
                            new FooterViewModel {Title="便斗", Link=""},
                            new FooterViewModel {Title="龍頭", Link=""},
                            new FooterViewModel {Title="配件", Link=""},
                            new FooterViewModel {Title="浴缸", Link=""},
                            new FooterViewModel {Title="三機", Link=""},
                            new FooterViewModel {Title="無障礙設備", Link=""},
                            new FooterViewModel {Title="線上型錄", Link="Catalog"},
                            new FooterViewModel {Title="清倉品", Link=""},
                        },
                    },
                    new FooterViewModel {Title="關於Derek",Link="", footerViewModels = new List<FooterViewModel>{
                            new FooterViewModel {Title="企業理念", Link=""},
                            new FooterViewModel {Title="企業沿革", Link=""},
                            new FooterViewModel {Title="企業設備", Link=""},
                            new FooterViewModel {Title="媒體專區", Link=""},
                            new FooterViewModel {Title="品牌故事", Link=""},
                            new FooterViewModel {Title="展示中心", Link="ExhibitionCenter"},
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
                            new FooterViewModel {Title="聯絡我們", Link="Contact"},
                        }
                    }
                }
            };

            return View(footerViewModel);
        }
    }
}
