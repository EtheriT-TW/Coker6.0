using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Newsletter;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Text.RegularExpressions;

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
        private readonly ITokenAppService tokenAppService;
        private readonly string ServiceName;
        private readonly IHtmlProcessor htmlProcessor;
        public ArticleAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            StringHandler stringHandler,
            IMapper mapper,
            IConfiguration configuration,
            ITagAppService tagAppService,
            IFileUploadAppService fileUploadAppService,
            ITokenAppService tokenAppService,
            IHtmlProcessor htmlProcessor
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.configuration = configuration;
            this.tagAppService = tagAppService;
            this.fileUploadAppService = fileUploadAppService;
            this.tokenAppService = tokenAppService;
            this.stringHandler = stringHandler;
            this.htmlProcessor = htmlProcessor;
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
                            Type = TagAssociateTypeEnum.文章,
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
                                               Available = !e.RemovedFromShelves,
                                               SerNO = e.SerNO,
                                               Popular = e.Popular,
                                               PopularVisible = e.PopularVisible,
                                               SaveHtml = e.SaveHtml,
                                               Html = e.Html,
                                               SaveCss = e.SaveCss,
                                               Css = e.Css,
                                               Tags = String.Join("、", (
                                                           from ta in db.Tag_Associates
                                                           where ta.FK_AId == e.Id && ta.Type == TagAssociateTypeEnum.文章 && !ta.IsDeleted
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
                                                      where ta.FK_AId == e.Id && ta.Type == TagAssociateTypeEnum.文章 && !ta.IsDeleted
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
                var result = await db.Article.Where(a => a.Id == Id).FirstOrDefaultAsync();

                if (result != null)
                {
                    var output = new ArticleGetDataDto
                    {
                        Id = result.Id,
                        Title = result.Title,
                        Subtitle = result.Subtitle,
                        Description = result.Description,
                        Longitude = result.Longitude,
                        Latitude = result.Latitude,
                        Visible = result.Visible,
                        SerNO = result.SerNO,
                        PopularVisible = result.PopularVisible,
                        TagDatas = new List<TagGetSelectedDto>(),
                        StartTime = result.StartTime,
                        EndTime = result.EndTime,
                        NodeDate = result.NodeDate,
                        RemovedFromShelves = !result.RemovedFromShelves,
                        permanent = result.permanent,
                        DataJson = string.IsNullOrEmpty(result.DataJson) ? null : JsonConvert.DeserializeObject<NewsletterFrameDto>(result.DataJson),
                        FileAreas = null,
                        Files = new List<FileGetArticleDisplayDto>()
                    };

                    if (output != null)
                    {
                        // 此處讀Html裡有哪些地方需要傳檔案
                        output.FileAreas = GetFileAreas(result.SaveHtml);

                        var fileDatas = await fileUploadAppService.getArticleFiles(output.Id);
                        if (fileDatas != null)
                        {
                            output.Files = fileDatas;
                        }

                        var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                        {
                            Fk_Aid = output.Id,
                            Type = TagAssociateTypeEnum.文章,
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
        private List<FileTypeAreaDto> GetFileAreas(string html)
        {
            var output = new List<FileTypeAreaDto>();

            if (html != "")
            {
                var doc = htmlProcessor.LoadHtml(stringHandler.HtmlDecode(html));
                var FileAreas = htmlProcessor.Find(doc, "[data-edit-type]");
                var ResultNodes = FileAreas.Where(d => d.GetAttributeValue("data-edit-type", "") == "File" || d.GetAttributeValue("data-edit-type", "") == "Files").ToList();
                foreach (var node in ResultNodes)
                {
                    var temp = new FileTypeAreaDto();
                    temp.type = node.GetAttributeValue("data-edit-type", "");
                    temp.key = node.GetAttributeValue("data-edit-key", "").Trim('“', '”', '"'); ;
                    temp.label = node.GetAttributeValue("data-edit-label", "").Trim('“', '”', '"'); ;

                    output.Add(temp);
                }
            }
            return output;
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
                var query = db.Article
                                .AsNoTracking()
                                .Where(e => siteIds.Contains(e.FK_WebsiteId))
                                .Where(e => !e.RemovedFromShelves)
                                .Where(e => e.Visible)
                                .Where(e => dto.Ids.Contains(e.Id))
                                .Where(e => e.permanent || e.StartTime < DateTime.Now && e.EndTime > DateTime.Now)
                                .Where(e => dto.Target == null || !string.IsNullOrEmpty(e.DataJson))
                                .OrderBy(a => a.SerNO)
                                .ThenByDescending(a => a.NodeDate)
                                .ThenByDescending(e => e.Id);
                if (dto.MaxLen != null && dto.MaxLen > 0) query = (IOrderedQueryable<Core.Models.Article>)query.Take((int)dto.MaxLen);
                var result = new List<Core.Models.Article>();
                int skip = ((dto.Page ?? 1) - 1) * dto.ShowNum ?? 12 - 1;
                skip = skip < 0 ? 0 : skip;
                if (dto.FindNearest == true)
                {
                    var distance = new List<DistanceTempDto>();
                    foreach (var data in query)
                    {
                        if (data.Longitude != null && data.Latitude != null)
                        {
                            var distemp = new DistanceTempDto();
                            distemp.Id = data.Id;
                            distemp.distance = Math.Sqrt(Math.Pow((double)data.Longitude - (double)dto.Longitude, 2) + Math.Pow((double)data.Latitude - (double)dto.Latitude, 2));
                            distance.Add(distemp);
                        }
                    }
                    distance.Sort((a, b) => a.distance < b.distance ? -1 : 1);
                    distance.Take(dto.MaxLen.Value).ToList();
                    var newresult = new List<Core.Models.Article>();
                    for (var i = 0; i < (dto.MaxLen.Value > distance.Count() ? distance.Count : dto.MaxLen.Value); i++)
                    {
                        newresult.Add(query.Where(e => e.Id == distance[i].Id).First());
                    }
                    result = newresult;
                }
                else result = query.ToList();
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
                    var tagids = await db.Tag_Associates.Where(e => e.FK_AId == Id && e.Type == TagAssociateTypeEnum.文章 && !e.IsDeleted).ToListAsync();

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
                await loginUserData.SetLogs(JsonConvert.SerializeObject(new { Id }), JsonConvert.SerializeObject(output));
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
                    importDto.Html = stringHandler.HtmlDecode(importDto.Html);
                    importDto.Html = htmlProcessor.RemoveNode(importDto.Html ?? "", ".backstageType");
                    importDto.Html = htmlProcessor.SetAttr(importDto.Html ?? "", "[target='_blank'] ", "rel", "noopener noreferrer");

                    importDto.Html = (importDto.Html ?? "").Replace($"/upload/{Orgname}/", "/upload/");
                    importDto.Css = (importDto.Css ?? "").Replace($"/upload/{Orgname}/", "/upload/");

                    article.Css = importDto.Css;
                    article.PageText = htmlProcessor.text(importDto.Html);
                    article.Html = stringHandler.HtmlEncode(importDto.Html);
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
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
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

                    var checktokenresponse = await tokenAppService.CheckToken(null);
                    var isLogin = checktokenresponse.IsLogin;

                    var Files = await fileUploadAppService.getArticleFiles(dto.Id);
                    var doc = htmlProcessor.LoadHtml(stringHandler.HtmlDecode(dto.SaveHtml));
                    var EditData = htmlProcessor.Find(doc, "[data-edit-type]");
                    var FileInsertNode = EditData.Where(d => d.GetAttributeValue("data-edit-type", "") == "File" || d.GetAttributeValue("data-edit-type", "") == "Files").ToList();

                    if (Files.Any() && FileInsertNode.Any())
                    {
                        foreach (var node in FileInsertNode)
                        {
                            var nodeType = node.GetAttributeValue("data-edit-type", "");
                            var areaKey = node.GetAttributeValue("data-edit-key", "");
                            var isSingle = nodeType.Equals("File", StringComparison.OrdinalIgnoreCase);

                            var templateName = node.GetAttributeValue("data-edit-template", "");
                            var templateNode = !string.IsNullOrWhiteSpace(templateName) ? htmlProcessor.Find(doc, $"#{templateName}")?.FirstOrDefault() : null;

                            if (templateNode == null)
                            {
                                var format = isSingle ? "{value}" : "{index}. {value}";
                                var idAttr = !string.IsNullOrWhiteSpace(templateName) ? $@" id=""{templateName}""" : "";
                                var html = @$"<a{idAttr} class=""download-item link_with_icon do_not_rename d-flex text-decoration-none edit_lock align-items-center d-none"">
                                                                        <div class=""icon pe-2""></div>
                                                                        <span data-edit-label=""檔案名稱"" data-edit-type=""string"" data-edit-format=""{format}"" class=""file-name name""></span>
                                                                        <span class=""download-btn"">
                                                                            <i class=""fas fa-lock""></i>
                                                                            <span class=""download-text"">下載</span>
                                                                            <i class=""fas fa-lock-open""></i>
                                                                        </span>
                                                                      </a>";

                                templateNode = HtmlNode.CreateNode(html);
                                node.PrependChild(templateNode);
                            }

                            node.SelectNodes(".//a[contains(@class,'download-item') and not(contains(@class,'d-none'))]")?.ToList().ForEach(n => n.Remove());

                            node.SelectSingleNode(".//div[contains(@class,'no-files-msg')]")?.Remove();

                            var candidates = Files.Where(f => f.isVisible && f.areakey.Equals(areaKey, StringComparison.OrdinalIgnoreCase));

                            if (isSingle) candidates = candidates.Take(1);

                            var list = candidates.ToList();

                            if (list.Count > 0)
                            {
                                var actualIndex = 1;
                                foreach (var file in list)
                                {
                                    var child = templateNode.CloneNode(true);
                                    child.Attributes.Remove("id");
                                    child.SetAttributeValue("class", string.Join(" ", child.GetAttributeValue("class", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(c => c != "d-none")));

                                    FileHtmlNodeSet(child, file, actualIndex.ToString(), isLogin);
                                    actualIndex++;

                                    node.AppendChild(child);
                                }
                            }
                            else
                            {
                                node.AppendChild(HtmlNode.CreateNode("<div class=\"no-files-msg\">無相關檔案可顯示</div>"));
                            }
                        }

                        dto.SaveHtml = doc.DocumentNode.OuterHtml;
                    }

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
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
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
                        var images = await fileUploadAppService.getImgFiles(new FileGetImgInputDto { Sid = articl.Id, Type = (int)FileBindTypeEnum.文章管理, Size = 1 });
                        if (images.Count > 0)
                        {
                            result.ImageUrl = images[0].Link;
                        }
                    }
                }
            }
            catch (Exception e)
            { }
            return result;
        }
        public async Task<ResponseMessageDto> RebuildContentWithFiles(long AId)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var article = await db.Article.Where(e => e.Id == AId).FirstOrDefaultAsync();

            try
            {
                if (article != null)
                {
                    var checktokenresponse = await tokenAppService.CheckToken(null);
                    var isLogin = checktokenresponse.IsLogin;
                    var Files = await fileUploadAppService.getArticleFiles(AId);

                    var SaveDoc = htmlProcessor.LoadHtml(stringHandler.HtmlDecode(article.SaveHtml));
                    var SaveEditData = htmlProcessor.Find(SaveDoc, "[data-edit-type]");
                    var SaveeNode = SaveEditData.Where(d => d.GetAttributeValue("data-edit-type", "") == "File" || d.GetAttributeValue("data-edit-type", "") == "Files").ToList();

                    var html = stringHandler.HtmlDecode(article.Html ?? "");
                    var Doc = htmlProcessor.LoadHtml(html);
                    var EditData = htmlProcessor.Find(Doc, "[data-edit-type]");
                    var Node = EditData.Where(d => d.GetAttributeValue("data-edit-type", "") == "File" || d.GetAttributeValue("data-edit-type", "") == "Files").ToList();

                    if (SaveeNode.Any() && Files.Any())
                    {
                        FileInsertNode(SaveeNode, SaveDoc, Files, isLogin);
                        article.SaveHtml = SaveDoc.DocumentNode.OuterHtml;

                        if (Node.Any())
                        {
                            FileInsertNode(Node, Doc, Files, isLogin);
                            article.Html = Doc.DocumentNode.OuterHtml;
                        }

                        await loginUserData.SaveChanges(article);
                        response.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        void FileInsertNode(List<HtmlNode> FileInsertNode, HtmlDocument Doc, List<FileGetArticleDisplayDto> Files, bool isLogin)
        {
            foreach (var node in FileInsertNode)
            {
                var nodeType = node.GetAttributeValue("data-edit-type", "");
                var areaKey = node.GetAttributeValue("data-edit-key", "");
                var isSingle = nodeType.Equals("File", StringComparison.OrdinalIgnoreCase);

                var templateName = node.GetAttributeValue("data-edit-template", "");
                var templateNode = !string.IsNullOrWhiteSpace(templateName) ? htmlProcessor.Find(Doc, $"#{templateName}")?.FirstOrDefault() : null;

                if (templateNode == null)
                {
                    var format = isSingle ? "{value}" : "{index}. {value}";
                    var idAttr = !string.IsNullOrWhiteSpace(templateName) ? $@" id=""{templateName}""" : "";
                    var html = @$"<a{idAttr} class=""download-item link_with_icon do_not_rename d-flex text-decoration-none edit_lock align-items-center d-none"">
                                                                    <div class=""icon pe-2""></div>
                                                                    <span data-edit-label=""檔案名稱"" data-edit-type=""string"" data-edit-format=""{format}"" class=""file-name name""></span>
                                                                    <span class=""download-btn"">
                                                                        <i class=""fas fa-lock""></i>
                                                                        <span class=""download-text"">下載</span>
                                                                        <i class=""fas fa-lock-open""></i>
                                                                    </span>
                                                                  </a>";

                    templateNode = HtmlNode.CreateNode(html);
                    node.PrependChild(templateNode);
                }

                node.SelectNodes(".//a[contains(@class,'download-item') and not(contains(@class,'d-none'))]")?.ToList().ForEach(n => n.Remove());

                node.SelectSingleNode(".//div[contains(@class,'no-files-msg')]")?.Remove();

                var candidates = Files.Where(f => f.isVisible && f.areakey.Equals(areaKey, StringComparison.OrdinalIgnoreCase));

                if (isSingle) candidates = candidates.Take(1);

                var list = candidates.ToList();

                if (list.Count > 0)
                {
                    var actualIndex = 1;
                    foreach (var file in list)
                    {
                        var child = templateNode.CloneNode(true);
                        child.Attributes.Remove("id");
                        child.SetAttributeValue("class", string.Join(" ", child.GetAttributeValue("class", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(c => c != "d-none")));

                        FileHtmlNodeSet(child, file, actualIndex.ToString(), isLogin);
                        actualIndex++;

                        node.AppendChild(child);
                    }
                }
                else
                {
                    node.AppendChild(HtmlNode.CreateNode("<div class=\"no-files-msg\">無相關檔案可顯示</div>"));
                }
            }
        }
        // 將之前前台才載入檔案的先全部Run一遍
        // 2026/05/14 21:15 正式站Run過一次
        public async Task<ResponseMessageDto> RebuildAllContentWithFiles()
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var articles = await db.Article.ToListAsync();
            try
            {
                foreach (var article in articles)
                {
                    var doc = htmlProcessor.LoadHtml(stringHandler.HtmlDecode(article.Html));
                    var EditData = htmlProcessor.Find(doc, "[data-edit-type]");
                    var FileInsertNode = EditData.Where(d => d.GetAttributeValue("data-edit-type", "") == "File" || d.GetAttributeValue("data-edit-type", "") == "Files").ToList();

                    if (FileInsertNode.Any())
                    {
                        await RebuildContentWithFiles(article.Id);
                    }
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        private void FileHtmlNodeSet(HtmlNode MainNode, FileGetArticleDisplayDto File, string index, bool IsLogin)
        {
            MainNode.SetAttributeValue("download", File.Name);
            MainNode.SetAttributeValue("data-fid", File.Id.ToString());
            MainNode.SetAttributeValue("data-extension", File.Extension);

            if (File.isEncryption)
            {
                MainNode.SetAttributeValue("href", "");
                MainNode.SetAttributeValue("class", $"btn_downloadEncryptedFile do_not_rename {MainNode.GetAttributeValue("class", "")}");
            }
            else
            {
                MainNode.SetAttributeValue("href", File.Link[0]);
                MainNode.SetAttributeValue("class", $"do_not_rename {MainNode.GetAttributeValue("class", "")}");
            }

            var namenode = MainNode.SelectSingleNode(".//*[" + "contains(concat(' ', normalize-space(@class), ' '), ' file-name ') " + "or contains(concat(' ', normalize-space(@class), ' '), ' name ')" + "]");
            if (namenode != null)
            {
                var format = namenode.GetAttributeValue("data-edit-format", "");
                if (format != "") File.Name = format.Replace("{index}", index).Replace("{value}", File.Name);
                namenode.RemoveAllChildren();
                namenode.AppendChild(namenode.OwnerDocument.CreateTextNode(File.Name));
            }

            var downloadbtnNode = MainNode.SelectSingleNode(".//*[contains(concat(' ', normalize-space(@class), ' '), ' download-btn ')]");

            if (downloadbtnNode != null)
            {
                HtmlNode insidenode = null;
                if (File.isEncryption)
                {
                    if (IsLogin) insidenode = downloadbtnNode.SelectSingleNode(".//*[contains(concat(' ', normalize-space(@class), ' '), ' lock-open ')]") ?? HtmlNode.CreateNode("<i class=\"lock-open fas fa-lock-open\"></i>");
                    else insidenode = downloadbtnNode.SelectSingleNode(".//*[contains(concat(' ', normalize-space(@class), ' '), ' lock ')]") ?? HtmlNode.CreateNode("<i class=\"lock fas fa-lock\"></i>");
                }
                else insidenode = downloadbtnNode.SelectSingleNode(".//*[contains(concat(' ', normalize-space(@class), ' '), ' default ')]") ?? HtmlNode.CreateNode("<span class=\"default\">下載</span>");

                if (insidenode != null)
                {
                    downloadbtnNode.RemoveAllChildren();
                    downloadbtnNode.AppendChild(insidenode.CloneNode(true));
                }
            }
            else if (File.isEncryption && !IsLogin)
            {
                downloadbtnNode = HtmlNode.CreateNode("<span class=\"download-btn\"><i class=\"fas fa-lock\"></i></span>");
                MainNode.AppendChild(downloadbtnNode);
            }
        }
        public async Task<ResponseMessageDto> FindArticleOrgName(long Id)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var siteData = await loginUserData.GetAllFrontWebsiteIdAndOrgName();
                var siteids = siteData.Select(x => x.Id).ToList();
                var ariricle = await db.Article.Where(e => siteids.Contains(e.FK_WebsiteId) && e.Id == Id && !e.RemovedFromShelves).FirstOrDefaultAsync();
                if (ariricle == null) throw new Exception("文章不存在");
                var orgName = siteData.FirstOrDefault(e => e.Id == ariricle.FK_WebsiteId)?.OrgName ?? "";
                if (string.IsNullOrEmpty(orgName)) throw new Exception("文章不存在於您的網站或子網站");
                response.Message = orgName;
                response.Success = true;
            }
            catch (Exception e)
            {
                response.Message = e.Message;
            }
            return response;
        }
    }
}
