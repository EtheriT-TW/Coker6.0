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
            return Content(xml, "application/xml");
        }
        private string SerializeToXmlWithDeclaration(Urlset urlset)
        {
            var xmlSerializer = new XmlSerializer(typeof(Urlset));

            // 設置 XmlWriterSettings，並明確指定 UTF-8 編碼
            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,  // 確保使用 UTF-8 編碼
                OmitXmlDeclaration = false  // 保證寫入 XML 宣告
            };

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var xmlWriter = XmlWriter.Create(streamWriter, xmlWriterSettings))
            {
                // 開始寫入 XML 文件
                xmlWriter.WriteStartDocument();  // 加入 XML 宣告
                xmlSerializer.Serialize(xmlWriter, urlset);
                streamWriter.Flush();  // 確保內容被寫入流中

                // 返回 UTF-8 編碼的 XML 字串
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}
