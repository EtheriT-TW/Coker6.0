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
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Dto;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EtheriT.Coker.Application.Common;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Shared.Dto.Newsletter;
using DevExtreme.AspNet.Data.ResponseModel;
using System.Net;

namespace EtheriT.Coker.Application.Article
{
    public class ArticleAppService : IArticleAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly StringHandler stringHandler;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly ITagAppService tagAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly string ServiceName;
        public ArticleAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            StringHandler stringHandler,
            IMapper mapper,
            IConfiguration configuration,
            ITagAppService tagAppService,
            IFileUploadAppService fileUploadAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.configuration = configuration;
            this.tagAppService = tagAppService;
            this.fileUploadAppService = fileUploadAppService;
            this.stringHandler = stringHandler;
            ServiceName = "Article";
        }
        public async Task<ResponseMessageDto> AddUp(ArticleDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tag_response = new ResponseMessageDto() { Success = false };

            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                long usetId = await loginUserData.GetUserId();
                var asoid = dto.Id;

                if (dto.Id == null || dto.Id == 0)
                {
                    Core.Models.Article a = mapper.Map<Core.Models.Article>(dto);
                    a.FK_WebsiteId = WebsiteID;
                    a.RemovedFromShelves = !a.RemovedFromShelves;
                    db.Article.Add(a);
                    await loginUserData.SaveChanges(a);
                    asoid = a.Id;
                }
                else
                {
                    var result = db.Article.Where(e => e.Id == dto.Id).FirstOrDefault();

                    if (result != null)
                    {
                        mapper.Map(dto, result);
                        result.RemovedFromShelves = !result.RemovedFromShelves;
                        await loginUserData.SaveChanges(result);
                    }
                    else throw new Exception("查無文章資料");
                }

                if (asoid != null)
                {
                    var tagitem = new List<TagAssociateDto>();
                    foreach (var data in dto.TagSelected)
                    {
                        tagitem.Add(new TagAssociateDto()
                        {
                            Id = data.Id,
                            FK_AId = (long)asoid,
                            FK_TId = data.FK_TId,
                            Type = (int)TagAssociateTypeEnum.文章,
                            IsDeleted = data.IsDeleted
                        });
                    }

                    tag_response = await tagAppService.TagAssociateAddDelect(tagitem);
                    output.Message = asoid.ToString();
                }

                output.Success = tag_response.Success;
                output.Message = asoid.ToString();
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
                var result = db.Article.Where(e => !e.IsDeleted && e.FK_WebsiteId == WebsiteID);

                if (result != null)
                {
                    var dataQuery = await (from e in result
                                           orderby e.Id descending
                                           select new ArticleListGetDto
                                           {
                                               Id = e.Id,
                                               Title = e.Title,
                                               Subtitle = e.Subtitle,
                                               Description = e.Description,
                                               Longitude = e.Longitude,
                                               Latitude = e.Latitude,
                                               Visible = e.Visible,
                                               SerNO = e.SerNO,
                                               Popular = e.Popular,
                                               PopularVisible = e.PopularVisible,
                                               SaveHtml = e.SaveHtml,
                                               Html = e.Html,
                                               SaveCss = e.SaveCss,
                                               Css = e.Css,
                                               Tags = String.Join("、", (
                                                           from ta in db.Tag_Associates
                                                           where ta.FK_AId == e.Id && ta.Type == (int)TagAssociateTypeEnum.文章 && !ta.IsDeleted
                                                           join t in db.Tags on ta.FK_TId equals t.Id
                                                           where !t.IsDeleted && t.FK_WebsiteId == WebsiteID
                                                           select t.Title
                                                       ).ToList()),
                                               StartTime = e.StartTime,
                                               EndTime = e.EndTime,
                                               permanent = e.permanent,
                                               NodeDate = e.NodeDate,
                                           }).ToListAsync();
                    var output = DataSourceLoader.Load(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無文章資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<ArticleListGetDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetNewsletterList(DataSourceLoadOptions loadOptions)
        {
            string msg;
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Article.Where(e => e.FK_WebsiteId == WebsiteID && !e.IsDeleted);
                if (result != null)
                {
                    var data = await (from e in result
                                      orderby e.Id descending
                                      select new ArticleListGetDto
                                      {
                                          Id = e.Id,
                                          Title = e.Title,
                                          Subtitle = e.Subtitle,
                                          Description = e.Description,
                                          Longitude = e.Longitude,
                                          Latitude = e.Latitude,
                                          Visible = e.Visible,
                                          SerNO = e.SerNO,
                                          Popular = e.Popular,
                                          PopularVisible = e.PopularVisible,
                                          SaveHtml = e.SaveHtml,
                                          Html = e.Html,
                                          SaveCss = e.SaveCss,
                                          Css = e.Css,
                                          DataJson = e.DataJson,
                                          Tags = String.Join("、", (
                                                      from ta in db.Tag_Associates
                                                      where ta.FK_AId == e.Id && ta.Type == (int)TagAssociateTypeEnum.文章 && !ta.IsDeleted
                                                      join t in db.Tags on ta.FK_TId equals t.Id
                                                      where !t.IsDeleted && t.FK_WebsiteId == WebsiteID
                                                      select t.Title
                                                  ).ToList()),
                                          StartTime = e.StartTime,
                                          EndTime = e.EndTime,
                                          permanent = e.permanent,
                                          NodeDate = e.NodeDate,
                                      }).ToListAsync();
                    var dataQuery = data.Where(e => e.Tags.Contains("電子報")).ToList();

                    if (dataQuery != null)
                    {
                        foreach (ArticleListGetDto item in dataQuery)
                        {
                            if (string.IsNullOrEmpty(item.DataJson)) continue;
                            var myData = JsonConvert.DeserializeObject<NewsletterFrameDto>(item.DataJson ?? "");
                            if (myData != null && myData.No != 0)
                            {
                                item.Title = $"第{myData.No}期 {myData.Title}";
                            }
                        }
                    }
                    var output = DataSourceLoader.Load(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無電子報資料");
            }
            catch (Exception e)
            {
                msg = e.Message;
            }

            return new JsonResult(new List<ArticleListGetDto>() { new ArticleListGetDto { Title = msg } }, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ArticleGetDataDto> GetDataOne(long Id)
        {
            try
            {
                var result = db.Article;

                if (result != null)
                {
                    var output = await (from e in result
                                        where e.Id == Id
                                        where !e.IsDeleted
                                        select new ArticleGetDataDto
                                        {
                                            Id = e.Id,
                                            Title = e.Title,
                                            Subtitle = e.Subtitle,
                                            Description = e.Description,
                                            Longitude = e.Longitude,
                                            Latitude = e.Latitude,
                                            Visible = e.Visible,
                                            SerNO = e.SerNO,
                                            PopularVisible = e.PopularVisible,
                                            TagDatas = new List<TagGetSelectedDto>(),
                                            StartTime = e.StartTime,
                                            EndTime = e.EndTime,
                                            NodeDate = e.NodeDate,
                                            RemovedFromShelves = !e.RemovedFromShelves,
                                            permanent = e.permanent,
                                            DataJson = string.IsNullOrEmpty(e.DataJson) ? null : JsonConvert.DeserializeObject<NewsletterFrameDto>(e.DataJson)
                                        }).FirstOrDefaultAsync();

                    if (output != null)
                    {
                        var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                        {
                            Fk_Aid = output.Id,
                            Type = (int)TagAssociateTypeEnum.文章,
                        }
                        );

                        if (tagDatas != null)
                        {
                            output.TagDatas = tagDatas;
                        }
                    }

                    return output;
                }
                else throw new Exception("查無文章資料");
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<List<DirectoryReleInfoDto>> GetDirectoryReleInfo(DirectoryReleInfoInputDto dto)
        {
            string error = string.Empty;
            try
            {
                long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
                List<long> siteIds = await db.MappingWebsiteRelationship.Where(e => e.FatherId == WebsiteID || e.Id == WebsiteID).Where(e => !e.IsDeleted).Select(e => e.Id).ToListAsync();
                if (siteIds.Count == 0) siteIds.Add(WebsiteID);
                var sites = await db.Websites.Where(e => siteIds.Contains(e.Id)).Where(e => !e.IsDeleted).ToListAsync();

                var output = new List<DirectoryReleInfoDto>();
                var articleData = new List<ArticleListGetDto>();
                var result = await db.Article
                                    .Where(e => dto.Ids.Contains(e.Id))
                                    .Where(e => !e.IsDeleted)
                                    .Where(e => !e.RemovedFromShelves)
                                    .Where(e => siteIds.Contains(e.FK_WebsiteId))
                                    .Where(e => e.Visible)
                                    .Where(e => e.permanent || (DateTime.Compare(DateTime.Now, (DateTime)e.StartTime) > 0 && DateTime.Compare(DateTime.Now, (DateTime)e.EndTime) < 0))
                                    .Where(e => dto.Target == null || !string.IsNullOrEmpty(e.DataJson))
                                    .OrderBy(a => a.SerNO)
                                    .ThenByDescending(a => a.NodeDate)
                                    .ThenByDescending(e => e.Id)
                                    .ToListAsync();
                if (dto.FindNearest == true)
                {
                    var distance = new List<DistanceTempDto>();
                    result.ForEach(data =>
                    {
                        if (data.Longitude != null && data.Latitude != null)
                        {
                            var distemp = new DistanceTempDto();
                            distemp.Id = data.Id;
                            distemp.distance = Math.Sqrt(Math.Pow((double)data.Longitude - (double)dto.Longitude, 2) + Math.Pow((double)data.Latitude - (double)dto.Latitude, 2));
                            distance.Add(distemp);
                        }
                    });
                    distance.Sort((a, b) => a.distance < b.distance ? -1 : 1);
                    distance.Take(dto.MaxLen.Value).ToList();
                    var newresult = new List<Core.Models.Article>();
                    for (var i = 0; i<(dto.MaxLen.Value>distance.Count()? distance.Count: dto.MaxLen.Value); i++)
                    {
                        newresult.Add(result.Find(e => e.Id == distance[i].Id));
                    }
                    result = newresult;
                }
                else
                {
                    if (dto.MaxLen != null && dto.MaxLen > 0) result = result.Take(dto.MaxLen.Value).ToList();
                }
                int skip = ((dto.Page ?? 1) - 1) * dto.ShowNum ?? 12 - 1;
                if (skip < 0) skip = 0;
                if (string.IsNullOrEmpty(dto.Target))
                {
                    articleData = mapper.Map(result, articleData).Skip(skip).Take(dto.ShowNum ?? 12).ToList();
                    if (articleData != null)
                    {
                        foreach (var data in articleData)
                        {
                            var imagedata = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
                            {
                                Sid = data.Id,
                                Type = (int)FileBindTypeEnum.文章管理,
                                Size = 3
                            });
                            NewsletterFrameDto? DataJson = JsonConvert.DeserializeObject<NewsletterFrameDto>(data.DataJson ?? "{}");

                            var output_data = new DirectoryReleInfoDto();
                            var website = sites.Find(e => e.Id == data.FK_WebsiteId);
                            if (website != null)
                            {
                                output_data.type = DirectoryTypeEnum.文章;
                                output_data = mapper.Map(data, output_data);
                                output_data.Link = $"/article/{data.Id}";
                                output_data.MainImage = imagedata.Count <= 0 ? "" : imagedata.First().Link;
                                output_data.NodeDate = data.NodeDate;
                                output_data.OrgName = website.OrgName;
                                output_data.Title = ((DataJson != null && DataJson.No != 0) ? $"第{DataJson.No}期 " : "") + output_data.Title;
                                output_data.Subtitle = data.Subtitle;
                                if (data.Html != null && data.Html.IndexOf("activity_start_time") > 0)
                                {
                                    var g = Regex.Match(stringHandler.HtmlDecode(data.Html), "activity_start_time\">(.*?)<").Groups;
                                    if (!string.IsNullOrEmpty(g[0].Value)) output_data.StartTime = DateTime.Parse(g[1].Value);
                                }
                                if (data.Html != null && data.Html.IndexOf("activity_addr") > 0)
                                {
                                    var g = Regex.Match(stringHandler.HtmlDecode(data.Html), "activity_addr\">(.*?)<").Groups;
                                    if (!string.IsNullOrEmpty(g[0].Value)) output_data.Address = g[1].Value;
                                }
                                if (data.Html != null && data.Html.IndexOf("activity_location") > 0)
                                {
                                    var g = Regex.Match(stringHandler.HtmlDecode(data.Html), "activity_location\">(.*?)<").Groups;
                                    if (!string.IsNullOrEmpty(g[0].Value)) output_data.Location = g[1].Value;
                                }
                                output.Add(output_data);
                            }
                        }
                    }
                }
                else
                {
                    List<NewsletterFrameDto> list = new List<NewsletterFrameDto>();
                    foreach (var item in result.Take(30))
                    {
                        if (!string.IsNullOrEmpty(item.DataJson))
                        {
                            NewsletterFrameDto? obj = JsonConvert.DeserializeObject<NewsletterFrameDto>(item.DataJson);
                            if (obj != null) list.Add(obj);
                        }
                    }
                    if (list.Any())
                    {
                        switch (dto.Target.ToLower())
                        {
                            case "conten2":
                                var items = list.Select(e => e.Conten2).Where(e => e != null).Where(e => e.Visible ?? false).ToList();
                                mapper.Map(items, output).Skip(skip).Take(dto.ShowNum ?? 12).ToList();
                                break;
                        }
                    }
                }

                return output;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<ResponseMessageDto> Delete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tagdeleteresponse = new ResponseMessageDto() { Success = true };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var result = db.Article.Where(e => e.Id == Id).FirstOrDefault();

                if (result != null)
                {
                    var tagids = await db.Tag_Associates.Where(e => e.FK_AId == Id && e.Type == (int)TagAssociateTypeEnum.文章 && !e.IsDeleted).ToListAsync();

                    if (tagids != null)
                    {
                        foreach (var tagid in tagids)
                        {

                            tagdeleteresponse = await tagAppService.TagAssociateDelete(tagid.Id);
                        }
                    }

                    var delete_img_dto = new FileDeleteDto
                    {
                        Sid = result.Id,
                        Type = (int)FileBindTypeEnum.文章管理
                    };
                    var imgdelete_response = await fileUploadAppService.deleteFileById(delete_img_dto);

                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    result.DeleterUserId = usetId;

                    db.SaveChanges();

                    output.Success = tagdeleteresponse.Success && imgdelete_response.Success;
                }
                else throw new Exception("查無文章資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            finally
            {
                await loginUserData.SetLogs(ServiceName, "Delete", JsonConvert.SerializeObject(new { Id }), JsonConvert.SerializeObject(output));
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
                    results.Title = article.Title;
                    results.Conten = new ArticleSaveContenDto
                    {
                        SaveHtml = article.SaveHtml,
                        SaveCss = article.SaveCss
                    };
                    results.Conten.SaveHtml = stringHandler.HtmlEncode(results.Conten.SaveHtml);
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

                dto.SaveHtml = stringHandler.HtmlEncode(dto.SaveHtml);
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
                response.Error = ex.Message;
            }
            finally
            {
                await loginUserData.SetLogs(ServiceName, "ImportConten", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }
        public async Task<ResponseMessageDto> SaveConten(ArticleSaveContenDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                dto.SaveHtml = stringHandler.HtmlEncode(dto.SaveHtml);
                var article = await db.Article.FirstOrDefaultAsync(e => e.Id == dto.Id);

                if (article != null)
                {
                    article.SaveHtml = dto.SaveHtml;
                    article.SaveCss = dto.SaveCss;
                    await loginUserData.SaveChanges(article);
                    response.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            finally
            {
                await loginUserData.SetLogs(ServiceName, "SaveConten", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
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
                var articl = await db.Article.Where(e => e.Id == dto.articleId).Where(e => !e.IsDeleted).Where(e => !e.RemovedFromShelves).Where(e => e.FK_WebsiteId == dto.siteId).FirstOrDefaultAsync();
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
                        result.Html = result.Html != null ? result.Html.Replace("&lt;body&gt;", "").Replace("&lt;/body&gt;", "") : result.Html;
                        result.LastModificationTime = articl.LastModificationTime ?? articl.CreationTime;
                        result.Popular = articl.PopularVisible ? articl.Popular : null;
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
