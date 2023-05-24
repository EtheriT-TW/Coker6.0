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
using System.Collections.Generic;

namespace EtheriT.Coker.Application.Article
{
    public class ArticleAppService : IArticleAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly ITagAppService tagAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        public ArticleAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
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
                    await loginUserData.SaveChanges(a);
                    asoid = a.Id;
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
                                        Tags = "",
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    if (output != null)
                    {
                        foreach (var data in output.data)
                        {
                            var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                            {
                                Fk_Aid = (long)data.GetType().GetProperty("Id").GetValue(data, null),
                                Type = (int)TagAssociateTypeEnum.文章
                            });

                            var tag_text = "";
                            foreach (var tagData in tagDatas)
                            {
                                tag_text += tag_text == "" ? tagData.Tag_Name : $"、{tagData.Tag_Name}";
                            }

                            data.GetType().GetProperty("Tags").SetValue(data, tag_text == "" ? "無" : tag_text);
                        }
                    }
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無文章資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<ArticleDataGetDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ArticleGetDataDto> GetDataOne(long Id)
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
                                        orderby e.SerNO
                                        select new ArticleGetDataDto
                                        {
                                            Id = e.Id,
                                            Title = e.Title,
                                            Description = e.Description,
                                            Visible = e.Visible,
                                            SerNO = e.SerNO,
                                            PopularVisible = e.PopularVisible,
                                            TagDatas = new List<TagGetSelectedDto>(),
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
        public async Task<List<DirectoryReleInfoDto>> GetDirectoryReleInfo(List<long> Ids)
        {

            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Article;
                var output = new List<DirectoryReleInfoDto>();
                var articleData = new List<ArticleGetDataDto>();

                if (result != null)
                {
                    foreach (var Id in Ids)
                    {
                        var tempoutput = await (from e in result
                                                where e.Id == Id
                                                where !e.IsDeleted && e.FK_WebsiteId == WebsiteID
                                                select new ArticleGetDataDto
                                                {
                                                    Id = e.Id,
                                                    Title = e.Title,
                                                    Description = e.Description,
                                                    SerNO = e.SerNO
                                                }).FirstOrDefaultAsync();

                        if (tempoutput != null)
                        {
                            articleData.Add(tempoutput);
                        }
                    }

                    if (articleData != null)
                    {
                        articleData.Sort((x, y) => (x.SerNO.CompareTo(y.SerNO) * 2 + x.Id.CompareTo(y.Id)));
                        foreach (var data in articleData)
                        {
                            var imagedata = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
                            {
                                Sid = data.Id,
                                Type = (int)FileBindTypeEnum.文章管理,
                                Size = 1
                            });

                            output.Add(new DirectoryReleInfoDto
                            {
                                Id = data.Id,
                                MainImage = imagedata.Count <= 0 ? "" : imagedata.First().Link,
                                Title = data.Title,
                                Description = data.Description
                            });
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

                    var delete_img_dto = new FileGetImgInputDto
                    {
                        Sid = result.Id,
                        Type = (int)FileBindTypeEnum.文章管理
                    };
                    var imgdelete_response = await fileUploadAppService.deleteImgBySId(delete_img_dto);

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
