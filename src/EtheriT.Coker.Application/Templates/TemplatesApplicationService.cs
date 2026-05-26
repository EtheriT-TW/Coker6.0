using AutoMapper;
using DevExpress.ReportServer.ServiceModel.DataContracts;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Processor;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Dto.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DevExpress.Xpo.Helpers.AssociatedCollectionCriteriaHelper;

namespace EtheriT.Coker.Application.Templates
{
    public class TemplatesApplicationService : ITemplatesApplicationService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly StringHandler stringHandler;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly IHtmlProcessor htmlProcessor;
        private readonly IHtmlSanitizeService htmlSanitizeService;
        public TemplatesApplicationService(
            CokerDbContext db, LoginUserData loginUserData, StringHandler stringHandler,
            IMapper mapper, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IHtmlProcessor htmlProcessor,
            IFileUploadAppService fileUploadAppService, IHtmlSanitizeService htmlSanitizeService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.stringHandler = stringHandler;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.htmlProcessor = htmlProcessor;
            this.fileUploadAppService = fileUploadAppService;
            this.htmlSanitizeService = htmlSanitizeService;
        }
        public async Task<TemplatesDto?> GetDefaultTemplatesAsync()
        {
            var items = httpContextAccessor.HttpContext?.Items;
            const string key = "CurrentTemplate";
            if (items != null && items.ContainsKey(key))
                return items[key] as TemplatesDto;

            var templatesDto = new TemplatesDto();
            var isFront = configuration.GetValue<long>("WebConfig:SiteId") != 0;
            var WebsiteID = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
            try
            {
                var website = await db.Websites.FirstOrDefaultAsync(e => e.Id == WebsiteID);
                if (website == null) throw new Exception("找不到網站資料");
                else if (!new List<int?> { 7, 8 }.Contains(website.LayoutType)) return null;

                var qurry = await db.Templates.Include(e => e.Website).Where(x => x.FK_WebsiteID == WebsiteID && x.Enable).OrderByDescending(e => e.LastModificationTime ?? e.CreationTime).FirstOrDefaultAsync();
                if (qurry != null)
                {
                    mapper.Map(qurry, templatesDto);
                    var sectionList = await db.TemplateSections
                        .Where(x => x.FK_TemplateID == qurry.Id)
                        .ToListAsync();

                    templatesDto.templateSections = mapper.Map<List<TemplateSectionsDto>>(sectionList);
                    var footerSection = templatesDto.templateSections.Find(e => e.sectionType == SectionTypeEnum.頁尾);
                    if (footerSection != null)
                    {
                        var footerEntity = await db.FooterTemplates
                            .FirstOrDefaultAsync(x => x.FK_TemplateSectionsId == footerSection.Id);

                        if (footerEntity != null)
                        {
                            footerSection.footerTemplateDto = mapper.Map<FooterTemplateDto>(footerEntity);

                            if (isFront)
                            {
                                var html = stringHandler.HtmlDecode(footerEntity.html ?? "");
                                var css = footerEntity.css ?? "";

                                if (!string.IsNullOrWhiteSpace(html) || !string.IsNullOrWhiteSpace(css))
                                {
                                    try
                                    {
                                        var sanitized = await EnsureFooterDisplayContentSanitizedAsync(
                                            WebsiteID,
                                            footerEntity.Id,
                                            html,
                                            css
                                        );

                                        html = sanitized.Html;
                                        css = sanitized.Css;
                                    }
                                    catch (Exception ex)
                                    {
                                        await loginUserData.SetLogs(
                                            JsonConvert.SerializeObject(new
                                            {
                                                Action = "AutoRepairHtmlSanitizeOnReadFailed",
                                                SourceType = HtmlSanitizeSourceType.頁尾,
                                                SourceId = footerEntity.Id,
                                                WebsiteId = WebsiteID
                                            }),
                                            JsonConvert.SerializeObject(new
                                            {
                                                Success = false,
                                                Error = ex.Message
                                            })
                                        );
                                    }
                                }

                                footerSection.footerTemplateDto.html = stringHandler.HtmlEncode(html);
                                footerSection.footerTemplateDto.css = css;
                            }
                        }
                    }

                }
                else templatesDto = null;
                if (items != null) items[key] = templatesDto;
                return templatesDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<ResponseMessageDto> GetDefaultFooterTemplatesAsync()
        {
            var response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var templateSections = await db.TemplateSections
                    .Include(x => x.template)
                    .Include(x => x.footerTemplates)
                    .Where(x => x.sectionType == SectionTypeEnum.頁尾 && x.template.FK_WebsiteID == websiteId)
                    .FirstOrDefaultAsync();
                if (templateSections != null && templateSections.footerTemplates != null)
                {
                    response.Object = mapper.Map<TemplateSectionsDto>(templateSections);
                }
                else
                {
                    var temp = await getDefaultTemplate();
                    var section = await getDefaultTemplateSections(temp.Id, SectionTypeEnum.頁尾);
                    response.Object = mapper.Map<TemplateSectionsDto>(section);
                }
                response.Success = true;
            }
            catch (Exception e)
            {
                response.Error = e.Message;
            }

            return response;
        }
        private async Task<Template> getDefaultTemplate()
        {
            var websiteId = await loginUserData.GetWebsiteId();
            var template = await db.Templates.Where(e => e.FK_WebsiteID == websiteId && e.Enable).FirstOrDefaultAsync();
            if (template == null)
            {
                var web = await db.Websites.FirstOrDefaultAsync(e => e.Id == websiteId);
                template = new Template
                {
                    Name = $"{web.Title}_{DateTime.Now.ToString("yyyyMMdd")}",
                    FK_WebsiteID = websiteId,
                    LayoutType = LayoutTypeEnum.單欄,
                    HeadType = HeadTypeEnum.logo在上選單在下,
                    templateTypeEnum = TemplateTypeEnum.Private,
                    FK_ThemeId = 0,
                    LayoutConfig = string.Empty,
                    Enable = true,
                };
                db.Templates.Add(template);
                await loginUserData.SaveChanges(template);
            }
            return template;
        }
        private async Task<TemplateSections> getDefaultTemplateSections(long TempId, SectionTypeEnum typeEnum)
        {
            var section = await db.TemplateSections
                .Include(x => x.template)
                .Include(x => x.footerTemplates)
                .Where(x => x.FK_TemplateID == TempId && x.sectionType == typeEnum)
                .FirstOrDefaultAsync();

            if (section == null)
            {
                var temp = await getDefaultTemplate();
                section = new TemplateSections
                {
                    FK_TemplateID = TempId,
                    sectionType = typeEnum,
                    ContentConfig = string.Empty,
                    template = temp
                };
                if (typeEnum == SectionTypeEnum.頁尾)
                {
                    var footer = new FooterTemplate
                    {
                        saveCss = string.Empty,
                        saveHtml = string.Empty,
                    };
                    section.footerTemplates = footer;
                }
                db.TemplateSections.Add(section);
                await loginUserData.SaveChanges(section);
            }
            else if (section.footerTemplates == null && typeEnum == SectionTypeEnum.頁尾)
            {
                var footer = new FooterTemplate
                {
                    FK_TemplateSectionsId = section.Id,
                    saveCss = string.Empty,
                    saveHtml = string.Empty,
                };
                section.footerTemplates = footer;
                await loginUserData.SaveChanges(section);
            }
            return section;
        }
        public async Task<ResponseMessageDto> importDefaultFooter(MenuSaveContenDto dto)
        {
            var response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var foot = await db.FooterTemplates
                    .Include(x => x.templateSections)
                    .ThenInclude(x => x.template)
                    .Where(x => x.Id == dto.Id && x.templateSections.template.FK_WebsiteID == websiteId)
                    .FirstOrDefaultAsync();
                if (foot != null)
                {
                    await saveDefaultFooter(dto);
                    FooterTemplate importDto = new FooterTemplate
                    {
                        Id = dto.Id,
                        html = dto.SaveHtml,
                        css = dto.SaveCss
                    };
                    string Orgname = await loginUserData.GetWebsiteOrgName();

                    importDto.html = stringHandler.HtmlDecode(importDto.html ?? "");
                    importDto.html = (importDto.html ?? "").Replace($"/upload/{Orgname}/", "/upload/");
                    importDto.css = (importDto.css ?? "").Replace($"/upload/{Orgname}/", "/upload/");

                    var sanitizeResult = await htmlSanitizeService.EnsurePublicContentAsync(new HtmlSanitizeInput
                    {
                        WebsiteId = websiteId,
                        SourceType = HtmlSanitizeSourceType.頁尾,
                        SourceId = foot.Id,
                        ContentKey = "Published",
                        SanitizePolicy = "PublicHtml",
                        Html = importDto.html ?? "",
                        Css = importDto.css ?? "",
                        Force = true
                    });

                    foot.css = sanitizeResult.Css;
                    foot.html = stringHandler.HtmlEncode(sanitizeResult.Html);

                    await loginUserData.SaveChanges(foot);
                    response.Success = true;
                }
                else throw new Exception("找不到頁尾資料");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            finally
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }
        public async Task<ResponseMessageDto> saveDefaultFooter(MenuSaveContenDto dto)
        {
            var response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var foot = await db.FooterTemplates
                    .Include(x => x.templateSections)
                    .ThenInclude(x => x.template)
                    .Where(x => x.Id == dto.Id && x.templateSections.template.FK_WebsiteID == websiteId)
                    .FirstOrDefaultAsync();
                if (foot != null)
                {
                    dto.SaveHtml = stringHandler.HtmlEncode(dto.SaveHtml);
                    foot.saveHtml = dto.SaveHtml;
                    foot.saveCss = dto.SaveCss;
                    await loginUserData.SaveChanges(foot);
                    response.Success = true;
                }
                else throw new Exception("找不到頁尾資料");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            finally
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }
        public async Task<ResponseMessageDto> saveDefaultHeader(HeaderTemplateDto dto)
        {
            var response = new ResponseMessageDto();
            try
            {
                var temp = await getDefaultTemplate();
                var header = await getDefaultTemplateSections(temp.Id, SectionTypeEnum.表頭);
                if (header != null)
                {
                    var orgName = await loginUserData.GetWebsiteOrgName();
                    header.template.HeadType = dto.HeadType;
                    dto.ContentConfig.Sliders.ForEach(e =>
                    {
                        if (!string.IsNullOrEmpty(e.DesktopImage) && !e.DesktopImage.StartsWith("http"))
                            e.DesktopImage = e.DesktopImage.Replace($"/upload/{orgName}/", "/upload/");
                        if (!string.IsNullOrEmpty(e.MobileImage) && !e.MobileImage.StartsWith("http"))
                            e.MobileImage = e.MobileImage.Replace($"/upload/{orgName}/", $"/upload/");
                    });

                    if (!string.IsNullOrEmpty(header.ContentConfig))
                    {
                        var config = JsonConvert.DeserializeObject<HeaderContentConfigDto>(header.ContentConfig);
                        if (config != null)
                        {
                            foreach (var e in config.Sliders)
                            {
                                var isDeleted = !dto.ContentConfig.Sliders.Any(x => x.DesktopImage == e.DesktopImage);
                                if (!string.IsNullOrEmpty(e.DesktopImage) && !e.DesktopImage.StartsWith("http") && isDeleted)
                                {
                                    var file = db.FileUploads.Where(f => f.DownloadFileName == e.DesktopImage).FirstOrDefault();
                                    if (file != null) await fileUploadAppService.deleteFile(file.GuidKey);
                                }

                                isDeleted = !dto.ContentConfig.Sliders.Any(x => x.MobileImage == e.MobileImage);
                                if (!string.IsNullOrEmpty(e.MobileImage) && !e.MobileImage.StartsWith("http") && isDeleted)
                                {
                                    var file = db.FileUploads.Where(f => f.DownloadFileName == e.MobileImage).FirstOrDefault();
                                    if (file != null) await fileUploadAppService.deleteFile(file.GuidKey);
                                }
                            }
                        }
                    }
                    header.ContentConfig = JsonConvert.SerializeObject(dto.ContentConfig);
                    await loginUserData.SaveChanges(header);
                    response.Success = true;
                }
                else throw new Exception("找不到頁首資料");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            finally
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }
        public async Task<ResponseMessageDto> getDefaultHeader()
        {
            var response = new ResponseMessageDto();
            try
            {
                var temp = await getDefaultTemplate();
                var header = await getDefaultTemplateSections(temp.Id, SectionTypeEnum.表頭);
                if (header != null)
                {
                    var dto = mapper.Map<HeaderTemplateDto>(header);
                    HeaderContentConfigDto? config = JsonConvert.DeserializeObject<HeaderContentConfigDto>(header.ContentConfig);
                    if (config != null)
                    {
                        var orgName = await loginUserData.GetWebsiteOrgName();
                        config.Sliders.ForEach(e =>
                        {
                            if (!string.IsNullOrEmpty(e.DesktopImage) && !e.DesktopImage.StartsWith("http"))
                                e.DesktopImage = e.DesktopImage.Replace("/upload/", $"/upload/{orgName}/");
                            if (!string.IsNullOrEmpty(e.MobileImage) && !e.MobileImage.StartsWith("http"))
                                e.MobileImage = e.MobileImage.Replace("/upload/", $"/upload/{orgName}/");
                        });
                        dto.ContentConfig = config;
                    }
                    response.Object = dto;
                    response.Success = true;
                }
                else throw new Exception("找不到頁首資料");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            return response;
        }
        private async Task<(string Html, string Css)> EnsureFooterDisplayContentSanitizedAsync(
    long websiteId,
    long footerTemplateId,
    string html,
    string css)
        {
            var sanitizeResult = await htmlSanitizeService.EnsurePublicContentAsync(new HtmlSanitizeInput
            {
                WebsiteId = websiteId,
                SourceType = HtmlSanitizeSourceType.頁尾,
                SourceId = footerTemplateId,
                ContentKey = "Published",
                SanitizePolicy = "PublicHtml",
                Html = html ?? "",
                Css = css ?? "",
                Force = false
            });

            if (!sanitizeResult.WasSanitized)
            {
                return (html ?? "", css ?? "");
            }

            var footer = await db.FooterTemplates
                .Include(x => x.templateSections)
                .ThenInclude(x => x.template)
                .FirstOrDefaultAsync(x =>
                    x.Id == footerTemplateId &&
                    x.templateSections.template.FK_WebsiteID == websiteId
                );

            if (footer == null)
            {
                return (sanitizeResult.Html, sanitizeResult.Css);
            }

            var before = new
            {
                Html = stringHandler.HtmlDecode(footer.html ?? ""),
                Css = footer.css ?? ""
            };

            footer.html = stringHandler.HtmlEncode(sanitizeResult.Html);
            footer.css = sanitizeResult.Css;

            await loginUserData.SaveChanges(footer);

            await loginUserData.SetLogs(
                JsonConvert.SerializeObject(new
                {
                    Action = "AutoRepairHtmlSanitizeOnRead",
                    SourceType = HtmlSanitizeSourceType.頁尾,
                    SourceId = footer.Id,
                    WebsiteId = websiteId,
                    ContentKey = "Published",
                    SanitizePolicy = "PublicHtml",
                    Before = before
                }),
                JsonConvert.SerializeObject(new
                {
                    Html = sanitizeResult.Html,
                    Css = sanitizeResult.Css,
                    Hash = sanitizeResult.ContentHash,
                    Version = sanitizeResult.SanitizeVersion,
                    WasSanitized = sanitizeResult.WasSanitized
                })
            );

            return (sanitizeResult.Html, sanitizeResult.Css);
        }
    }
}
