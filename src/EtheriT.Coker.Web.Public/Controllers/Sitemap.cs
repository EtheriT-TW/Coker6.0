using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Web.Public.Sitemap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace EtheriT.Coker.Web.Public.Controllers
{
    public class Sitemap : Controller
    {
        private readonly ISitemap sitemap;
        private readonly IWebHostEnvironment _env;
        public Sitemap(ISitemap sitemap, IWebHostEnvironment env) { 
            this.sitemap = sitemap;
            _env = env;
        }
        [HttpGet]
        public async Task<Urlset> Index()
        {
            return await sitemap.GetUrlsetAsync();
        }
    }
}
