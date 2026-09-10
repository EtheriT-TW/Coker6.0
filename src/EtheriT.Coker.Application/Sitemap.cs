using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.WebsiteCache;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Public.Sitemap;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public class Sitemap : ISitemap
    {
        private static readonly HashSet<string> NoIndexMenuRoutes = new(StringComparer.OrdinalIgnoreCase)
        {
            "search",
            "demosearch",
            "columnarsearch",
            "shoppingcar",
            "member",
            "favorites",
            "productdemo"
        };
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
                var header = await db.JsonObjects.Where(e => e.CacheKey == WebsiteCacheKeys.Menu).Where(e => e.FK_WebsiteId == site.Id).FirstOrDefaultAsync();
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
                    loc = BuildHomeUrl(site)
                });
            }
            setWebMenuUrl(Maps.Maps, urlset.Urls);
            await setArticleUrl(urlset.Urls);
            await setProductUrl(urlset.Urls);
            await setTechnicalCertificateUrl(urlset.Urls);
            return NormalizeUrlset(urlset);
        }
        private string BuildHomeUrl(WebSiteOrgNameDto site)
        {
            var normalizedSiteUrl = siteUrl.TrimEnd('/');
            return siteId == site.Id
                ? $"{normalizedSiteUrl}/"
                : $"{normalizedSiteUrl}/{(site.OrgName ?? string.Empty).Trim('/')}";
        }
        private void setWebMenuUrl(List<MenuItemDto> Maps, List<UrlDto> Urls) {
            if (Maps == null || !Maps.Any()) return;
            string orgName;
            Maps.ForEach(map =>
            {
                if (!string.IsNullOrEmpty(map.RouterName))
                {
                    if (map.hasContan &&
                        !string.Equals(map.RouterName, "home", StringComparison.OrdinalIgnoreCase) &&
                        !NoIndexMenuRoutes.Contains(map.RouterName))
                    {
                        orgName = webSites.Find(e => e.Id == map.FK_WebsiteId)?.OrgName??"";
                        Urls.Add(new UrlDto
                        {
                            loc = $"{siteUrl}/{orgName}/{map.RouterName}".Replace("//", "/").Replace(":/", "://"),
                            lastmod = FormatLastModified(map.LastModificationTime ?? map.CreationTime)
                        });
                    }
                    if (map.Children!=null && map.Children.Any()) setWebMenuUrl(map.Children, Urls);
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
                        loc = $"{siteUrl}/{site.OrgName}/search/article/{a.Id}".Replace("//", "/").Replace(":/", "://"),
                        lastmod = FormatLastModified(a.LastModificationTime ?? a.CreationTime)
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
                        loc = $"{siteUrl}/{site.OrgName}/search/product/{p.Id}".Replace("//", "/").Replace(":/", "://"),
                        lastmod = FormatLastModified(p.LastModificationTime ?? p.CreationTime)
                    });
                });
            }
        }
        private async Task setTechnicalCertificateUrl(List<UrlDto> Urls)
        {
            var now = DateTime.Now;
            foreach (var site in webSites)
            {
                var certificates = await db.TechnicalCertificates
                    .Where(e =>
                        e.FK_WebsiteId == site.Id &&
                        !e.IsDeleted &&
                        e.Disp_opt &&
                        (e.Permanent ||
                            ((!e.StartDate.HasValue || e.StartDate <= now) &&
                             (!e.EndDate.HasValue || e.EndDate >= now))))
                    .ToListAsync();
                certificates.ForEach(certificate =>
                {
                    Urls.Add(new UrlDto
                    {
                        loc = $"{siteUrl}/{site.OrgName}/search/techcert/{certificate.Id}"
                            .Replace("//", "/")
                            .Replace(":/", "://"),
                        lastmod = FormatLastModified(
                            certificate.LastModificationTime ?? certificate.CreationTime)
                    });
                });
            }
        }

        private static Urlset NormalizeUrlset(Urlset urlset)
        {
            var normalizedUrls = new List<UrlDto>();
            var urlsByLocation = new Dictionary<string, UrlDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var url in urlset.Urls)
            {
                var location = NormalizeLocation(url.loc);
                if (location == null)
                {
                    continue;
                }

                if (urlsByLocation.TryGetValue(location, out var existing))
                {
                    if (IsLaterLastModified(url.lastmod, existing.lastmod))
                    {
                        existing.lastmod = url.lastmod;
                    }
                    continue;
                }

                url.loc = location;
                urlsByLocation[location] = url;
                normalizedUrls.Add(url);
            }

            urlset.Urls = normalizedUrls;
            return urlset;
        }

        private static string? NormalizeLocation(string? location)
        {
            if (!Uri.TryCreate(location?.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return null;
            }

            var builder = new UriBuilder(uri)
            {
                Fragment = string.Empty
            };

            if (builder.Path.Length > 1)
            {
                builder.Path = builder.Path.TrimEnd('/');
            }

            return builder.Uri.AbsoluteUri;
        }

        private static bool IsLaterLastModified(string? candidate, string? current)
        {
            if (!DateTimeOffset.TryParse(
                    candidate,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var candidateDate))
            {
                return false;
            }

            return !DateTimeOffset.TryParse(
                       current,
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out var currentDate) ||
                   candidateDate > currentDate;
        }

        private static string FormatLastModified(DateTime value)
            => value.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
    }
}
