using AutoMapper;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Templates
{
    public class TemplatesApplicationService: ITemplatesApplicationService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        public TemplatesApplicationService(CokerDbContext db, LoginUserData loginUserData, IMapper mapper, IConfiguration configuration)
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.configuration = configuration;
        }
        public async Task<TemplatesDto?> GetDefaultTemplatesAsync()
        {
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
                }
                else templatesDto = null;
                return templatesDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
