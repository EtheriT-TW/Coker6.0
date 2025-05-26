using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.EnterAd;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using System.Web;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Templates;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Shared.Templates;

namespace EtheriT.Coker.Application.HtmlContent
{
    public class HtmlContentAppService : IHtmlContentAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly ITemplatesApplicationService templatesApplicationService;
        public HtmlContentAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            IFileUploadAppService fileUploadAppService,
            ITemplatesApplicationService templatesApplicationService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.fileUploadAppService = fileUploadAppService;
            this.templatesApplicationService = templatesApplicationService;
        }
        public async Task<ResponseMessageDto> AddUp(HtmlContentDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                long userid = await loginUserData.GetUserId();
                var ascoid = dto.Id;
                dto.Html = HttpUtility.HtmlEncode(dto.Html);
                if (dto.Id == 0)
                {
                    if (userid != 0)
                    {
                        long WebsiteID = await loginUserData.GetWebsiteId();
                        Html_Content newItem = mapper.Map<Html_Content>(dto);
                        newItem.FK_WebsiteId = WebsiteID;
                        db.Html_Contents.Add(newItem);
                        await loginUserData.SaveChanges(newItem);
                        ascoid = newItem.Id;
                    }
                    else throw new Exception("查無資料");
                }
                else
                {
                    var db_hc = db.Html_Contents.Where(e => e.Id == dto.Id).FirstOrDefault();
                    if (db_hc != null && userid != 0)
                    {
                        mapper.Map(dto, db_hc);
                        await loginUserData.SaveChanges(db_hc);
                    }
                    else throw new Exception("查無資料");
                }
                output.Success = true;
                output.Message = ascoid.ToString();
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
            return output;
        }
        public async Task<HtmlContentListOutpotDto> GetAllComponent()
        {
            HtmlContentListOutpotDto respose = new HtmlContentListOutpotDto();
            try
            {
                var result = await db.Html_Contents.Include(e => e.ObjectClassify)
                        .Where(e => !e.IsDeleted && e.Disp_opt)
                        .OrderBy(e => e.ObjectClassify.SerNo)
                        .ThenBy(e => e.Ser_no)
                        .ToListAsync();
                respose.List = mapper.Map<List<HtmlContentDto>>(result);
                respose.Success = true;
            }
            catch (Exception e)
            {
                respose.Error = e.Message;
            }

            return respose;
        }
        public async Task<HtmlContentListOutpotDto> GetComponent(long type)
        {
            HtmlContentListOutpotDto respose = new HtmlContentListOutpotDto();
            try
            {
                var result = await db.Html_Contents.Include(e => e.ObjectClassify).Where(e => e.Type == type).ToListAsync();
                respose.List = mapper.Map<List<HtmlContentDto>>(result);
                respose.Success = true;
            }
            catch (Exception e)
            {
                respose.Error = e.Message;
            }
            return respose;
        }
        public async Task<JsonResult> GetAllList(int type, DataSourceLoadOptions loadOptions)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Html_Contents;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted && e.Type == type && e.FK_WebsiteId == WebsiteID
                                    orderby e.Ser_no
                                    select new HtmlContentGetAllListDto
                                    {
                                        Id = e.Id,
                                        Type = e.Type,
                                        Title = e.Title,
                                        Img = e.Img,
                                        Html = e.Html,
                                        Ser_no = e.Ser_no,
                                        Disp_opt = e.Disp_opt,
                                        ObjectType = e.ObjectType,
                                        Link = e.Link,
                                        Target = e.Target,
                                        StartDate = e.StartDate,
                                        EndDate = e.EndDate,
                                        permanent = e.permanent
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    var htmlType = 0;
                    switch (type)
                    {
                        case (int)HtmlContentTypeEnum.右側浮動廣告:
                            htmlType = (int)FileBindTypeEnum.右側浮動廣告;
                            break;
                        case (int)HtmlContentTypeEnum.進入廣告:
                            htmlType = (int)FileBindTypeEnum.進入廣告;
                            break;
                    }
                    if (output != null && htmlType != -1)
                    {
                        foreach (var data in output.data)
                        {
                            var htmlId = data.GetType().GetProperty("Id").GetValue(data, null);
                            var getImgFileInput = new FileGetImgInputDto
                            {
                                Sid = (long)htmlId,
                                Type = htmlType,
                                Size = 1
                            };
                            var image = await fileUploadAppService.getImgFiles(getImgFileInput);
                            if (image.Count > 0)
                            {
                                data.GetType().GetProperty("Img").SetValue(data, image[0].Link);
                            }
                        }
                    }
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }
            return new JsonResult(new List<HtmlContentGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<HtmlContentDto> GetOne(int id)
        {
            try
            {

                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Html_Contents.Where(e => e.Id == id && !e.IsDeleted && e.FK_WebsiteId == WebsiteID).FirstOrDefault();

                if (result != null)
                {
                    HtmlContentDto output = new HtmlContentDto()
                    {
                        Id = result.Id,
                        Type = result.Type,
                        Title = result.Title,
                        Img = result.Img,
                        Html = result.Html,
                        Ser_no = result.Ser_no,
                        Disp_opt = result.Disp_opt,
                        ObjectType = result.ObjectType,
                        Link = result.Link,
                        Target = result.Target,
                        StartDate = result.StartDate,
                        EndDate = result.EndDate,
                        permanent = result.permanent
                    };

                    return output;
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<JsonResult> GetDisplay(long webid, int type, int number)
        {
            try
            {
                var result = db.Html_Contents;

                if (result != null)
                {
                    var output = await (from e in result
                                        where !e.IsDeleted && e.Disp_opt && e.Type == type && e.FK_WebsiteId == webid
                                        where e.permanent || (DateTime.Compare(DateTime.Now, (DateTime)e.StartDate) > 0 && DateTime.Compare(DateTime.Now, (DateTime)e.EndDate) < 0)
                                        orderby e.Ser_no
                                        select new HtmlContentDisplayDto
                                        {
                                            Id = e.Id,
                                            Title = e.Title,
                                            Img = new List<string>(),
                                            Html = e.Html,
                                            Link = e.Link,
                                            Target = e.Target,
                                        }).Take(number).ToArrayAsync();

                    if (output != null)
                    {
                        switch (type)
                        {
                            case (int)HtmlContentTypeEnum.右側浮動廣告:
                                for (var i = 0; i < output.Length; i++)
                                {
                                    var r_imagedata = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
                                    {
                                        Sid = output[i].Id,
                                        Type = (int)FileBindTypeEnum.右側浮動廣告,
                                        Size = 1
                                    });
                                    output[i].Img.Add(r_imagedata.First().Link);
                                }
                                break;
                            case (int)HtmlContentTypeEnum.進入廣告:
                                var e_imagedata = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
                                {
                                    Sid = output[0].Id,
                                    Type = (int)FileBindTypeEnum.進入廣告,
                                    Size = 1
                                });
                                foreach (var img in e_imagedata)
                                {
                                    output[0].Img.Add(img.Link);
                                }
                                break;
                        }
                    }

                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });

                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<HtmlContentDisplayDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long userid = await loginUserData.GetUserId();
                var db_hc = db.Html_Contents.Where(e => e.Id == Id).FirstOrDefault();

                if (db_hc != null && userid != 0)
                {
                    db_hc.IsDeleted = true;
                    await loginUserData.SaveChanges(db_hc);
                    switch (db_hc.Type)
                    {
                        case (int)HtmlContentTypeEnum.右側浮動廣告:
                            var delete_image_r = await fileUploadAppService.deleteFileById(new FileDeleteDto()
                            {
                                Sid = db_hc.Id,
                                Type = (int)FileBindTypeEnum.右側浮動廣告,
                            });
                            break;
                        case (int)HtmlContentTypeEnum.進入廣告:
                            var delete_image_e = await fileUploadAppService.deleteFileById(new FileDeleteDto()
                            {
                                Sid = db_hc.Id,
                                Type = (int)FileBindTypeEnum.進入廣告,
                            });
                            break;
                    }
                    output.Success = true;
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<HtmlContentTypeDto> GetTypeList()
        {
            HtmlContentTypeDto response = new HtmlContentTypeDto { Success = true };
            try
            {
                bool isSupUser = await loginUserData.isSystemUser();
                if (isSupUser)
                {
                    bool othersOnly = await loginUserData.IsExtraSuperUser();
                    response.Type = (from o in db.ObjectTypes.Where(e => !e.IsDeleted)
                                    orderby o.SerNo
                                    select new EnumDictionaryDto { 
                                        Key =o.Title,
                                        Value = o.Id
                                    }).ToList();
                }
                else
                {
                    response.Type = new List<EnumDictionaryDto>
                    {
                        new EnumDictionaryDto{ Key = ObjectTypeEnum.自訂.ToString(), Value = (int)ObjectTypeEnum.自訂 }
                    };
                }
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Error = e.Message;
            }
            return response;
        }
        public async Task<UploadFileOutputDto> getHtmlContentFiles(GetFileListDto dto)
        {
            UploadFileOutputDto response = new UploadFileOutputDto
            {
                Files = new List<FileItemDto>()
            };
            try
            {
                long websiteId = await loginUserData.GetWebsiteId();
                string orgName = await loginUserData.GetWebsiteOrgName();
                string html = string.Empty;
                switch (dto.type)
                {
                    case GrapesPageTypeEnum.頁面:
                        var menu = await db.WebMenus.Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId && e.Id == dto.Id).FirstOrDefaultAsync();
                        if (menu != null) html = menu.SaveCss + menu.SaveHtml;
                        break;
                    case GrapesPageTypeEnum.文章:
                        var art = await db.Article.Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId && e.Id == dto.Id).FirstOrDefaultAsync();
                        if (art != null) html = art.SaveCss + art.SaveHtml;
                        break;
                    case GrapesPageTypeEnum.商品:
                        var prod = await db.Prods.Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId && e.Id == dto.Id).FirstOrDefaultAsync();
                        if (prod != null) html = prod.SaveCss + prod.SaveHtml;
                        break;
                    case GrapesPageTypeEnum.技術文件:
                        var tech = await db.TechnicalCertificates.Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId && e.Id == dto.Id).FirstOrDefaultAsync();
                        if (tech != null) html = tech.Css + tech.Html;
                        break;
                    case GrapesPageTypeEnum.頁尾:
                        var footer = await templatesApplicationService.GetDefaultFooterTemplatesAsync();
                        if (footer != null && footer.Success && footer.Object != null) html =
                                 ((TemplateSectionsDto)footer.Object).footerTemplateDto?.css ?? "" +
                                ((TemplateSectionsDto)footer.Object).footerTemplateDto?.html ?? "";
                        break;
                }
                if (!string.IsNullOrEmpty(html))
                {
                    List<string> list = new List<string>();
                    Regex r = new Regex(@"\/upload\/(.*?)(\.)");
                    var match = r.Match(html);
                    while (match.Success)
                    {
                        var s = match.Value.ToString().Split("/");
                        if (s.Length != 0)
                        {
                            list.Add(s[s.Length - 1].Replace(".", "").ToLower());
                        }
                        match = match.NextMatch();
                    }
                    var files = db.FileUploads
                                .Where(e => e.FK_WebsiteId == websiteId)
                                .Where(e => !e.IsDeleted)
                                .Where(e => e.FileGuid != null && list.Contains(e.FileGuid.ToString().ToLower()));
                    var result = from file in files
                                 select new FileItemDto
                                 {
                                     Guid = file.GuidKey,
                                     Name = file.OriginalFileName,
                                     Path = (file.DownloadFileName ?? "").Replace(@"\", "/").Replace("/upload/", $"/upload/{orgName}/"),
                                 };
                    response.Files = await result.ToListAsync();
                }
                else response.Files = new List<FileItemDto>();

                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
    }
}
