
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
			ViewData["OrgName"] = defaultData.OrgName;

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
								LINE_Link = "https://lin.ee/b5G8BVo",
								Content = new List<string>
								{
									"Copyright©",
									"2022 隆昌窯業股份有限公司",
									"版權所有"
								}
							};
							break;
						case 9:
                            footerViewModel = new FooterViewModel
                            {
                                footerViewModels = new List<FooterViewModel> {
                                    new FooterViewModel { Title = "夯酷客", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "香辛與調味料", Link = "/yuanjer/Flavorings" },
                                            new FooterViewModel { Title = "高湯鮮味", Link = "/yuanjer/umami" },
                                            new FooterViewModel { Title = "炸粉", Link = "/yuanjer/batter" },
                                            new FooterViewModel { Title = "飲品、烘焙及零嘴", Link = "/yuanjer/baking" },
                                            new FooterViewModel { Title = "清潔蔬果", Link = "/yuanjer/vegclean" },
                                        }
                                    },
                                    new FooterViewModel { Title = "BELGA專區", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "全產品", Link = "/yuanjer/pro_belga" },
                                        }
                                    },
                                    new FooterViewModel { Title = "關於我們", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "公司介紹", Link = "/yuanjer/company" },
                                            new FooterViewModel { Title = "產品介紹", Link = "/yuanjer/yuanjer_pro" },
                                            new FooterViewModel { Title = "專業證照", Link = "/yuanjer/certification" },
                                            new FooterViewModel { Title = "產品自主性檢驗報告", Link = "/yuanjer/report" },
                                        }
                                    },
                                    new FooterViewModel { Title = "客戶服務", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "會員專區", Link = "/yuanjer/Member" },
                                            new FooterViewModel { Title = "常見問題", Link = "/yuanjer/faq" },
                                            new FooterViewModel { Title = "購物須知", Link = "/yuanjer/shoppingnotic" },
                                            new FooterViewModel { Title = "聯絡我們", Link = "/yuanjer/contact_us" },
                                        }
                                    },
                                    new FooterViewModel { Title = "最新消息", Link = "/yuanjer/News"},
                                    new FooterViewModel { Title = "美味生活", Link = "/yuanjer/howliving"}
                                },
                                Logo_Image = "/upload/yulogo.png",
                                Facebook_Link = "https://www.facebook.com/BELGADELIGHTS/",
                                IG_Link = "https://www.instagram.com/belgadelights/",
                                YoutubeChannel_Link = "",
                                LINE_Link = "https://line.me/ti/p/%40ezu9806k",
                                Content = new List<string>
                                {
                                    "Copyright©",
                                    "2025 沅哲有限公司",
                                    "版權所有"
                                }
                            };
                            break;
						case 13:
							footerViewModel = new FooterViewModel
							{
								footerViewModels = new List<FooterViewModel> {
									new FooterViewModel { Title = "商品分類", Link = "", footerViewModels = new List<FooterViewModel> {
											new FooterViewModel { Title = "銀髮友善 ", Link = "/defood/pro_eatender" },
											new FooterViewModel { Title = "鐵蛋系列", Link = "/defood/pro1" },
											new FooterViewModel { Title = "豆干系列", Link = "/defood/pro2" },
											new FooterViewModel { Title = "滷味系列", Link = "/defood/pro3" },
											new FooterViewModel { Title = "伴手禮盒", Link = "/defood/pro4" },
											new FooterViewModel { Title = "紹興系列", Link = "/defood/pro5" },
											new FooterViewModel { Title = "國境之南系列", Link = "/defood/pro6" },
											new FooterViewModel { Title = "舊攤滷味系列", Link = "/defood/pro7" },
										}
									},
									new FooterViewModel { Title = "關於我們", Link = "", footerViewModels = new List<FooterViewModel> {
											new FooterViewModel { Title = "品牌故事", Link = "/defood/brand" },
											new FooterViewModel { Title = "我們的榮耀", Link = "/defood/honor" },
											new FooterViewModel { Title = "國際認證", Link = "/defood/certification" },
											new FooterViewModel { Title = "市場通路", Link = "/defood/marketing" },
											new FooterViewModel { Title = "人才招募 ", Link = "/defood/recruit" },
										}
									},
									new FooterViewModel { Title = "訊息專區", Link = "", footerViewModels = new List<FooterViewModel> {
											new FooterViewModel { Title = "公告事項", Link = "/defood/news" },
											new FooterViewModel { Title = "媒體影音", Link = "/defood/video" },
											new FooterViewModel { Title = "平面媒體", Link = "/defood/report" },
										}
									},
									new FooterViewModel { Title = "食品代工", Link = "/defood/OEN", footerViewModels = new List<FooterViewModel> {
											new FooterViewModel { Title = "廠房設備", Link = "/defood/plantequipment" },
											new FooterViewModel { Title = "生產製程", Link = "/defood/prodprocess" }
										}
									},
									new FooterViewModel { Title = "銀髮友善", Link = "/defood/Eatender"},
									new FooterViewModel { Title = "客戶服務", Link = "/defood/contact_us"}
								},
								Logo_Image = "/upload/logo.png",
								LINE_Link = "https://lin.ee/s5Dt4hA",
								Facebook_Link = "https://www.facebook.com/defoodec",
								IG_Link = "https://www.instagram.com/luway2024/",
								YoutubeChannel_Link = "https://www.youtube.com/@nickgodes",
								Content = new List<string>
								{
									"Copyright©",
									"2024 得意中華食品有限公司",
									"版權所有"
								}
							};
							break;
                        case 16:
                            footerViewModel = new FooterViewModel
                            {
                                footerViewModels = new List<FooterViewModel> {
                                    new FooterViewModel { Title = "關於華生", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "公司簡介 ", Link = "/watsbio/company" },
                                            new FooterViewModel { Title = "經營理念", Link = "/watsbio/philosophy" },
                                            new FooterViewModel { Title = "研發團隊", Link = "/watsbio/team" },
                                            new FooterViewModel { Title = "專業認證", Link = "/watsbio/certification" },
                                            new FooterViewModel { Title = "自愈力學堂", Link = "/watsbio/LearningCenter" },
                                        }
                                    },
                                    new FooterViewModel { Title = "商品介紹", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "保健食品 ", Link = "/watsbio/product" },
                                            new FooterViewModel { Title = "購物流程", Link = "/watsbio/shopflow" },
                                            new FooterViewModel { Title = "付款方式", Link = "/watsbio/payment" },
                                            new FooterViewModel { Title = "訂購資訊", Link = "/watsbio/orderinfo" },
                                        }
                                    },
                                    new FooterViewModel { Title = "保健醫美", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "關於牛樟芝 ", Link = "/watsbio/aboutpro1" },
                                            new FooterViewModel { Title = "關於鹿角靈芝", Link = "/watsbio/aboutpro2" },
                                            new FooterViewModel { Title = "醫美訊息", Link = "/watsbio/MedicalAestheticInfo" },
                                            new FooterViewModel { Title = "養生保健", Link = "/watsbio/HolisticHealth" },
                                            new FooterViewModel { Title = "生活分享", Link = "/watsbio/lifeshare" },
                                        }
                                    },
                                    new FooterViewModel { Title = "更多", Link = "", footerViewModels = new List<FooterViewModel> {
                                            new FooterViewModel { Title = "最新消息", Link = "/watsbio/news" },
                                            new FooterViewModel { Title = "聯絡我們", Link = "/watsbio/contact" },
                                        }
                                    },
                                },
                                Logo_Image = "/upload/logo.png",
                                LINE_Link = "https://line.me/ti/p/T11xUWo0DU",
                                Facebook_Link = "",
                                IG_Link = "",
                                YoutubeChannel_Link = "",
                                Content = new List<string>
                                {
                                    "Copyright©",
                                    "2024 華生國際生技股份有限公司 版權所有",
                                    "Watson Biotech",
                                    "ALL Rights Reserved"
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
				case 10:
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
									new FooterViewModel { Title = "相關連結", Link = "/unitedtw/other", footerViewModels = new List<FooterViewModel> {
										}
									}
								},
								Content = new List<string>
								{
									"<span><i class=\"fa-solid fa-phone\"></i></span><a href=\"tel:03-3179599\" target=\"_blank\" title=\"撥打電話至:07-3737909(另開新視窗)\" class=\"tel\">03-3179599</a><br>" +
									"<span><i class=\"fa-solid fa-at\"></i></span>電子郵件：<a href=\"mailto:unitedte168@gmail.com\" target=\"_blank\" title=\"發送電子郵件至:unitedte168@gmail.com(另開新視窗)\">unitedte168@gmail.com</a><br>" +
									"<span><i class=\"fa-solid fa-house\"></i></span>地址：<a href=\"https://goo.gl/maps/eoGMYGKvxetaReKX8\" target=\"_blank\" title=\"連結至:google地圖(另開新視窗)\">330桃園市桃園區經國路168號</a><br>" +
									"<div id=\"qrcode\"><a href=\"/unitedtw/home\"><img src=\"/upload/footer_qrcode.png\"></a></div>"
								},
								Copyright = "Copyright © 2020 UNITED MEDICAL FOUNDATION TAIWAN. All Rights Reserved"
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
						case 10:
							footerViewModel = new FooterViewModel
							{
								Title = "",
								footerViewModels = new List<FooterViewModel> {
									new FooterViewModel { Title = "最新消息", Link = "/asnet/news"
									},
									new FooterViewModel { Title = "醫藥資訊", Link = "Medicalinfo", footerViewModels = new List<FooterViewModel> {
										}
									},
									new FooterViewModel { Title = "生活資訊", Link = "LifeInfo", footerViewModels = new List<FooterViewModel> {
										}
									},
									new FooterViewModel { Title = "預約匿名篩檢", Link = "http://redribbon.tw/vghks/index.php", footerViewModels = new List<FooterViewModel> {
										}
									},
									new FooterViewModel { Title = "視訊篩檢服務", Link = "VideoServices", footerViewModels = new List<FooterViewModel> {
										}
									},
									new FooterViewModel { Title = "Facebook", Link = "https://www.facebook.com/vghksasnet", footerViewModels = new List<FooterViewModel> {
										}
									},
									new FooterViewModel { Title = "網站地圖", Link = "website", footerViewModels = new List<FooterViewModel> {
										}
									}
								},
								Content = new List<string>
								{
									"<div class='abreast my-1 d-flex justify-content-center align-items-center'>",
	"									<div class='contact-info'>",
	"										 <div class='info-row'>",
	"											  <span class='label'>諮詢專線：</span>",
	"													<span class='value'><a href='tel:07-3468299' class='tel'>07-3468299</a></span>",
	"										 </div>",
	"										 <div class='info-row'>",
	"												 <span class='label'>篩檢地點：</span>",
	"													<span class='value'><a href='https://maps.app.goo.gl/C2mZJDm7atAJ4kA86' target=\"_blank\">高雄市左營區大中一路386號<br>醫療大樓10樓(感染症諮詢篩檢中心)</a></span>",
											"</div>",
										"</div>",
									"</div>"
								},
								Copyright = "最佳瀏覽解析度為1920x1080以上"
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
					switch (defaultData.Id)
					{
						case 5:
                            footerViewModel = new FooterViewModel
                            {
                                Content = new List<string>
                                {
                                    "Copyright© 高鋒開發有限公司 版權所有 |<br id=\"iswrap\"> KaoFeng Development Co., Ltd. ALL Rights Reservd<br>電話：<a href=\"tel:07-3737909\">07-3737909</a> &nbsp; 傳真：07-3737915<br id=\"iswrap\"> &nbsp; 地址：<a href=\"https://maps.app.goo.gl/6Q8ggmAWi6VCQ9us5\" target=\"_blank\"> 高雄市仁武區鳳仁路177-2號</a>",
                                }
                            };
                            break;
						case 12:
                            footerViewModel = new FooterViewModel
                            {
                                Content = new List<string>
                                {
                                    "Copyright© 湛盛度量衡器企業社 版權所有 |<br id=\"iswrap\"> 湛盛電子秤 ALL Rights Reservd<br>電話：<a href=\"tel:06-3128599\">06-3128599</a> &nbsp; 傳真：06-312-9399<br id=\"iswrap\"> &nbsp; Email：<a href=\"MAILTO:L2730520@yahoo.com.tw\" target=\"_blank\"> L2730520@yahoo.com.tw</a>",
                                }
                            };
                            break;
						case 15:
                            footerViewModel = new FooterViewModel
                            {
                                Content = new List<string>
                                {
                                    "Copyright© 寶順保全(股)公司 版權所有 |<br id=\"iswrap\"> 高雄總公司：高雄市前鎮區民權二路6號5樓-2\r\n<br>TEL:<a href=\"tel:07-3306396\">(07)3306396 轉 552~557</a> &nbsp; FAX:(07)3306506\r\n<br id=\"iswrap\">",
                                }
                            };
                            break;
                        default:
                            footerViewModel = new FooterViewModel();
                            break;
                    }
					break;
				case 8:
					string footerMessage = "";
					switch (siteId)
					{
						case 6:
							footerMessage = "<span class=\"footer-massage\">Copyright©2024 榮唐運輸股份有限公司 版權所有 <br id=\"iswrap\">&nbsp;&nbsp;電話：<a href=\"tel:07-8912360\">07-8912360(代表號)</a> <br id=\"smail-wrap\">&nbsp;&nbsp;傳真：<span>07-8912380</span><br id=\"smail-wrap\">&nbsp;&nbsp;統一編號:<span>13179181</span></span><hr id=\"smail-wrap\"><span class=\"footer-massage\">地址：<a href=\"https://maps.app.goo.gl/VcnQ5HaZ8ZVnstX27\" target=\"_blank\">高雄市小港區高坪十一路大坪頂停車場2號</a> &nbsp;&nbsp;<br id=\"iswrap\">E-mail：<a href=mailto:\"longtop.mail@msa.hinet.net\">longtop.mail@msa.hinet.net</a></span>";
							break;
						case 7:
							footerMessage = "<div class=\"d-flex\" id=\"footrow\">" +
												 "<div class=\"span6\" id=\"footword\">" +
													"<div class=\"wordfoot\">83163 高雄市大寮區濃公路79號</div>" +
													"<div class=\"wordfoot wordleft\">電話：(07)7884882-4 &nbsp;&nbsp;傳真：(07)788-4885-6</div>" +
													"<div class=\"wordfoot\">No.79, Nonggong Rd., Daliao Dist., Kaohsiung City 831, Taiwan (R.O.C.)</div>" +
												 "</div>" +
											 "</div>";
							break;
						case 11:
							footerMessage =
									"<div class=\"d-flex my-2\"><div id=\"tonight-block\"><a id=\"tonight-logo\" href=\"https://www.tonight-motel.com.tw/default1.asp\" title=\"連結至:晶夜官方網站(另開新視窗)\" target=\"_blank\"><img src = \"/upload/footer_image.png\"></a></div>" +
									"<div id=\"message-block\"><span>興震億建設．震億營造 ALL Rights Reserved</span></br>" +
									"<span>電話:</span><a href=\"tel:(08)751-7125\" target=\"_blank\" title=\"撥打電話至:(08)751-7125(另開新視窗)\" class=\"tel\">(08)751-7125</a> <span> &nbsp;&nbsp;傳真:(08)751-7135</span><br>" +
									"<span>地址:</span><a href=\"https://g.co/kgs/D83qX5m\" target=\"_blank\" title=\"連結至:google地圖(另開新視窗)\">屏東縣屏東市清溪里清寧街223號</a></div></div>";
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
