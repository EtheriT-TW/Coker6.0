using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Application.Shared.Dto.Article;

namespace EtheriT.Coker.Application.Article
{
    public class ArticleAppService : IArticleAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        public ArticleAppService(
            CokerDbContext db,
            LoginUserData loginUserData
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Article;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted && e.FK_WebsiteId == WebsiteID
                                    select new ArticleListGetDto
                                    {
                                        FK_WebsiteId = e.FK_WebsiteId,
                                        Title = e.Title,
                                        Description = e.Description,
                                        Visible = e.Visible,
                                        SerNO = e.SerNO,
                                        Popular = e.Popular,
                                        PopularVisible = e.PopularVisible,
                                        SaveHtml = e.SaveHtml,
                                        Html = e.Html,
                                        SaveCss = e.SaveCss,
                                        Css = e.Css,
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無文章資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<MarqueeGetDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
    }
}
