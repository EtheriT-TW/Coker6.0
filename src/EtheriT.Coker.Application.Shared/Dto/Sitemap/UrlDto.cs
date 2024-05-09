using System.Xml.Serialization;

namespace EtheriT.Coker.Web.Public.Sitemap
{
    [XmlType("url")]
    public class UrlDto
    {
        public string loc { get; set; }
        public string priority { get; set; }
        public string lastmod { get; set; }
        public string changefreq { get; set; }
    }
}
