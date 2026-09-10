using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Web.Public.Sitemap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Xml.Serialization;
using System.Xml;
using System.Text;

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
        [Produces("application/xml")]
        public async Task<IActionResult> Index()
        {
            var urlset = await sitemap.GetUrlsetAsync();
            var xml = SerializeToXmlWithDeclaration(urlset);
            return File(xml, "application/xml; charset=utf-8");
        }

        [HttpGet]
        public IActionResult Legacy()
        {
            return RedirectToRoutePermanent("Sitemap");
        }

        private static byte[] SerializeToXmlWithDeclaration(Urlset urlset)
        {
            var xmlSerializer = new XmlSerializer(typeof(Urlset));
            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                OmitXmlDeclaration = false
            };

            using var memoryStream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
            {
                xmlSerializer.Serialize(xmlWriter, urlset);
            }

            return memoryStream.ToArray();
        }
    }
}
