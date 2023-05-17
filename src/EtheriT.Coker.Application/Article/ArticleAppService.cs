using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Dto;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Web;
using EtheriT.Coker.Web.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Dto;
using Microsoft.Extensions.Configuration;

namespace EtheriT.Coker.Application.Article
{
    public class ArticleAppService : IArticleAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        public ArticleAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            IConfiguration configuration
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.configuration = configuration;
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

            return new JsonResult(new List<ArticleDataGetDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
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
                                        where e.Id == Id
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
        public async Task<GetArticleContenDto> GetConten(SearchIDDto dto)
        {
            GetArticleContenDto results = new GetArticleContenDto();
            try
            {
                long siteId = await loginUserData.GetWebsiteId();
                var article = await db.Article.Where(e => e.FK_WebsiteId == siteId)
                                    .Where(e => e.Id == dto.Id)
                                    .Where(e => !e.IsDeleted)
                                    .FirstOrDefaultAsync();
                if (article != null)
                {
                    results.Conten = new ArticleSaveContenDto
                    {
                        SaveHtml = article.SaveHtml,
                        SaveCss = article.SaveCss
                    };
                    results.Conten.SaveHtml = HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(results.Conten.SaveHtml));
                    results.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.Error = ex.Message;
            }
            return results;
        }
        public async Task<ResponseMessageDto> ImportConten(ArticleSaveContenDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var userId = await loginUserData.GetUserId();

                dto.SaveHtml = HttpUtility.HtmlEncode(dto.SaveHtml);
                ArticleContenDto importDto = new ArticleContenDto
                {
                    Id = dto.Id,
                    Html = dto.SaveHtml,
                    Css = dto.SaveCss
                };
                var s = await SaveConten(dto);
                var user = await loginUserData.GetUser();
                var article = await db.Article.FirstOrDefaultAsync(e => e.Id == dto.Id);
                if (article != null)
                {
                    string Orgname = await loginUserData.GetWebsiteOrgName();
                    importDto.Html = (importDto.Html ?? "").Replace($"/upload/{Orgname}/", "/upload/");
                    importDto.Css = (importDto.Css ?? "").Replace($"/upload/{Orgname}/", "/upload/");

                    article.Html = importDto.Html;
                    article.Css = importDto.Css;
                    article.LastModificationTime = DateTime.Now;
                    article.LastModifierUserId = userId;

                    await loginUserData.SaveChanges(article);
                    response.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> SaveConten(ArticleSaveContenDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                dto.SaveHtml = HttpUtility.HtmlEncode(dto.SaveHtml);
                var user = await loginUserData.GetUser();
                var article = await db.Article.FirstOrDefaultAsync(e => e.Id == dto.Id);

                article.SaveHtml = dto.SaveHtml;
                article.SaveCss = dto.SaveCss;
                article.LastModificationTime = DateTime.Now;
                article.LastModifierUserId = user.Id;

                db.SaveChanges();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<GetFrontContenOutputDto> GetFrontConten(ArticleGetFrontContenInputDto dto)
        {
            if (dto.siteId == null)
            {
                dto.siteId = configuration.GetValue<long>("WebConfig:SiteId");
            }
            GetFrontContenOutputDto result = new GetFrontContenOutputDto();
            try
            {
                var side = await db.Websites.Where(e => e.Id == dto.siteId).FirstOrDefaultAsync();
                var articl = await db.Article.Where(e => e.Id == dto.articleId).Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == dto.siteId).FirstOrDefaultAsync();
                if (side != null)
                {
                    result.SiteName = side.Title;
                    if (articl != null)
                    {
                        result.Id = (int)articl.Id;
                        result.Title = articl.Title;
                        result.Description = articl.Description;
                        result.Html = articl.Html;
                        result.Css = articl.Css;
                        result.Html = result.Html.Replace("&lt;body&gt;", "").Replace("&lt;/body&gt;", "");
                    }
                }
            }
            catch (Exception e)
            {
            }
            return result;
        }
    }
}
