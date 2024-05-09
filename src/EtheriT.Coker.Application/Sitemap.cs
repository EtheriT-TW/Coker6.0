using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Dto.enumType;
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
        private readonly string orgName;
        public Sitemap(CokerDbContext db, LoginUserData loginUserData) {
            this.db = db;
            this.loginUserData = loginUserData;
            this.siteId = loginUserData.GetFrontWebsiteId();
            this.siteUrl = loginUserData.GetFrontWebsiteUrl().Result;
            this.orgName = loginUserData.GetWebsiteOrgName(siteId).Result;
        }
        public async Task<Urlset> GetUrlsetAsync() {
            SiteMapDto Sitemap = new SiteMapDto();
            var header = await db.JsonObjects.Where(e => e.Type == (int)JsonObjectEnum.主選單).Where(e => e.FK_WebsiteId == siteId).FirstOrDefaultAsync();
            if (header != null && string.IsNullOrEmpty(header.Json))
            {
                var list = JsonConvert.DeserializeObject<List<MenuItemDto>>(header.Json);
                if(list!=null && list.Any()) Sitemap.Maps = list;
            }
            return await GetUrlsetAsync(Sitemap);
        }
        private async Task<Urlset> GetUrlsetAsync(SiteMapDto Maps) {
            Urlset urlset = new Urlset();
            setWebMenuUrl(Maps.Maps, urlset.Urls, 0.9);
            await setArticleUrl(urlset.Urls);
            await setProductUrl(urlset.Urls);
            await setTechCertUrl(urlset.Urls);
            return urlset;
        }
        private void setWebMenuUrl(List<MenuItemDto> Maps, List<UrlDto> Urls, double priority = 1.0) {
            if (Maps == null || !Maps.Any()) return;
            Maps.ForEach(map =>
            {
                if (!string.IsNullOrEmpty(map.RouterName))
                {
                    Urls.Add(new UrlDto
                    {
                        loc = $"{siteUrl}/{orgName}/{map.RouterName}".Replace("//", "/"),
                        priority = priority.ToString(),
                        lastmod = (map.LastModificationTime ?? map.CreationTime).ToString("yyyy/MM/dd HH:mm:ss"),
                        changefreq = "monthly"
                    });
                    if (map.Children!=null && map.Children.Any()) setWebMenuUrl(map.Children, Urls, priority - 0.1);
                }
            });
            return;
        }
        private async Task setArticleUrl(List<UrlDto> Urls) {
            var Arti = await db.Article.Where(e => e.FK_WebsiteId == siteId && !e.IsDeleted && e.Visible && !e.RemovedFromShelves).ToListAsync();
            Arti.ForEach(a => {
                Urls.Add(new UrlDto
                {
                    loc = $"{siteUrl}/{orgName}/Search/Article/{a.Id}".Replace("//", "/"),
                    priority = "1.0",
                    lastmod = (a.LastModificationTime ?? a.CreationTime).ToString("yyyy/MM/dd HH:mm:ss"),
                    changefreq = "never"
                });
            });
        }
        private async Task setProductUrl(List<UrlDto> Urls)
        {
            var prods = await db.Prods.Where(e => e.FK_WebsiteId == siteId && !e.IsDeleted && e.Visible && !e.RemovedFromShelves).ToListAsync();
            prods.ForEach(p => {
                Urls.Add(new UrlDto
                {
                    loc = $"{siteUrl}/{orgName}/Search/Product/{p.Id}".Replace("//", "/"),
                    priority = "1.0",
                    lastmod = (p.LastModificationTime ?? p.CreationTime).ToString("yyyy/MM/dd HH:mm:ss"),
                    changefreq = "monthly"
                });
            });
        }
        private async Task setTechCertUrl(List<UrlDto> Urls)
        {
            var Techs = await db.TechnicalCertificates.Where(e => e.FK_WebsiteId == siteId && !e.IsDeleted).ToListAsync();
            Techs.ForEach(t => {
                Urls.Add(new UrlDto
                {
                    loc = $"{siteUrl}/{orgName}/Search/Product/{t.Id}".Replace("//", "/"),
                    priority = "0.5",
                    lastmod = (t.LastModificationTime ?? t.CreationTime).ToString("yyyy/MM/dd HH:mm:ss"),
                    changefreq = "monthly"
                });
            });
        }
    }
}
