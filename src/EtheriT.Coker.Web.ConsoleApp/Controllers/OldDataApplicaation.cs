using EtheriT.Coker.Web.ConsoleApp.DbContextSet;
using EtheriT.Coker.Web.ConsoleApp.Models.OldDB;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace EtheriT.Coker.Web.ConsoleApp.Controllers
{
    public class OldDataApplicaation
    {
        public int SiteID { get; set; }
        public string source { get; set; }
        public string orgName { get; set; }
        public List<int> auIds { get; set; }
        public List<int> subIds { get; set; }
        public List<int> shopSubId { get; set; }
        public List<Models.FileUpload> FileUploads { get; set; }
        public List<Models.Article> loadData(out List<Models.Tag> tags)
        {
            List<Models.Article> articles = new List<Models.Article>();
            tags = new List<Models.Tag>();
            using (var dbContext = new OldDbContext(source))
            {
                var menuSubArt = dbContext.MenuSubs.Where(e => auIds.Contains(e.authors_id)).ToList();
                var menuSubArtItem = dbContext.MenuSubs.Where(e => auIds.Contains(e.id)).Select(e => new { e.id, e.title }).ToList();
                var menuSubArtIds = menuSubArt.Select(e => e.id).ToList();

                var shopIds = dbContext.Menus.Where(e => shopSubId.Contains(e.sub_id)).Select(e => e.id).ToList();

                var Menus = dbContext.Menus.Where(e => menuSubArtIds.Contains(e.sub_id) || subIds.Contains(e.sub_id) || shopIds.Contains(e.id)).OrderBy(o => o.ser_no).ThenByDescending(e => e.id).ToList();
                var menus = Menus.Select(e => e.id).ToList();
                var menusIdStr = Menus.Select(e => e.id.ToString()).ToList();
                var menuSubs = Menus.GroupBy(e => e.sub_id).Select(e => e.Key).ToList();
                var shops = dbContext.ShopInfos.Where(e => shopIds.Contains(e.menuID));
                var allshopsIds = shops.Select(e => e.id).ToList();
                var fence = dbContext.Fence.Where(o => allshopsIds.Contains(o.cid));
                var fenceIds = fence.Select(e => e.id).ToList();

                var menuCount = dbContext.MenuConts.Where(e => menus.Contains(e.menu_id)).Where(e => e.type == "1");
                var shopsCount = dbContext.MenuConts.Where(e => fenceIds.Contains(e.menu_id)).Where(e => e.type == "13");

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
                    var subData = menuSub.Where(e => e.id == item.sub_id);
                    string tagName = subData.Select(e => e.title).ToList()[0];
                    long TagId = 100000 + item.sub_id;
                    string Title = item.title;

                    if (menuSubArtIds.Contains(item.sub_id))
                    {
                        var auId = subData.Select(e => new { e.authors_id, e.title }).FirstOrDefault();
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
                    var mySubData = subData.FirstOrDefault();
                    var myMenuCount = menuCount.Where(e => e.menu_id == item.id).OrderBy(o => o.ser_no).ToList();
                    var myShops = shops.Where(e => e.menuID == item.id).ToList();
                    var myShopsIds = myShops.Select(e => e.id).ToList();
                    var mtShopFence = fence.Where(e => myShopsIds.Contains(e.cid)).ToList();

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
                            NodeDate = string.IsNullOrEmpty(item.note_date) ? null : DateTime.Parse(item.note_date),
                            StartTime = string.IsNullOrEmpty(item.start_date) ? null : DateTime.Parse(item.start_date),
                            EndTime = string.IsNullOrEmpty(item.end_date) ? null : DateTime.Parse(item.end_date),
                            permanent = item.end_date == "2100-12-31",
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
                        if (mySubData != null)
                        {
                            if (!string.IsNullOrEmpty(item.img1)) addFile(item.img1);
                            switch (int.Parse(string.IsNullOrEmpty(mySubData.use_module) ? "0" : mySubData.use_module))
                            {
                                case 8:
                                case 17:
                                    if (orgName == "ksp" && item.sub_id == 89)
                                    {
                                        article.Html = kspCompny(item);
                                    }
                                    else
                                    {
                                        article.Html = HttpUtility.HtmlDecode($@"
                                            <div class=""row mx-0"">
                                                <div class=""col-12 col-md-5 px-0"">
                                                    <figure>
                                                        <img class=""img-fluid"" src=""{(string.IsNullOrEmpty(item.img1) ? "/upload/htmlConten/4bd8bee2-2679-41a9-bb98-9270b6d00801.jpg" : item.img1)}"" alt=""{item.title}"" />
                                                    </figure>
                                                </div>
                                                <span class=""w-auto"">{item.cont ?? "".Trim()}</span>
                                            </div>
                                        ");
                                    }
                                    break;
                                default:
                                    article.Html = HttpUtility.HtmlDecode(item.cont ?? "".Trim());
                                    break;
                            }
                        }
                    }
                    else
                    {
                        article.Html += HttpUtility.HtmlDecode(item.cont ?? "".Trim());
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

                    switch (item.contDispType)
                    {
                        case 2:
                            string html2 = $@"<div class=""row row-cols-1 row-cols-sm-2 gx-0"">";
                            for (int j = 0; j < myMenuCount.Count; j++)
                            {
                                html2 += $@"<div class=""col py-3"">{AddMenuCountHtml(myMenuCount[j])}</div>";
                            }
                            article.Html += html2 + "</div>";
                            break;
                        case 3:
                            string html3 = $@"<div class=""row row-cols-1 row-cols-sm-3 gx-0"">";
                            for (int j = 0; j < myMenuCount.Count; j++)
                            {
                                html3 += $@"<div class=""col"">{AddMenuCountHtml(myMenuCount[j])}</div>";
                            }
                            article.Html += html3 + "</div>";
                            break;
                        case 4:
                            string html4 = $@"<div class=""row row-cols-2 row-cols-sm-4 gx-0"">";
                            for (int j = 0; j < myMenuCount.Count; j++)
                            {
                                html4 += $@"<div class=""col"">{AddMenuCountHtml(myMenuCount[j])}</div>";
                            }
                            article.Html += html4 + "</div>";
                            break;
                        default:
                            string html = $@"<div class=""row row-cols-1 gx-0"">";
                            for (int j = 0; j < myMenuCount.Count; j++)
                            {
                                html += $@"<div class=""col"">{AddMenuCountHtml(myMenuCount[j])}</div>";
                            }
                            article.Html += html + "</div>";
                            break;
                    }
                    if (myShops.Any())
                    {
                        string htmlString = HttpUtility.HtmlDecode(article.Html);
                        article.Description = Regex.Replace(htmlString, @"<(.|\n)*?>", "");
                        for (int j = 0; j < myShops.Count; j++)
                        {
                            article.Html = AddShopMenuCountHtml(myShops[j]);
                            for (int k = 0; k < mtShopFence.Count(); k++)
                            {
                                var myShopsMenuCount = shopsCount.Where(e => e.menu_id == mtShopFence[k].id).OrderBy(o => o.ser_no).ToList();
                                switch (mtShopFence[k].colNum)
                                {
                                    case 1:
                                        for (int l = 0; l < myShopsMenuCount.Count; l++)
                                        {
                                            article.Html += AddMenuCountHtml(myShopsMenuCount[l]);
                                        }
                                        break;
                                    default:
                                        string htmlStr = $@"<div class=""row row-cols-{(
                                            mtShopFence[k].colNum == 3 ? 1 : mtShopFence[k].colNum == 6 ? 3 : 2
                                        )} row-cols-sm-{mtShopFence[k].colNum} gx-0"">";
                                        for (int l = 0; l < myShopsMenuCount.Count; l++)
                                        {
                                            htmlStr += $@"<div class=""col"">{AddMenuCountHtml(myShopsMenuCount[l])}</div>";
                                        }
                                        article.Html += $"{htmlStr}</div>";
                                        break;
                                }
                            }
                        }
                    }

                    //排除部分文字無法被編輯問題
                    if (!string.IsNullOrEmpty(article.Html))
                    {
                        var m = Regex.Matches(article.Html ?? "", "(?:div>[\\s]*)[^\\s]*([\u4e00-\u9fa5_\",-]+[\\s]{0,1})+");
                        for (int j = 0; j < m.Count; j++)
                        {
                            if (string.IsNullOrEmpty(m[j].Value.Trim())) continue;
                            else if (Regex.IsMatch(m[j].Value, "^[\"_]")) continue;
                            else if (m[j].Value.Trim().Length <= 1) continue;
                            else
                                article.Html.Replace(m[j].Value, $"div><span>{m[j].Value.Replace("div>", "")}</span>");
                        }
                    }
                    article.Html = HttpUtility.HtmlEncode($@"<div class=""container"">{article.Html.Replace("/upload/Article/", "/upload/")
                                .Replace("/upload/", $"/upload/Article/")
                                .Replace("/Article/htmlConten/", "/htmlConten/")}</div>");
                    article.SaveHtml = article.Html.Replace("/upload/",$"/upload/{orgName}/");
                    articles.Add(article);
                }
            }
            return articles;
        }
        private string kspCompny(Models.Menus item)
        {
            if (item == null) return "";
            List<string> parms = new List<string>();
            string html = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(item.cont ?? ""));
            html = html.Replace("<br />", "<br>").Replace("<br><br>", "<br>");
            foreach (Match match in Regex.Matches(html, @"<span\s*[^>]*>([\s\S]+?)<br>", RegexOptions.IgnoreCase))
            {
                var s = match.Value.Split("</span>");
                foreach (string s2 in s)
                {
                    parms.Add(Regex.Replace(s2, @"<[^>]*>", ""));
                }
            }
            html = $@"<div class=""row mx-0 my-3 frame_type_2"">
                <div class=""col-12 col-md-5 px-0 d-flex justify-content-center"">
                    <div class=""img_shadow d-flex justify-content-center align-items-center"">
                        <img src=""{(string.IsNullOrEmpty(item.img1) ? "/upload/htmlConten/8ff2919c-dff5-420d-897b-d0eb639f8473.jpg" : item.img1)}"" class=""img-fluid"" />
                    </div>
                </div>
                <div class=""col-12 col-md-7 custom_h5 pt-0 px-0 px-5 flex-column"">
                    <div class=""d-flex align-items-baseline my-2 flex-column"">
                        <ul>";
            for (int i = 0; i < parms.Count; i = i + 2)
            {
                html += $@"<li class=""p-2""><span>{parms[i]}</span>：{(i + 1 < parms.Count ? parms[i + 1] : "")}</li>";
            }
            html += @"  </ul>
                        <div class=""mt-5 font-weight-bold"">歡迎有興趣之企業洽詢！</div>
                    </div>
                </div>
            </div>";
            return html;
        }
        private string AddShopMenuCountHtml(ShopInfo shop)
        {
            if (shop == null) return "";
            return $@"<div class=""container articletype"">
                <div class=""templatecontent"">
                    <div custom_block_template=""true"" class=""row mx-0 my-3 frame_type_1"">
                        <div class=""col-12 col-md-5 px-0 d-flex justify-content-center"">
                            <img src=""{(string.IsNullOrEmpty(shop.Picture1) ? "/upload/htmlConten/8ff2919c-dff5-420d-897b-d0eb639f8473.jpg" : shop.Picture1)}"" alt="""" class=""img-fluid"" />
                        </div>
                        <div class=""col-12 col-md-7 custom_h5 pt-0 px-0 px-md-5 d-flex justify-content-center flex-column"">
                            {(
                                shop.start == null || shop.end == null ? "" :
                                $@"<div class=""d-flex my-2"">
                                    <div class=""title_block px-2 py-2 rounded-3 me-2 d-grid"">活動時間</div>
                                    <div class=""d-flex align-items-center date"">
                                        <span class=""activity_time"">{shop.start.Value.ToString("yy年MM月dd日 tt HH:mm")}~{shop.end.Value.ToString("yy年MM月dd日 tt HH:mm")}</span>
                                        <a target=""_blank"" title=""{shop.name}"" class=""text-black ps-2"" href=""https://www.google.com/calendar/render?action=TEMPLATE&text={shop.name}&dates={shop.start.Value.ToUniversalTime().ToString("yyyyMMdd")}T{shop.start.Value.ToUniversalTime().ToString("HHmmss")}Z&details={shop.Toldescribe}"">
                                            <i class=""fa-solid fa-calendar-days"">
                                            </i>
                                        </a>
                                    </div>
                                </div>"
                            )}
                            {(
                                string.IsNullOrEmpty(shop.location) ? "" :
                                $@"<div class=""d-flex my-2"">
                                    <div class=""title_block px-2 py-2 rounded-3 me-2 d-grid"">
                                        活動地點
                                    </div>
                                    <a target=""_blank"" title=""連結至:{shop.location}(另開新視窗)"" href=""https://www.google.com.tw/maps/place/{shop.location}&z=16&output=embed&t="" class=""d-flex justify-content-center align-items-center text-black"">
                                        <span class=""activity_location"">{shop.location}</span><i class=""ps-2 fa-solid fa-location-dot"">
                                        </i>
                                    </a>
                                </div>"
                            )}
                            {(
                                string.IsNullOrEmpty(shop.Add) ? "" :
                                $@"<div class=""d-flex my-2"">
                                    <div class=""title_block px-2 py-2 rounded-3 me-2 d-grid"">
                                        地址
                                    </div>
                                    <a target=""_blank"" title=""連結至:{shop.Add}(另開新視窗)"" href=""https://www.google.com.tw/maps/place/{shop.Add}&z=16&output=embed&t="" class=""d-flex justify-content-center align-items-center text-black"">
                                        <span class=""activity_addr"">{shop.Add}</span><i class=""ps-2 fa-solid fa-location-dot"">
                                        </i>
                                    </a>
                                </div>"
                            )}
                            {(
                                string.IsNullOrEmpty(shop.org) ? "" : $@"<div class=""d-flex my-2"">
                                    <div class=""title_block px-2 py-2 rounded-3 me-2 d-grid"">主辦單位</div>
                                    <span class=""activity_organizer d-flex justify-content-center align-items-center"">{shop.org}</span>    
                                </div>"
                            )}
                            {(
                                string.IsNullOrEmpty(shop.co_organiser) ? "" : $@"<div class=""d-flex my-2"">
                                    <div class=""title_block px-2 py-2 rounded-3 me-2 d-grid"">協辦單位</div>
                                    <span class=""activity_assist_organizer d-flex justify-content-center align-items-center"">{shop.co_organiser}</span>    
                                </div>"
                            )}
                            {(
                                string.IsNullOrEmpty(shop.Website) ? "" : $@"<div class=""d-flex my-2"">
                                    <div class=""title_block px-2 py-2 rounded-3 me-2 d-grid"">
                                        相關連結
                                    </div>
                                    <a target=""_blank"" title=""連結至:相關連結(另開新視窗)"" href=""{shop.Website}""  class=""d-flex justify-content-center align-items-center text-black"">
                                        <span class=""activity_link"">{shop.Website}</span><i class=""ps-2 fa-regular fa-link-simple""></i>
                                    </a>
                                </div>"
                            )}
                            {(
                                string.IsNullOrEmpty(shop.Tel) ? "" : $@"<div class=""d-flex my-2"">
                                    <div class=""title_block px-2 py-2 rounded-3 me-2 d-grid"">
                                        電話
                                    </div>
                                    <a target=""_blank"" title=""撥打電話至:{shop.Tel}"" href=""tel:{shop.Tel}"" class=""text-black"">
                                        <span class=""activity_tel"">{shop.Tel}</span><i class=""ps-2 fa-solid fa-phone"">
                                        </i>
                                    </a>
                                </div>"
                            )}
                        </div>
                    </div>
                </div>
            </div>
            {(
                string.IsNullOrEmpty(shop.Toldescribe)?"":
                $@"<div class=""container"">{(shop.Toldescribe ?? "").Replace("\n", "<br />")}</div>"
            )}
            {(
                string.IsNullOrEmpty(shop.Add) ? "" :
                $@"<div class=""container""><iframe class=""map"" frameborder=""0"" title=""{shop.location}地圖"" src=""https://maps.google.com.tw/maps?f=q&hl=zh-TW&geocode=&q={shop.location}&z=16&output=embed&t="" class=""w-100""></iframe></div>"
            )}
            ";
        }
        private string AddMenuCountHtml(Menu_cont cont)
        {
            string html = "";
            cont.title = HttpUtility.HtmlDecode(cont.title);
            cont.cont = HttpUtility.HtmlDecode(cont.cont);
            switch (cont.objectType)
            {
                case 1:
                    html += cont.cont;
                    break;
                case 2:
                    if (string.IsNullOrEmpty(cont.img)) html = "";
                    else
                    {
                        addFile(cont.img);
                        html = $@"<a href=""{cont.img}"" download=""{(string.IsNullOrEmpty(cont.title) ? "" : cont.title)}"" target=""_blank"" class=""link_with_icon d-flex text-decoration-none edit_lock"">
                            <div class=""icon pe-2""><i></i></div>
                            <div class=""name text-black"">{cont.title}</div>
                        </a>";
                    }
                    break;
                case 3:
                    if (string.IsNullOrEmpty(cont.img))
                    {
                        html = $@"<div class=""col-12 p-3"">
                                    <div>{cont.cont}</div>
                                </div>";
                    }
                    else
                    {
                        addFile(cont.img);
                        switch (cont.img_align)
                        {
                            case "right":
                                html = $@"<div class=""d-flex flex-column-reverse flex-md-row mt-5 row mx-0"">
                                    <div class=""col-12 col-md-6 p-3"">
                                        <div class=""fw-bold custom_h4 pb-3"">{cont.title}</div>
                                        <div>{cont.cont}</div>
                                    </div>
                                    {(string.IsNullOrEmpty(cont.col1) ?
                                        @$"<div" :
                                        @$"<a href=""{cont.col1}"" title=""連結至：{cont.title}{(cont.col9 == "_blank" ? "(另開新視窗)" : "")}"" target=""{cont.col9}""")} class=""col-12 col-md-6 text-center align-self-center px-0"">
                                        <img src=""{cont.img}"" alt=""{(string.IsNullOrEmpty(cont.col1) ? cont.title : " ")}"" class=""img-fluid""/>
                                    {(string.IsNullOrEmpty(cont.col1) ? "</div>" : @$"</a>")}
                                </div>";
                                break;
                            case "center":
                                html = $@"<figure class=""d-flex flex-column text-center"">
                                    {(string.IsNullOrEmpty(cont.col1) ?
                                        @$"" :
                                        @$"<a href=""{cont.col1}"" title=""連結至：{cont.title}{(cont.col9 == "_blank" ? "(另開新視窗)" : "")}"" target=""{cont.col9}"" >")}
                                    <img src=""{cont.img}"" alt=""{(string.IsNullOrEmpty(cont.col1) ? cont.title : " ")}"" class=""gjs-plh-image""/>
                                    {(string.IsNullOrEmpty(cont.col1) ? "</figure>" : @$"</a>")}
                                    {(string.IsNullOrEmpty(cont.cont) ? "" : $@"<figcaption>
                                        <div class=""text-start"">{cont.cont}</div>
                                    </figcaption>")}
                                </figure>";
                                break;
                            default:
                                html = $@"<div class=""d-flex mt-5 row mx-0"">
                                    {(string.IsNullOrEmpty(cont.col1) ?
                                        @$"<div" :
                                        @$"<a href=""{cont.col1}"" title=""連結至：{cont.title}{(cont.col9 == "_blank" ? "(另開新視窗)" : "")}"" target=""{cont.col9}""")} class=""col-12 col-md-6 text-center align-self-center px-0"">
                                        <img src=""{cont.img}"" alt=""{(string.IsNullOrEmpty(cont.col1) ? cont.title : " ")}"" class=""img-fluid""/>
                                    {(string.IsNullOrEmpty(cont.col1) ? "</div>" : @$"</a>")}
                                    <div class=""col-12 col-md-6 p-3"">
                                        <div class=""text-start"">{cont.cont}</div>
                                    </div>
                                </div>";
                                break;
                        }
                    }
                    break;
                case 4:
                    html = $@"<iframe frameborder=""0"" title=""{cont.title}"" src=""https://maps.google.com/maps?&amp;q={cont.title}&z=20&t=q&output=embed"" class=""w-100""></iframe>";
                    break;
                case 5:
                    string theId = GetRandomString4(5);
                    html = $@"<div id=""{theId}""  class=""container my-2 qa"">
                        <a data-bs-toggle=""collapse"" href=""#{theId}_content"" role=""button"" aria-expanded=""true"" aria-controls=""{theId}_content"" class=""btn w-100 qa-bg d-flex justify-content-between align-items-center p-3 fs-5"">
                            <span draggable=""true"" class=""fas fa-angle-right"">{cont.title}</span>
                        </a>
                        <div id=""{theId}_content"" class=""collapse show"">
                          <div class=""card card-body"">{cont.cont}</div>
                        </div>
                      </div>";
                    break;
                case 6:
                    Regex regex = new Regex("^.*(?:(?:youtu.be\\/|v\\/|vi\\/|u\\/w\\/|embed\\/)|(?:(?:watch)??v(?:i)?=|&v(?:i)?=))([^#&?]*).*");
                    html = $@"<iframe title=""{cont.title}"" frameborder=""0"" src=""https://www.youtube.com/embed/{regex.Match(cont.col1 ?? "").Value}"" class=""w-100""></iframe>";
                    break;
                default: break;
            }
            return html;
        }
        private void addFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            else path = path.Replace("/upload/Article/", "/upload/")
                            .Replace("/upload/", $"/upload/Article/")
                            .Replace("/Article/htmlConten/", "/htmlConten/");
            if (FileUploads == null) FileUploads = new List<Models.FileUpload>();
            Models.FileUpload? file = FileUploads.Find(e => e.DownloadFileName == path);
            var array = path.Split('/');
            string OriginalFileName = array[array.Length - 1];
            string ContentType;
            var array2 = OriginalFileName.Split('.');
            switch (array2[array2.Length - 1].Trim().ToLower())
            {
                case "doc":
                    ContentType = "application/msword";
                    break;
                case "pdf":
                    ContentType = "application/pdf";
                    break;
                case "png":
                    ContentType = "image/png";
                    break;
                default:
                    ContentType = "image/jpeg";
                    break;
            }

            if (file == null)
            {
                file = new Models.FileUpload()
                {
                    FK_WebsiteId = SiteID,
                    GuidKey = Guid.NewGuid(),
                    FileGuid = Guid.NewGuid(),
                    OriginalFileName = OriginalFileName,
                    DownloadFileName = path,
                    ContentType = ContentType,
                    Size = 0,
                    IsDeleted = false,
                    CreationTime = DateTime.Now,
                    CreatorUserId = 1
                };
                FileUploads.Add(file);
            }
        }

        private string GetRandomString4(int length)
        {
            var str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var next = new Random();
            var builder = new StringBuilder();
            for (var i = 0; i < 5; i++)
            {
                builder.Append(str[next.Next(0, str.Length)]);
            }
            return builder.ToString();
        }
    }
}
