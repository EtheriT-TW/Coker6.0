using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Application.Webs.Dto;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EtheriT.Coker.Application
{
    public class WebsiteApplication : IWebsiteApplication
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly string ApplicationName;
        private readonly IConfiguration Configuration;
        private readonly IMapper mapper;
        public WebsiteApplication(
            CokerDbContext db,
            LoginUserData loginUserData,
            IConfiguration Configuration,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper
        )
        {
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.loginUserData = loginUserData;
            this.Configuration = Configuration;
            this.mapper = mapper;
            ApplicationName = "Website";

        }
        public async Task<DefaultDataDto> GetDefaultData(long siteId, string? website)
        {
            long fid = siteId;
            string ParntOrgNames = "";
            if (website != null && !website.Equals("upload"))
            {
                ParntOrgNames = await GetParntOrgName(fid);
                var tempid = await GetSiteId(siteId, website.ToString());
                if (tempid != 0)
                {
                    siteId = tempid;
                }
            }
            var orgname = await GetOrgName(siteId);
            orgname = (orgname == null || orgname == "") ? "Page" : orgname;
            var Layout_Type = await GetLayoutType(siteId);
            var view = Layout_Type == 0 ? "Default" : $"Layout_{Layout_Type}";

            DefaultDataDto defaultData = new DefaultDataDto
            {
                Id = siteId,
                OrgName = orgname,
                ParntOrgNames = ParntOrgNames,
                Layout_Type = Layout_Type,
                View = view,
            };

            return defaultData;
        }
        public async Task<int> GetLayoutType(long Id)
        {
            var data = await (from w in db.Websites
                              where w.Id == Id
                              select w.LayoutType).FirstOrDefaultAsync();

            return data == null ? 0 : int.Parse(data.ToString());
        }
        public async Task<string> GetOrgName(long Id)
        {
            var data = await (from w in db.Websites
                              where w.Id == Id
                              select w.OrgName).FirstOrDefaultAsync();

            return data == null ? "Page" : data.ToString();
        }
        private async Task<string> GetParntOrgName(long Id)
        {
            var re = await db.MappingWebsiteRelationship
                .Where(e => !e.IsDeleted)
                .Where(e => e.FatherId== Id)
                .Select(e => e.WebsiteId).ToListAsync();
            var data = await (from w in db.Websites
                              where w.Id == Id || re.Contains(w.Id)
                              select w.OrgName).ToListAsync();

            return data == null ? "" : String.Join("|", data.ToArray());
        }
        public async Task<long> GetSiteId(long father_id, string key)
        {
            var childId = await (from w in db.MappingWebsiteRelationship
                                 where w.FatherId == father_id
                                 select w.WebsiteId).ToListAsync();
            if (childId.Count() > 0)
            {
                var id = await (from w in db.Websites
                                where w.OrgName == key
                                select w.Id).FirstOrDefaultAsync();
                if (childId.Contains(id))
                {
                    return id;
                }
            }
            return 0;
        }

        [Authorize]
        public async Task<List<WebsDto>> GetAll()
        {
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            var date = from w in db.Websites
                       join bind in db.MappingUserAndWebsites on w.Id equals bind.WebsiteId
                       join u in db.Users on bind.UserId equals u.Id
                       where u.Account == name
                       select new WebsDto
                       {
                           Id = w.Id,
                           Name = w.Title,
                           Description = w.Description ?? "",
                           Images = w.Icon ?? ""
                       };
            if (date.Any())
            {
                long siteId = await loginUserData.GetWebsiteId();
                var output = await date.ToListAsync();
                if (siteId == 0 && output.Any())
                {
                    siteId = output.FirstOrDefault().Id;
                    await Exchange(new WebExchangeDto { Id = siteId });
                }
                var item = output.Find(e => e.Id == siteId);
                if (item != null) item.Check = true;
                return output;
            }
            else return new List<WebsDto>();
        }
        public async Task<List<WebsiteDataDto>> GetAllData(long SiteId)
        {
            var issubsite = await (from w in db.MappingWebsiteRelationship
                                   where w.WebsiteId == SiteId
                                   where w.IsDeleted != true
                                   select w.Id).ToListAsync();
            var date = await (from w in db.Websites
                              where w.Id == SiteId
                              where w.IsDeleted != true
                              select new WebsiteDataDto
                              {
                                  Id = w.Id,
                                  Title = w.Title ?? "",
                                  OrgName = w.OrgName ?? "",
                                  Logo = w.Logo ?? "",
                                  isSubsite = issubsite == null ? false : true,
                              }).ToListAsync();
            return date;
        }
        public async Task<ResponseMessageDto> GetPrivacyAndTerms()
        {
            var response = new ResponseMessageDto() { Success = false };
            var websiteid = await loginUserData.GetWebsiteId();
            long privacy_id = 0;
            long terms_id = 0;
            try
            {
                var privacy = await db.WebMenus.Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteid && e.RouterName == "footer_privacy").FirstOrDefaultAsync();
                if (privacy != null) privacy_id = privacy.Id;
                var terms = await db.WebMenus.Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteid && e.RouterName == "terms").FirstOrDefaultAsync();
                if (terms != null) terms_id = terms.Id;
                response.Success = true;
                response.Message = $"{privacy_id} {terms_id}";
                return response;
            }
            catch(Exception ex)
            {
                response.Error = ex.Message;
                return response;
            }
            
        }
        public async Task<ResponseMessageDto> Exchange(WebExchangeDto dto)
        {
            ResponseMessageDto responseMessageDto = new ResponseMessageDto();
            try
            {
                if ((await loginUserData.CheckedWebSiteId(dto.Id)))
                {
                    long usetId = await loginUserData.GetUserId();
                    Guid secret = loginUserData.GetSecret();
                    var token = await db.Tokens
                                        .Where(t => t.id == secret)
                                        .Where(t => t.UserID == usetId)
                                        .FirstOrDefaultAsync();
                    var user = await db.Users.Where(e => e.Id == usetId).FirstOrDefaultAsync();
                    if (token != null)
                    {
                        token.websiteId = dto.Id;
                        responseMessageDto.Message = dto.Id.ToString();
                        db.SaveChanges();
                    }
                    else throw new Exception("金鑰失效");
                }
                else throw new Exception("網站不存在");
            }
            catch (Exception e)
            {
                responseMessageDto.Success = false;
                responseMessageDto.Error = e.Message;
            }
            await loginUserData.SetLogs(ApplicationName, "Exchange", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(responseMessageDto));
            return responseMessageDto;
        }
        public async Task<GetFrontContenOutputDto> GetPrivacyConten(GetFrontContenInputDto dto) {
            if (dto.siteId == null)
            {
                dto.siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            }
            GetFrontContenOutputDto result = new GetFrontContenOutputDto();
            try
            {
                var side = await db.Websites.Where(e => e.Id == dto.siteId).FirstOrDefaultAsync();
                if (side != null)
                {
                    result.SiteName = side.Title;
                    result.LastModificationTime = null;
                    result.Html = (result.Html??"").Replace("&lt;body&gt;", "").Replace("&lt;/body&gt;", "");
                    result.CurrentUrl = $"/Privacy";
                    result.VisibleFooter = true;
                    result.VisibleHeader = true;
                }
            }
            catch { }
            return result;
        }
        public async Task<WebsiteEditOutputDto> GetWebsiteData() {
            WebsiteEditOutputDto result = new WebsiteEditOutputDto { Success=false, Website = new WebsiteEditDto(), Company = new Company.CompanyDto() };
            long siteId = await loginUserData.GetWebsiteId();
            try
            {
                var data = await db.Websites.Include(e => e.Company).Where(e => e.Id == siteId).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                if (data != null)
                {
                    var cid = data.Company.Select(e => e.FK_CompanyId).ToList();
                    var com = await db.Companies.Where(e => cid.Contains(e.Id)).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                    mapper.Map(data, result.Website);
                    mapper.Map(com, result.Company);
                    result.Success=true;
                }
                else throw new Exception("網站不存在");
            }
            catch (Exception e) {
                result.Error = e.Message;
            }
            return result;
        }
        public async Task<ResponseMessageDto> Save(WebsiteEditDto dto) {
            var response = new ResponseMessageDto() { Success = false };
            var websiteid = await loginUserData.GetWebsiteId();
            try
            {
                if (websiteid == 0) throw new Exception("未再登入狀態");
                var data = await db.Websites.Where(e => e.Id == websiteid).Where(e => !e.IsDeleted).FirstOrDefaultAsync();

                if (data == null) throw new Exception("網站不存在");
                mapper.Map(dto,data);
                await loginUserData.SaveChanges(data);
                response.Success = true;
            }catch (Exception e)
            {
                response.Error = e.Message;
            }
            await loginUserData.SetLogs(ApplicationName, "Save", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
    }
}
