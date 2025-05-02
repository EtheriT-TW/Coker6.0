using AutoMapper;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DevExpress.Xpo.Helpers.AssociatedCollectionCriteriaHelper;

namespace EtheriT.Coker.Application.Templates
{
    public class TemplatesApplicationService: ITemplatesApplicationService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        public TemplatesApplicationService(CokerDbContext db, LoginUserData loginUserData, IMapper mapper, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }
        public async Task<TemplatesDto?> GetDefaultTemplatesAsync()
        {
            var items = httpContextAccessor.HttpContext?.Items;
            const string key = "CurrentTemplate";
            if (items != null && items.ContainsKey(key))
                return items[key] as TemplatesDto;

            var templatesDto = new TemplatesDto();
            var WebsiteID = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
            try
            {
                var qurry = await db.Templates.Include( e => e.Website).Where(x => x.FK_WebsiteID == WebsiteID && x.Enable).OrderByDescending(e => e.LastModificationTime ?? e.CreationTime).FirstOrDefaultAsync();
                if (qurry != null)
                {
                    mapper.Map(qurry, templatesDto);
                    var sectionList = await db.TemplateSections
                        .Where(x => x.FK_TemplateID == qurry.Id)
                        .ToListAsync();

                    templatesDto.templateSections = mapper.Map<List<TemplateSectionsDto>>(sectionList);
                    var footerSection = templatesDto.templateSections.Find(e => e.sectionType == Shared.Dto.enumType.Template.SectionTypeEnum.頁尾);
                    if (footerSection != null)
                    {
                        var footerEntity = await db.FooterTemplates
                            .FirstOrDefaultAsync(x => x.FK_TemplateSectionsId == footerSection.Id);

                        if (footerEntity != null)
                        {
                            footerSection.footerTemplateDto = mapper.Map<FooterTemplateDto>(footerEntity);
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
    }
}
