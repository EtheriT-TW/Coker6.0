using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Application.Webs.Dto;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace EtheriT.Coker.Application
{
    public class WebsiteApplication : IWebsiteApplication
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly string ApplicationName;
        public WebsiteApplication(
            CokerDbContext db,
            LoginUserData loginUserData,
            IHttpContextAccessor httpContextAccessor
        )
        {
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.loginUserData = loginUserData;
            ApplicationName = "Website";

        }
        public async Task<DefaultDataDto> GetDefaultData(long siteId, string? website)
        {
            if (website != null && !website.Equals("upload"))
            {
                var tempid = await GetSiteId(siteId, website.ToString());
                if (tempid != 0)
                {
                    siteId = await GetSiteId(siteId, website.ToString());
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
        public async Task<long> GetSiteId(long father_id, string key)
        {
            var childId = await (from w in db.MappingWebsiteRelationship
                                 where w.FatherId == father_id
                                 select w.WebsiteId).ToListAsync();
            if (childId != null)
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
            var date = from w in db.Websites
                       where w.Id == SiteId
                       select new WebsDto
                       {
                           Id = w.Id,
                           Name = w.Title,
                           Description = w.Description ?? "",
                           Images = w.Icon ?? ""
                       };
            return null;
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
    }
}
