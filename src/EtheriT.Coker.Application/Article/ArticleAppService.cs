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
using EtheriT.Coker.Application.Dto;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ResponseMessageDto> AddUp_Simple(ArticleDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                long usetId = await loginUserData.GetUserId();
                if (dto.Id == 0)
                {
                    Core.Models.Article a = new Core.Models.Article
                    {
                        FK_WebsiteId = WebsiteID,
                        Title = dto.Title,
                        Description = dto.Description,
                        Visible = dto.Visible,
                        SerNO = dto.SerNO,
                        PopularVisible = dto.PopularVisible,
                        CreatorUserId = usetId,
                    };
                    db.Article.Add(a);
                }
                else
                {
                    var result = db.Article.Where(e => e.Id == dto.Id).FirstOrDefault();

                    if (result != null)
                    {
                        result.FK_WebsiteId = WebsiteID;
                        result.Title = dto.Title;
                        result.Description = dto.Description;
                        result.Visible = dto.Visible;
                        result.SerNO = dto.SerNO;
                        result.PopularVisible = dto.PopularVisible;
                        result.LastModifierUserId = usetId;
                        result.LastModificationTime = DateTime.Now;
                    }
                    else throw new Exception("查無文章資料");
                }
                db.SaveChanges();
                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            return output;
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
                                    select new ArticleDataGetDto
                                    {
                                        Id = e.Id,
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
        public async Task<ArticleDataGetDto> GetSimple(long Id)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Article;

                if (result != null)
                {
                    var output = await (from e in result
                                        where !e.IsDeleted && e.FK_WebsiteId == WebsiteID
                                        select new ArticleDataGetDto
                                        {
                                            Id = e.Id,
                                            Title = e.Title,
                                            Description = e.Description,
                                            Visible = e.Visible,
                                            SerNO = e.SerNO,
                                            PopularVisible = e.PopularVisible,
                                        }).FirstOrDefaultAsync();

                    return output;
                }
                else throw new Exception("查無文章資料");
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<ResponseMessageDto> Delete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                long usetId = await loginUserData.GetUserId();
                var result = db.Article.Where(e => e.Id == Id).FirstOrDefault();

                if (result != null)
                {
                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    result.DeleterUserId = usetId;
                    db.SaveChanges();
                    output.Success = true;
                }
                else throw new Exception("查無文章資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            return output;
        }
    }
}
