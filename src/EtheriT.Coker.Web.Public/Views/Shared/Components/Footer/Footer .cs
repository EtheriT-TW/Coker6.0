
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.Public.Views.Shared.Components.Header;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Footer
{
    public class Footer : ViewComponent
    {
        private readonly IWebsiteApplication websiteApplication;
        private readonly IConfiguration Configuration;
        public Footer(
            IWebsiteApplication websiteApplication,
            IConfiguration Configuration
            )
        {
            this.websiteApplication = websiteApplication;
            this.Configuration = Configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var routeData = HttpContext.GetRouteData();
            var website = HttpContext.GetRouteData().Values["website"];
            if (website == null)
            {
                website = HttpContext.GetRouteData().Values["key"];
            }
            var website_str = website == null ? "" : website.ToString();
            var defaultData = await websiteApplication.GetDefaultData(siteId, website_str);

            FooterViewModel footerViewModel;

            switch (defaultData.Layout_Type)
            {
                case 1:
                    switch (defaultData.Id)
                    {
                        case 2:
                            footerViewModel = new FooterViewModel
                            {
                                footerViewModels = new List<FooterViewModel> {
                                    new FooterViewModel { Title = "產品專區", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "馬桶 ", Link = "/lcb/Apro" },
                                            new FooterViewModel { Title = "微電腦馬桶座", Link = "/lcb/Bpro" },
                                            new FooterViewModel { Title = "面盆&浴櫃組", Link = "/lcb/Cpro" },
                                            new FooterViewModel { Title = "浴室龍頭", Link = "/lcb/Dpro" },
                                            new FooterViewModel { Title = "無障礙設備", Link = "/lcb/Epro" },
                                            new FooterViewModel { Title = "浴缸", Link = "/lcb/Fpro" },
                                            new FooterViewModel { Title = "浴室配件", Link = "/lcb/Gpro" },
                                            new FooterViewModel { Title = "小便斗", Link = "/lcb/Hpro" },
                                            new FooterViewModel { Title = "停產專區", Link = "/lcb/discontinue" },
                                        }
                                    },
                                    new FooterViewModel { Title = "關於DEREK", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "品牌故事", Link = "/lcb/brand_story" },
                                            new FooterViewModel { Title = "標章認證", Link = "/lcb/mark_certification" },
                                            new FooterViewModel { Title = "實績案件", Link = "/lcb/actual_cases" },
                                            new FooterViewModel { Title = "最新消息", Link = "/lcb/news" },
                                            new FooterViewModel { Title = "加入Derek ", Link = "/lcb/recruiting" },
                                        }
                                    },
                                    new FooterViewModel { Title = "智慧科技", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "淨未來全自動智慧馬桶", Link = "/lcb/smart3plus" },
                                            new FooterViewModel { Title = "Clean極淨", Link = "/lcb/Pro_Clean" },
                                            new FooterViewModel { Title = "Speed極瞬", Link = "/lcb/Pro_Speed" },
                                            new FooterViewModel { Title = "Smart極智", Link = "/lcb/Pro_smart" },
                                            new FooterViewModel { Title = "Comfort極悅", Link = "/lcb/Pro_Comfort" },
                                        }
                                    },
                                    new FooterViewModel { Title = "銷售據點", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "旗艦門市", Link = "/lcb/distribution" },
                                            new FooterViewModel { Title = "經銷據點", Link = "/lcb/flagship" }
                                        }
                                    },
                                    new FooterViewModel { Title = "客戶服務", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "常見問題", Link = "/lcb/faq" },
                                            new FooterViewModel { Title = "清潔保養", Link = "/lcb/clean_mainte" },
                                            new FooterViewModel { Title = "維修服務", Link = "/lcb/repair_service" },
                                            new FooterViewModel { Title = " 聯絡我們", Link = "/lcb/contact_us" },
                                            new FooterViewModel { Title = " 產品型錄下載", Link = "/lcb/catalog" }
                                        }
                                    }
                                },
                                Logo_Image = "/upload/derek_logo.png",
                                Facebook_Link = "https://www.facebook.com/LCB.TW",
                                IG_Link = "https://www.instagram.com/tw_derek/?igshid=YmMyMTA2M2Y%3D",
                                YoutubeChannel_Link = "https://www.youtube.com/@derek6494",
                                Content = new List<string>
                                {
                                    "Copyright©",
                                    "2022 隆昌窯業股份有限公司",
                                    "版權所有"
                                }
                            };
                            break;
                        default:
                            footerViewModel = new FooterViewModel();
                            break;
                    }
                    break;
                case 3:
                    footerViewModel = new FooterViewModel
                    {
                        footerViewModels = new List<FooterViewModel> {
                            new FooterViewModel { Title = "最新消息", Link = "/ksp/home", footerViewModels = new List<FooterViewModel> {
                                new FooterViewModel { Title = "園區公告", Link = "/ksp/news" },
                                new FooterViewModel { Title = "活動列表", Link = "/ksp/Activity" },
                                new FooterViewModel { Title = "影音專區", Link = "/ksp/VideoZone" },
                                new FooterViewModel { Title = "防疫紓困專區", Link = "/ksp/PreventionZone" },
                            },
                            },
                            new FooterViewModel { Title = "園區資訊", Link = "/ksp/home", footerViewModels = new List<FooterViewModel> {
                                new FooterViewModel { Title = "園區介紹", Link = "/ksp/introduce" },
                                new FooterViewModel { Title = "交通資訊", Link = "/ksp/TrafficInfo" },
                                new FooterViewModel { Title = "休閒設施", Link = "/ksp/Facilities" },
                                new FooterViewModel { Title = "會議室租借", Link = "/ksp/MeetingRoomRental" },
                            },
                            },
                            new FooterViewModel { Title = "投資資訊", Link = "/ksp/home", footerViewModels = new List<FooterViewModel> {
                                new FooterViewModel { Title = "投資優勢", Link = "/ksp/InvestmentAdvantages" },
                                new FooterViewModel { Title = "租售空間", Link = "/ksp/RentandSellSpace" },
                                new FooterViewModel { Title = "投資進駐", Link = "/ksp/Stationed" },
                                new FooterViewModel { Title = "後續經營", Link = "/ksp/SubsequentOperation" },
                            },
                            },
                            new FooterViewModel { Title = "園區廠商", Link = "/ksp/home", footerViewModels = new List<FooterViewModel> {
                                new FooterViewModel { Title = "產業分類查詢", Link = "/ksp/firm_all" },
                                new FooterViewModel { Title = "地理位置查詢", Link = "/ksp/Locationinquire" }
                            },
                            },
                            new FooterViewModel { Title = "輔助資源", Link = "/ksp/home", footerViewModels = new List<FooterViewModel> {
                                new FooterViewModel { Title = "高雄軟體園區專屬輔助資源", Link = "/ksp/TutoringResources" },
                                new FooterViewModel { Title = "政府輔助資源", Link = "/ksp/AuxiliaryResources" },
                                new FooterViewModel { Title = "中小企業輔導體系", Link = "/ksp/GuidanceSystem" },
                                new FooterViewModel { Title = "法人單位/公協會", Link = "/ksp/PublicAssociation" },
                            },
                            }
                        },
                        line_qr = "/upload/ksp/line_qr.jpg",
                        line_title = "KSP高雄軟體園區\r\n官方LINE@",
                        line_describe = "即時的掌握園區第一手資訊\r\n快點加入高軟官方Line@",
                        Privacy_Link = $"/{defaultData.OrgName}/Privacy",
                        Accessibility_Link = "https://accessibility.moda.gov.tw/Applications/Detail?category=20231110163027",
                        Accessibility_Badge = "/upload/accessibility_badge.png",
                        Content = new List<string>
                        {
                            "經濟部產業園區管理局 版權所有© 2023 BIP ALL Rights Reserved."
                        }
                    };
                    break;
                case 4:
                    footerViewModel = new FooterViewModel
                    {
                        Sitemap_Link = $"/{defaultData.OrgName}/Website",
                        Privacy_Link = $"/{defaultData.OrgName}/Privacy",
                        Accessibility_Link = "https://accessibility.moda.gov.tw/Applications/Detail?category=20231110163027",
                        Accessibility_Badge = "/upload/accessibility_badge.png",
                        Content = new List<string>
                        {
                            "Tel：07-3611212 分機 317 / Fax：07-3612751 地址：811636高雄市楠梓區加昌路600號",
                            "經濟部產業園區管理局 版權所有 © 2023 BIP ALL Rights Reserved"
                        }
                    };
                    break;
                case 5:
                case 9:
                    switch (defaultData.Id)
                    {
                        case 3:
                            footerViewModel = new FooterViewModel
                            {
                                Title = "www.濠廣.tw",
                                footerViewModels = new List<FooterViewModel> {
                                    new FooterViewModel { Title = "關於濠廣", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "公司介紹", Link = "/haoguang/introduce" },
                                            new FooterViewModel { Title = "領導專業團隊", Link = "/haoguang/team" },
                                            new FooterViewModel { Title = "聯絡我們", Link = "/haoguang/contactUs" },
                                        }
                                    },
                                    new FooterViewModel { Title = "服務項目", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "風力發電服務工程", Link = "/haoguang/Windpowergeneration" },
                                            new FooterViewModel { Title = "散裝/貨輪船體維修", Link = "/haoguang/repair02" },
                                            new FooterViewModel { Title = "船舶主機、輔機維修", Link = "/haoguang/repair01" },
                                            new FooterViewModel { Title = "更多服務", Link = "/haoguang/moreservice" },
                                        }
                                    },
                                    new FooterViewModel { Title = "實績展示", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "水閥換新", Link = "/haoguang/watervalve" },
                                            new FooterViewModel { Title = "吊桿前端滑輪", Link = "/haoguang/pulley" },
                                            new FooterViewModel { Title = "船舶吊桿維修", Link = "/haoguang/shipboom" },
                                            new FooterViewModel { Title = "進口錨鍊吊掛更換安裝", Link = "/haoguang/anchorchain" },
                                            new FooterViewModel { Title = "艙蓋板整形", Link = "/haoguang/Hatchcover" },
                                            new FooterViewModel { Title = "開艙液壓缸", Link = "/haoguang/cylinder" },
                                            new FooterViewModel { Title = "更換錨鍊", Link = "/haoguang/anchorchain02" },
                                            new FooterViewModel { Title = "駕駛台警報系統", Link = "/haoguang/alert" },
                                            new FooterViewModel { Title = "駕駛台玻璃防水工程", Link = "/haoguang/waterproof" },
                                            new FooterViewModel { Title = "其他工程", Link = "/haoguang/Otherprojects" },
                                        }
                                    }
                                },
                                Content = new List<string>
                                {
                                    "© HAO GUANG",
                                    "International Enterprise Co, Ltd.",
                                    " ALL RIGHTS RESERVED. Design by EtheriT"
                                }
                            };
                            break;
                        case 4:
                            footerViewModel = new FooterViewModel
                            {
                                Title = "https://www.fu-how-24.com",
                                locale = defaultData.locale,
                                footerViewModels = new List<FooterViewModel> {
                                    new FooterViewModel { Title = "Company", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "About ", Link = "/haoguang/introduce" },
                                            new FooterViewModel { Title = "Lead Professional Teams", Link = "/haoguang/team" },
                                            new FooterViewModel { Title = "Contact", Link = "/haoguang/contactUs" },
                                        }
                                    },
                                    new FooterViewModel { Title = "Service", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "Wind Power Services", Link = "/haoguang/Windpowergeneration" },
                                            new FooterViewModel { Title = "Bulk/Freighter Hull Repairs", Link = "/haoguang/repair02" },
                                            new FooterViewModel { Title = "Main Engine and Auxiliary Machine Maintenance", Link = "/haoguang/repair01" },
                                            new FooterViewModel { Title = "More Services", Link = "/haoguang/moreservice" },
                                        }
                                    },
                                    new FooterViewModel { Title = "Performance", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "Water Valve Replacement", Link = "/haoguang/watervalve" },
                                            new FooterViewModel { Title = "Derrick Head Block", Link = "/haoguang/pulley" },
                                            new FooterViewModel { Title = "Vessel Derrick", Link = "/haoguang/shipboom" },
                                            new FooterViewModel { Title = "Imported Anchor Chain Hanging, Replacement, and Installation", Link = "/haoguang/anchorchain" },
                                            new FooterViewModel { Title = "Winch Clutch Replacement", Link = "/haoguang/winchclutch" },
                                            new FooterViewModel { Title = "Hatch Cover Plate Deformed", Link = "/haoguang/Hatchcover" },
                                            new FooterViewModel { Title = "Open Cabin Hydraulic Cylinders", Link = "/haoguang/anchorchain02" },
                                            new FooterViewModel { Title = "Replacement of Anchor Chain", Link = "/haoguang/cylinder" },
                                            new FooterViewModel { Title = "Bridge Alarm Systems", Link = "/haoguang/alert" },
                                            new FooterViewModel { Title = "Other Projects", Link = "/haoguang/Otherprojects" }
                                        }
                                    }
                                },
                                Content = new List<string>{
                                    "© HAO GUANG",
                                    "International Enterprise Co, Ltd.",
                                    " ALL RIGHTS RESERVED. Design by EtheriT"
                                }
                            };
                            break;
						case 8:
                            footerViewModel = new FooterViewModel
                            {
                                Title = "",
                                footerViewModels = new List<FooterViewModel> {
                                    new FooterViewModel { Title = "關於基金會", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "宗旨", Link = "/unitedtw/purpose" },
                                            new FooterViewModel { Title = "組織章程", Link = "/unitedtw/regulations" },
                                        }
                                    },
                                    new FooterViewModel { Title = "活動訊息", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "社區活動", Link = "/unitedtw/activity" },
                                            new FooterViewModel { Title = "影片連結", Link = "/unitedtw/video" },
                                        }
                                    },
                                    new FooterViewModel { Title = "公開資訊", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "年度預算及工作計畫", Link = "/unitedtw/plan" },
                                            new FooterViewModel { Title = "年度財務報表", Link = "/unitedtw/financial" },
                                            new FooterViewModel { Title = "年度工作報告書", Link = "/unitedtw/report" },
                                            new FooterViewModel { Title = "捐款名錄", Link = "/unitedtw/Donationer" },
                                        }
                                    },
                                    new FooterViewModel { Title = "聯絡我們", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "位置諮詢", Link = "/unitedtw/address" },
                                            new FooterViewModel { Title = "Mail聯繫", Link = "/unitedtw/mail" },
                                        }
                                    },
                                    new FooterViewModel { Title = "相關連結", Link = "other", footerViewModels = new List<FooterViewModel> {
                                        }
                                    }
                                },
                                Content = new List<string>
                                {
                                    "<span><i class=\"fa-solid fa-phone\"></i></span><a href=\"tel:03-3179599\" class=\"tel\">03-3179599</a><br>" +
                                    "<span><i class=\"fa-solid fa-at\"></i></span>電子郵件：<a href=\"mailto:03-3179599\">unitedte168@gmail.com</a><br>" +
                                    "<span><i class=\"fa-solid fa-house\"></i></span>地址：<a href=\"https://goo.gl/maps/eoGMYGKvxetaReKX8\">330桃園市桃園區經國路168號</a><br>" +
                                    "<div id=\"qrcode\"><a href=\"/unitedtw/home\"><img src=\"/upload/footer_qrcode.png\"></a></div>"
                                }
                            };
                            break;
						default:
                            footerViewModel = new FooterViewModel
                            {
                                Title = "kao-feng.cocker.com.tw",
                                footerViewModels = new List<FooterViewModel> {
                                    new FooterViewModel { Title = "關於高峰", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "公司介紹", Link = "/kao-feng/introduce" },
                                            new FooterViewModel { Title = "領導專業團隊", Link = "/kao-feng/team" },
                                            new FooterViewModel { Title = "聯絡我們", Link = "/kao-feng/contactUs" },
                                        }
                                    }
                                },
                                Content = new List<string>
                                {
                                    "© Kao Feng",
                                    "International Enterprise Co, Ltd.",
                                    " ALL RIGHTS RESERVED. Design by EtheriT"
                                }
                            };
                            break;
                    }
                    break;
                case 6:
                    footerViewModel = new FooterViewModel
                    {
                        Link = $"/upload/{defaultData.OrgName}",
                        Sitemap_Link = $"/{defaultData.OrgName}/Website",
                        Privacy_Link = $"/{defaultData.OrgName}/Privacy",
                        Accessibility_Link = "https://accessibility.moda.gov.tw/Applications/Detail?category=20231110163027",
                        Accessibility_Badge = "/upload/accessibility_badge.png",
                        line_qr = $"/upload/{defaultData.OrgName}/lineqr.png",
                        Content = new List<string>
                        {
                            "Tel：07-3611212 分機 317  /   Fax：07-3612751",
                            "地址：811636高雄市楠梓區加昌路600號",
                            "經濟部產業園區管理局 版權所有 © 2024 BIP ALL Rights Reserved"
                        }
                    };
                    break;
                case 7:
                    footerViewModel = new FooterViewModel
                    {
                        Content = new List<string>
                                {
                                    "Copyright© 高鋒開發有限公司 版權所有 |<br id=\"iswrap\"> KaoFeng Development Co., Ltd. ALL Rights Reservd<br>電話：<a href=\"tel:07-3737909\">07-3737909</a> &nbsp; 傳真：07-3737915<br id=\"iswrap\"> &nbsp; 地址：<a href=\"https://maps.app.goo.gl/6Q8ggmAWi6VCQ9us5\" target=\"_blank\"> 高雄市仁武區鳳仁路177-2號</a>",
                                }
                    };
                    break;
                case 8:
                    string footerMessage = "";
                    switch (siteId) {
                        case 6:
                            footerMessage = "<span class=\"footer-massage\">Copyright©2024 榮唐運輸股份有限公司 版權所有 <br id=\"iswrap\">&nbsp;&nbsp;電話：<a href=\"tel:07-8912360\">07-8912360(代表號)</a> <br id=\"smail-wrap\">&nbsp;&nbsp;傳真：<span>07-8912380</span><br id=\"smail-wrap\">&nbsp;&nbsp;統一編號:<span>13179181</span></span><hr id=\"smail-wrap\"><span class=\"footer-massage\">地址：<a href=\"https://maps.app.goo.gl/VcnQ5HaZ8ZVnstX27\" target=\"_blank\">高雄市小港區高坪十一路大坪頂停車場2號</a> &nbsp;&nbsp;<br id=\"iswrap\">E-mail：<a href=mailto:\"longtop.mail@msa.hinet.net\">longtop.mail@msa.hinet.net</a></span>";
                            break;
                        case 7:
                            footerMessage = "<div class=\"d-flex\" id=\"footrow\">" +
                                                 "<div class=\"span6\" id=\"footword\">" +
                                                    "<div class=\"wordfoot\">83163 高雄市大寮區濃公路79號</div>" +
                                                    "<div class=\"wordfoot wordleft\">電話：(07)7884882-4</div>" +
                                                    "<div class=\"wordfoot\">傳真：(07)788-4885-6</div>" +
                                                    "<div class=\"wordfoot\">No.79, Nonggong Rd., Daliao Dist., Kaohsiung City 831, Taiwan (R.O.C.)</div>" +
                                                    "<div class=\"wordfoot wordleft\">TEL：<a href=\"tel:886-77884882\">886-7-788-4882~4</a> &nbsp;&nbsp;FAX：886-7-788-4885~6</div>" +
                                                 "</div>" +
                                             "</div>";
                            break;
                    }
                    footerViewModel = new FooterViewModel
                    {
                        Content = new List<string>
                                 {
                                     footerMessage,
                                 }
                    };
                    break;
                default:
					footerViewModel = new FooterViewModel();
					break;
			}
            return View(defaultData.View, footerViewModel);
        }
    }
}
