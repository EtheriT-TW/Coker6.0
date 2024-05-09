using System.Xml.Serialization;

namespace EtheriT.Coker.Web.Public.Sitemap
{
    [XmlRoot(Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    [XmlType("urlset")]
    public class Urlset
    {
        [XmlElement("url")]
        public List<UrlDto> Urls { get; set; } = new List<UrlDto>();
    }
}
