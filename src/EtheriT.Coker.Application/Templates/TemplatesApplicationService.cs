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
using System;
using System.Collections.Generic;
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
        private readonly IHtmlProcessor htmlProcessor;
        public TemplatesApplicationService(
            CokerDbContext db, LoginUserData loginUserData, StringHandler stringHandler,
            IMapper mapper, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IHtmlProcessor htmlProcessor
        ) {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.stringHandler = stringHandler;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.htmlProcessor = htmlProcessor;
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
                            if (isFront) {
                                footerSection.footerTemplateDto.html = footerEntity.html;
                                footerSection.footerTemplateDto.css = footerEntity.css;
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
        public async Task<ResponseMessageDto> GetDefaultFooterTemplatesAsync() {
            var response = new ResponseMessageDto();
            try {
                var websiteId = await loginUserData.GetWebsiteId();
                var templateSections = await db.TemplateSections
                    .Include(x => x.template)
                    .Include(x => x.footerTemplates)
                    .Where(x => x.sectionType == SectionTypeEnum.頁尾 && x.template.FK_WebsiteID == websiteId)
                    .FirstOrDefaultAsync();
                if (templateSections != null)
                {
                    response.Object = mapper.Map<TemplateSectionsDto>(templateSections);
                }
                else {
                    var temp = await getDefaultTemplate();
                    var section = await getDefaultTemplateSections(temp.Id, SectionTypeEnum.頁尾);
                    response.Object = mapper.Map<TemplateSectionsDto>(section);
                }
                response.Success = true;
            } catch (Exception e) {
                response.Error = e.Message;
            }

            return response;
        }
        private async Task<Template> getDefaultTemplate() {
            var websiteId = await loginUserData.GetWebsiteId();
            var template = await db.Templates.Where(e => e.FK_WebsiteID == websiteId && e.Enable).FirstOrDefaultAsync();
            if (template == null) {
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
                .Include(x => x.footerTemplates)
                .Where(x => x.FK_TemplateID == TempId && x.sectionType == typeEnum)
                .FirstOrDefaultAsync();

            if (section == null)
            {
                section = new TemplateSections
                {
                    FK_TemplateID = TempId,
                    sectionType = typeEnum,
                    ContentConfig = string.Empty,
                };
                if (typeEnum == SectionTypeEnum.頁尾)
                {
                    var footer = new FooterTemplate
                    {
                        FK_TemplateSectionsId = section.Id,
                        saveCss = string.Empty,
                        saveHtml = string.Empty,
                    };
                    section.footerTemplates = footer;
                }
                db.TemplateSections.Add(section);
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
                    importDto.html = stringHandler.HtmlDecode(importDto.html);
                    importDto.html = htmlProcessor.RemoveNode(importDto.html ?? "", ".backstageType");
                    importDto.html = (importDto.html ?? "").Replace($"/upload/{Orgname}/", "/upload/");
                    foot.css = (importDto.css ?? "").Replace($"/upload/{Orgname}/", "/upload/");
                    foot.html = stringHandler.HtmlEncode(importDto.html);
                    await loginUserData.SaveChanges(foot);
                    response.Success = true;
                }
                else throw new Exception("找不到頁尾資料");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            finally {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }
        public async Task<ResponseMessageDto> saveDefaultFooter(MenuSaveContenDto dto) {
            var response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var foot = await db.FooterTemplates
                    .Include(x => x.templateSections)
                    .ThenInclude(x => x.template)
                    .Where(x => x.Id == dto.Id && x.templateSections.template.FK_WebsiteID == websiteId)
                    .FirstOrDefaultAsync();
                if (foot != null) {
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
    }
}
