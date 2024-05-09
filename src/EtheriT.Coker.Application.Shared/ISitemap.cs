using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Web.Public.Sitemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared
{
    public interface ISitemap
    {
        public Task<Urlset> GetUrlsetAsync();
    }
}
