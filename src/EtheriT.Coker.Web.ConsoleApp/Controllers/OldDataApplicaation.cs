using EtheriT.Coker.Web.ConsoleApp.DbContextSet;
using EtheriT.Coker.Web.ConsoleApp.Models.OldDB;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Text.RegularExpressions;
using System.Web;

namespace EtheriT.Coker.Web.ConsoleApp.Controllers
{
	public class OldDataApplicaation
	{
		private string oldDb { get; set; }
		private int SiteID { get; set; }
		public OldDataApplicaation(string connectionStr, int siteID = 0)
		{
			oldDb = connectionStr;
			SiteID = siteID;
		}
		public List<Models.Article> loadData(List<int> auIds, List<int> subIds, List<int> shopSubId, out List<Models.Tag> tags)
		{
			List<Models.Article> articles = new List<Models.Article>();
			tags = new List<Models.Tag>();
			using (var dbContext = new OldDbContext(oldDb))
			{
				var menuSubArt = dbContext.MenuSubs.Where(e => auIds.Contains(e.authors_id)).ToList();
				var menuSubArtItem = dbContext.MenuSubs.Where(e => auIds.Contains(e.id)).Select(e => new { e.id, e.title }).ToList();
				var menuSubArtIds = menuSubArt.Select(e => e.id).ToList();

				var Menus = dbContext.Menus.Where(e => menuSubArtIds.Contains(e.sub_id) || subIds.Contains(e.sub_id)).ToList();
				var shopIds = dbContext.Menus.Where(e => shopSubId.Contains(e.sub_id)).Select(e => e.id).ToList();
				var menus = Menus.Select(e => e.id).ToList();
				var menusIdStr = Menus.Select(e => e.id.ToString()).ToList();
				var menuSubs = Menus.GroupBy(e => e.sub_id).Select(e => e.Key).ToList();
				var shops = dbContext.ShopInfos.Where(e => shopIds.Contains(e.menuID));

				var menuCount = dbContext.MenuConts.Where(e => menus.Contains(e.menu_id)).Where(e => e.type == "1");
				var shopsCount = dbContext.MenuConts.Where(e => shopIds.Contains(e.menu_id)).Where(e => e.type == "11");

				var menuSub = dbContext.MenuSubs.Where(e => menuSubs.Contains(e.id));


				var TagAssociates = dbContext.ProdTag.Where(e => menusIdStr.Contains(e.prod_id)).Where(e => e.type == "cont").ToList();
				var TagIds = TagAssociates.Select(e => e.tag_id).ToList();
				var Tags = dbContext.Tag.Where(e => TagIds.Contains(e.id.ToString())).ToList();
				for (int i = 0; i < Tags.Count(); i++)
				{
					if (tags.Find(e => e.Title == Tags[i].title) == null)
					{
						tags.Add(new Models.Tag
						{
							Id = Tags[i].id,
							Title = Tags[i].title,
							CreationTime = new DateTime(),
							CreatorUserId = 1,
							FK_WebsiteId = SiteID,
						});
					}
				}
				for (int i = 0; i < Menus.Count; i++)
				{
					Models.Menus item = Menus[i];
					string tagName = menuSub.Where(e => e.id == item.sub_id).Select(e => e.title).ToList()[0];
					long TagId = 100000 + item.sub_id;
					string Title = item.title;

					if (menuSubArtIds.Contains(item.sub_id))
					{
						var auId = menuSub.Where(e => e.id == item.sub_id).Select(e => new { e.authors_id, e.title }).FirstOrDefault();
						if (auId != null)
						{
							var au = menuSubArtItem.Where(e => e.id == auId.authors_id).FirstOrDefault();
							if (au != null)
							{
								TagId = 100000 + auId.authors_id;
								tagName = au.title;
							}
						}
						Title = auId.title;
					}

					int serNo;
					int.TryParse(item.ser_no, out serNo);
					var myMenuCount = menuCount.Where(e => e.menu_id == item.id).ToList();
					var myShops = shops.Where(e => e.menuID == item.id).ToList();
					var myShopsMenuCount = shopsCount.Where(e => e.menu_id == item.id).ToList();

					if (tags.Find(e => e.Title == tagName) == null)
					{
						tags.Add(new Models.Tag
						{
							Id = TagId,
							Title = tagName,
							CreationTime = new DateTime(),
							CreatorUserId = 1,
							FK_WebsiteId = SiteID,
						});
					}
					Models.Article? article = articles.Find(e => e.Title == Title);
					if (article == null)
					{
						article = new Models.Article
						{
							Title = Title,
							Html = HttpUtility.HtmlDecode(item.cont),
							FK_WebsiteId = SiteID,
							SerNO = serNo,
							Visible = item.disp_opt == "Y",
							PopularVisible = item.popular_disp == "Y",
							Popular = item.popular.Value,
							Description = "",
							CreatorUserId = 1,
							CreationTime = DateTime.Now,
							IsDeleted = false,
							Associates = new List<Models.Tag_Associate> {
								new Models.Tag_Associate
								{
									FK_TId = TagId,
									Type= 2,
									CreationTime = DateTime.Now,
									IsDeleted = false,
									CreatorUserId = 1,
								}
							}
						};
					}
					var myTags = TagAssociates.Where(e => e.prod_id == item.id.ToString()).ToList();
					for (int j = 0; j < myTags.Count(); j++)
					{
						var t = tags.Find(e => e.Id.ToString() == myTags[j].tag_id);
						if (t != null)
						{
							Models.Tag_Associate a = new Models.Tag_Associate
							{
								FK_TId = t.Id,
                                Type = 2,
                                CreationTime = DateTime.Now,
								IsDeleted = false,
								CreatorUserId = 1,
							};
							article.Associates.Add(a);
						}
					}

					for (int j = 0; j < myShops.Count; j++)
					{
						article.Html += AddShopMenuCountHtml(myShops[j]);
					}
					for (int j = 0; j < myShopsMenuCount.Count; j++)
					{
						article.Html += AddMenuCountHtml(myShopsMenuCount[j]);
					}
					for (int j = 0; j < myMenuCount.Count; j++)
					{
						article.Html += AddMenuCountHtml(myMenuCount[j]);
					}
					article.Html = HttpUtility.HtmlEncode($@"<div class=""container"">{article.Html}</div>");
					article.SaveHtml = article.Html;
					articles.Add(article);
				}
			}
			return articles;
		}
		private string AddShopMenuCountHtml(ShopInfo shop)
		{
			return $@"
                <div class=""custom_h3 fw-bold my-2"">{shop.name}</div>
                <div class=""row mx-0"">
                    <div class=""col-12 col-md-5 px-0"">
                        <figure>
                            <img class=""img-fluid"" src=""{shop.Picture1}"" alt="""" />
                            <figcaption class=""text-center pt-1"">{shop.Picdescribe1}</figcaption>
                        </figure>
                    </div>
                    <div class=""col-12 col-md-7 custom_h5 pt-0 pt-md-5 px-0 px-md-5"">
                        {(
							shop.start != null && shop.end != null ? "" :
							$@"<div class=""align-items-baseline d-flex my-2"">
                                <div class=""title_block px-2 py-2 rounded-3 me-2"">活動時間</div>
                                <div class=""d-flex align-items-center"">
                                    {shop.start.Value.ToString("yy年MM月dd日 tt HH:mm")}~{shop.end.Value.ToString("yy年MM月dd日 tt HH:mm")}
                                    <a class=""text-black ps-2"" target=""_blank"" title=""{shop.name}"" href=""https://www.google.com/calendar/render?action=TEMPLATE&text={shop.name}&dates={shop.start.Value.ToUniversalTime().ToString("yyyyMMdd")}T{shop.start.Value.ToUniversalTime().ToString("HHmmss")}Z&details={shop.Toldescribe}"">
                                        <i class=""fa-solid fa-calendar-days""></i>
                                    </a>
                                </div>
                            </div>"
						)}
                        {(
							string.IsNullOrEmpty(shop.Add) ? "" :
							$@"<div class=""align-items-baseline d-flex my-2"">
                                <div class=""title_block px-2 py-2 rounded-3 me-2"">活動地點</div>
                                <a class=""d-flex align-items-center text-black"" target=""_blank"" title=""連結至:{shop.location}(另開新視窗)"" href=""https://www.google.com.tw/maps/place/{shop.location}&z=16&output=embed&t="">
                                {shop.location}<i class=""ps-2 fa-solid fa-location-dot""></i>
                                </a>
                            </div>"
						)}
                        {(
							string.IsNullOrEmpty(shop.Add) ? "" :
							$@"<div class=""align-items-baseline d-flex my-2"">
                                <div class=""title_block px-2 py-2 rounded-3 me-2"">地址</div>
                                <a class=""text-black"" target=""_blank"" title=""連結至:{shop.Add}(另開新視窗)"" href=""https://www.google.com.tw/maps/place/{shop.Add}&z=16&output=embed&t="">
                                 {shop.Add}<i class=""ps-2 fa-solid fa-location-dot""></i>
                                </a>
                             </div>"
						)}
                        {(
							string.IsNullOrEmpty(shop.org) ? "" : $@"<div class=""align-items-baseline d-flex my-2"">
                                <div class=""title_block px-2 py-2 rounded-3 me-2"">主辦單位</div>
                                <div class="""">{shop.org}</div>    
                            </div>"
						)}
                        {(
							string.IsNullOrEmpty(shop.co_organiser) ? "" : $@"<div class=""align-items-baseline d-flex my-2"">
                                <div class=""title_block px-2 py-2 rounded-3 me-2"">協辦單位</div>
                                <div class="""">{shop.co_organiser}</div>
                            </div>"
						)}
                        {(
							string.IsNullOrEmpty(shop.Website) ? "" : $@"<div class=""align-items-baseline d-flex my-2"">
                                <div class=""title_block px-2 py-2 rounded-3 me-2"">相關連結</div>
                                <a class=""text-black"" target=""_blank"" title=""連結至:相關連結(另開新視窗)"" href=""{shop.Website}"">
                                {shop.Website}<i class=""ps-2 fa-regular fa-link-simple""></i>
                                </a>
                            </div>"
						)}
                        {(
							string.IsNullOrEmpty(shop.Tel) ? "" : $@"<div class=""align-items-baseline d-flex my-2"">
                                <div class=""title_block px-2 py-2 rounded-3 me-2"">電話</div>
                                <a class=""text-black"" target=""_blank"" title=""撥打電話至:{shop.Tel}"" href=""tel:{shop.Tel}"">
                                {shop.Tel}<i class=""ps-2 fa-solid fa-phone""></i>
                                </a>
                            </div>"
						)}
                    </div>
                 </div>";
		}
		private string AddMenuCountHtml(Menu_cont cont)
		{
			string html = "";
			cont.title = HttpUtility.HtmlDecode(cont.title);
			cont.cont = HttpUtility.HtmlDecode(cont.cont);
			switch (cont.type)
			{
				case "1":
					html += cont.cont;
					break;
				case "2":
					html = $@"<a download="""" href=""{cont.col1}"" title="" "" target=""_blank"" class=""link_with_icon d-flex text-decoration-none edit_lock"">
                        <div class=""icon pe-2""><i></i></div>
                        <div class=""name text-black"">{cont.title}</div>
                    </a>";
					break;
				case "3":
					switch (cont.img_align)
					{
						case "right":
							html = $@"<div class=""mt-5 row mx-0"">
                                <div class=""col-12 col-md-6 text-center align-self-center px-0"">
                                    <img src=""{cont.img}"" alt="""" class=""img-fluid""/>
                                </div>
                                <div class=""col-12 col-md-6 p-3"">
                                    <div class=""fw-bold custom_h4 pb-3"">{cont.title}</div>
                                    <div>{cont.cont}</div>
                                </div>
                            </div>";
							break;
						case "center":
							html = $@"<figure class=""d-flex flex-column text-center"">
                                <img src=""{cont.title}"" alt="""" class=""gjs-plh-image""/>
                                <figcaption>
                                    <div>{cont.cont}</div>
                                </figcaption>
                            </figure>";
							break;
						default:
							html = $@"<div class=""flex-column-reverse flex-md-row mt-5 row mx-0"">
                                <div class=""col-12 col-md-6 p-3"">
                                    <div class=""fw-bold custom_h4 pb-3"">{cont.title}</div>
                                    <div>{cont.cont}</div>
                                </div>
                                <div class=""col-12 col-md-6 text-center align-self-center px-0"">
                                    <img src=""{cont.img}"" alt="""" class=""img-fluid""/>
                                </div>
                            </div>";
							break;
					}
					break;
				case "4":
					html = $@"<iframe frameborder=""0"" title=""{cont.title}"" src=""https://maps.google.com/maps?&amp;q={cont.title}&z=20&t=q&output=embed"" class=""w-100""></iframe>";
					break;
				case "6":
					Regex regex = new Regex("^.*(?:(?:youtu.be\\/|v\\/|vi\\/|u\\/w\\/|embed\\/)|(?:(?:watch)??v(?:i)?=|&v(?:i)?=))([^#&?]*).*");
					html = $@"<iframe title=""{cont.title}"" frameborder=""0"" src=""https://www.youtube.com/embed/{regex.Match(cont.col1 ?? "").Value}"" class=""w-100""></iframe>";
					break;
				default: break;
			}
			return html;
		}
	}
}
