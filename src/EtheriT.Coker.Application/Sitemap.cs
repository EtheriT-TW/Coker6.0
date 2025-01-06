using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Public.Sitemap;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public class Sitemap : ISitemap
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly long siteId;
        private readonly string siteUrl;
        private readonly List<string> childOrgNames;
        private readonly List<WebSiteOrgNameDto> webSites;
        public Sitemap(CokerDbContext db, LoginUserData loginUserData) {
            this.db = db;
            this.loginUserData = loginUserData;
            this.siteId = loginUserData.GetFrontWebsiteId();
            this.siteUrl = loginUserData.GetFrontWebsiteUrl().Result;
            this.childOrgNames = loginUserData.GetFrontChildOrgName();
            this.webSites = db.Websites.Where(e => e.Id == siteId || childOrgNames.Contains(e.OrgName))
                .Select(e => new WebSiteOrgNameDto { Id = e.Id, OrgName = e.OrgName, Level = e.Level }).ToList();
        }
        public async Task<Urlset> GetUrlsetAsync() {
            SiteMapDto Sitemap = new SiteMapDto();
            foreach (var site in webSites) {
                var header = await db.JsonObjects.Where(e => e.Type == (int)JsonObjectEnum.主選單).Where(e => e.FK_WebsiteId == site.Id).FirstOrDefaultAsync();
                if (header != null && !string.IsNullOrEmpty(header.Json))
                {
                    var list = JsonConvert.DeserializeObject<List<MenuItemDto>>(header.Json);
                    if (list != null && list.Any()) Sitemap.Maps.AddRange(list);
                }
            }
            return await GetUrlsetAsync(Sitemap);
        }
        private async Task<Urlset> GetUrlsetAsync(SiteMapDto Maps) {
            Urlset urlset = new Urlset();
            foreach (var site in webSites)
            {
                urlset.Urls.Add(new UrlDto
                {
                    loc = siteId == site.Id ? siteUrl : $"{siteUrl}/{site.OrgName}",
                    priority = "1.00",

                });
            }
            setWebMenuUrl(Maps.Maps, urlset.Urls, 0.9);
            await setArticleUrl(urlset.Urls);
            await setProductUrl(urlset.Urls);
            return urlset;
        }
        private void setWebMenuUrl(List<MenuItemDto> Maps, List<UrlDto> Urls, double priority = 1.0) {
            if (Maps == null || !Maps.Any()) return;
            string orgName;
            Maps.ForEach(map =>
            {
                if (!string.IsNullOrEmpty(map.RouterName))
                {
                    if (map.hasContan && map.RouterName != "home")
                    {
                        orgName = webSites.Find(e => e.Id == map.FK_WebsiteId)?.OrgName??"";
                        Urls.Add(new UrlDto
                        {
                            loc = $"{siteUrl}/{orgName}/{map.RouterName}".Replace("//", "/").Replace(":/", "://"),
                            priority = priority.ToString(),
                            lastmod = (map.LastModificationTime ?? map.CreationTime).ToString("yyyy-MM-ddTHH:mm:sszzz"),
                            changefreq = "monthly"
                        });
                    }
                    if (map.Children!=null && map.Children.Any()) setWebMenuUrl(map.Children, Urls, priority - 0.1);
                }
            });
            return;
        }
        private async Task setArticleUrl(List<UrlDto> Urls) {
            foreach (var site in webSites) {
                var Arti = await db.Article.Where(e => e.FK_WebsiteId == site.Id && !e.IsDeleted && e.Visible && !e.RemovedFromShelves).ToListAsync();
                Arti.ForEach(a => {
                    Urls.Add(new UrlDto
                    {
                        loc = $"{siteUrl}/{site.OrgName}/Search/Article/{a.Id}".Replace("//", "/").Replace(":/", "://"),
                        priority = "1.0",
                        lastmod = (a.LastModificationTime ?? a.CreationTime).ToString("yyyy-MM-ddTHH:mm:sszzz"),
                        changefreq = "never"
                    });
                });
            }
        }
        private async Task setProductUrl(List<UrlDto> Urls)
        {
            foreach (var site in webSites) {
                if (site.Level == WebsiteLevelEnum.形象) continue;
                var prods = await db.Prods.Where(e => e.FK_WebsiteId == site.Id && !e.IsDeleted && e.Visible && !e.RemovedFromShelves).ToListAsync();
                prods.ForEach(p => {
                    Urls.Add(new UrlDto
                    {
                        loc = $"{siteUrl}/{site.OrgName}/Search/Product/{p.Id}".Replace("//", "/").Replace(":/", "://"),
                        priority = "1.0",
                        lastmod = (p.LastModificationTime ?? p.CreationTime).ToString("yyyy-MM-ddTHH:mm:sszzz"),
                        changefreq = "monthly"
                    });
                });
            }
        }
    }
}
