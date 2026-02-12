using AutoMapper;
using DevExpress.XtraReports.Design.ParameterEditor;
using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Authorization;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Application.Webs.Dto;
using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EtheriT.Coker.Application
{
    public class LoginUserData
    {

        private readonly CokerDbContext db;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;
        private readonly ICookieManagerAppService cookieManager;
        private readonly IMapper mapper;
        public LoginUserData(
            CokerDbContext db,
            IHttpContextAccessor httpContextAccessor,
            ICookieManagerAppService cookieManager,
            IConfiguration configuration,
            IMapper mapper
        ) {
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
            this.cookieManager = cookieManager;
            this.mapper = mapper;
        }

        public string? GetClientIP()
        {
            if (httpContextAccessor.HttpContext == null) return StringValues.Empty;
            return httpContextAccessor.HttpContext.Connection?.RemoteIpAddress?.ToString();
        }
        public async Task<long> GetUserId() {
            long id;
            try {
                if (httpContextAccessor.HttpContext == null) throw new Exception();
                ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
                string name = user.Identity?.Name;
                id = (await db.Users.Where(e => e.Account == name).FirstOrDefaultAsync())?.Id ?? 0;
            }
            catch(Exception ex)
            {
                id = 0;
            }
            return id;
        }
        public async Task<List<long>> GetUserRoleIds() {
            var uid = await GetUserId();
            var wid = await GetWebsiteId();
            var result = await (from rol in db.Roles.Where(e => !e.IsDeleted && e.FK_WebsiteId==wid)
                    join map in db.MappingUserAndRoles on rol.Id equals map.RoleId
                    where map.UserId == uid
                    select rol).Select(e => e.Id).ToListAsync();
            return result;
        }
        public async Task<UserSimplifyDto> GetUser() {
			UserSimplifyDto user = new UserSimplifyDto { Id = 0 };
            try
            {
                if (httpContextAccessor.HttpContext == null) throw new Exception();
                ClaimsPrincipal logUser = httpContextAccessor.HttpContext?.User;
                string name = logUser.Identity?.Name;
                var detail = await db.Users.Where(e => e.Account == name).FirstOrDefaultAsync();
                mapper.Map(detail, user);
            }
            catch (Exception ex)
            {
                user.Id = 0;
            }
            return user;
        }
        public async Task<UserSimplifyDto> GetUser(long id)
		{
			UserSimplifyDto user = new UserSimplifyDto { Id = 0 };
			try
			{
				var detail = await db.Users.Where(e => e.Id == id).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
				mapper.Map(detail, user);
			}
			catch (Exception ex)
			{
				user.Id = 0;
			}
			return user;
		}
        public bool IsLoggedIn()
        {
            var user = httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated ?? false;
        }
        public async Task<long> GetWebsiteId()
        {
            if (httpContextAccessor.HttpContext == null) return 0;

            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            Guid secret = GetSecret();
            var token = await db.Tokens.Where(t => t.id == secret).FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(name)) return 0;
            else if (token == null || token.websiteId == 0)
            {
                long lastWebSite = 0;
                if (httpContextAccessor.HttpContext != null)
                {
                    long.TryParse(httpContextAccessor.HttpContext.Request.Cookies["lastWebSite"], out lastWebSite);
                }
                if (await CheckedWebSiteId(lastWebSite))
                {
                    if (token != null && token.websiteId != lastWebSite)
                    {
                        token.websiteId = lastWebSite;
                        db.SaveChanges();
                    }
                    return lastWebSite;
                }
                else {
                    var date = from w in db.Websites
                               join bind in db.MappingUserAndWebsites on w.Id equals bind.WebsiteId
                               join u in db.Users on bind.UserId equals u.Id
                               where u.Account == name
                               select new WebsDto
                               {
                                   Id = w.Id
                               };
                    if (date.Any())
                    {
                        var websiteId = (await date.FirstOrDefaultAsync()).Id;
                        if (token != null && token.websiteId != websiteId)
                        {
                            token.websiteId = websiteId;
                            db.SaveChanges();
                        }
                        return websiteId;
                    }
                    else return 0;
                }
            }
            else
            {
                var isSysUser = await isSystemUser();
                var userDetail = await db.Users.Where(u => u.Id == token.UserID).FirstOrDefaultAsync();
                if (userDetail == null) return 0;
                else if (isSysUser) {
                    var website = await db.Websites.Where(e => e.Id == token.websiteId).FirstOrDefaultAsync();
                    return website == null ? 0 : website.Id;
                }
                var check = db.MappingUserAndWebsites.Where(m => m.UserId == userDetail.Id).Where(m => m.WebsiteId == token.websiteId);
                int c = check.Count();
                if (check.Any()) return token.websiteId;
                else return 0;
            }
        }
        public async Task<bool> IsExtraSuperUser() {
            bool isThis = false;
            var user = await GetUser();
            if (user == null) return false;

            var role = await db.Roles.Where(e => !e.IsDeleted && e.IsSuperUser && e.FK_WebsiteId == 1).FirstOrDefaultAsync();
            if (role != null)
            {
                var perm = await db.MappingUserAndRoles.Where(e => e.UserId == user.Id && e.RoleId == role.Id).FirstOrDefaultAsync();
                isThis = perm == null;
            }
            return isThis;
        }
        public long GetFrontWebsiteId() {
            return configuration.GetValue<long>("WebConfig:SiteId");
        }
        public List<string> GetFrontChildOrgName()
        {
            var list = configuration.GetSection("WebConfig:childSiteOrgName").Get<List<string>>();
            if (list == null) list = new List<string>();
            return list;
        }
        public async Task<string> GetWebsiteName() {
            Guid s = GetSecret();
            string name = "";
            var t = from token in db.Tokens.Where(o => o.id == s)
                    join web in db.Websites on token.websiteId equals web.Id
                    select web;
            var myWeb = await t.FirstOrDefaultAsync();
            if (myWeb != null) name = myWeb.Title;
            return name;
        }
        public async Task<string> GetWebsiteOrgName()
        {
            long id = await GetWebsiteId();
            string name = "";
            try
            {
                var frontWebsiteId = GetFrontWebsiteId();
                if (frontWebsiteId == 0 && id != 0)
                {
                    var website = await db.Websites.Where(w => w.Id == id).FirstOrDefaultAsync();
                    if(website != null) name = website.OrgName;
                }
                else throw new Exception();
            }
            catch {}
            return name;
        }
        public async Task<string> GetWebsiteOrgName(long id) {
            string name = "";
            try
            {
                if (id != 0)
                {
                    var website = await db.Websites.Where(w => w.Id == id).FirstOrDefaultAsync();
                    if (website != null) name = website.OrgName;
                }
                else throw new Exception();
            }
            catch { }
            return name;
        }
        public async Task<List<WebSiteOrgNameDto>> GetAllFrontWebsiteIdAndOrgName() {
            var websiteId = GetFrontWebsiteId();
            var orgNames = GetFrontChildOrgName();
            var w = await db.Websites.Where(e => orgNames.Contains(e.OrgName) || websiteId == e.Id).Select(e => new WebSiteOrgNameDto { Id = e.Id,Level = e.Level, OrgName = e.OrgName }).ToListAsync();
            return w ?? new List<WebSiteOrgNameDto>();
        }
        public async Task<string> GetWebsiteLocal() {
            string local = "";
            try
            {
                long id = await GetWebsiteId();
                if (id == 0) id = GetFrontWebsiteId();
                var website = await db.Websites.Where(w => w.Id == id).FirstOrDefaultAsync();
                if (website != null) local = website.Locale;
            }
            catch { }
            return local;
        }
        public async Task<int> GetWebsiteUseFrameLevel()
        {
            long id = await GetWebsiteId();
            int level = 0;
            try
            {
                if (id != 0)
                {
                    var website = await db.Websites.Where(w => w.Id == id).FirstOrDefaultAsync();
                    if (website != null) level = website.LayoutType??0;
                }
                else throw new Exception();
            }
            catch { }
            return level;
        }
        public async Task<string> GetWebsiteUrl()
        {
            Guid s = GetSecret();
            string url = "";
            var t = from token in db.Tokens.Where(o => o.id == s)
                    join web in db.Websites on token.websiteId equals web.Id
                    select web;
            var myWeb = await t.FirstOrDefaultAsync();
            if (myWeb != null) url = myWeb.DefaultUrl??"";
            return url;
        }
        public async Task<string> GetFrontWebsiteUrl() { 
            long siteId = GetFrontWebsiteId();
            var site = await db.Websites.Where(e => !e.IsDeleted && e.Id == siteId).FirstOrDefaultAsync();
            if (site != null) return site.DefaultUrl ?? "";
            else return "";
        }
        public async Task<WebsiteLevelEnum> GetWebsiteLevel(long id)
        {
            if (id != 0) return await db.Websites.Where(e => e.Id == id).Select(e => e.Level).FirstOrDefaultAsync();
            else return WebsiteLevelEnum.形象;
        }
        public async Task<WebsiteLevelEnum> GetWebsiteLevel() {
            long id = await GetWebsiteId();
            if (id != 0) return await GetWebsiteLevel(id);
            else return WebsiteLevelEnum.形象;
        }
        public string GetAuthorization() {
            if (httpContextAccessor.HttpContext == null) return StringValues.Empty;
            var authorizationHeader = httpContextAccessor
                .HttpContext.Request.Headers["Authorization"];

            if (authorizationHeader == StringValues.Empty) {
                return StringValues.Empty;
            } else {
                return authorizationHeader.Single().Split(" ").Last();
            }
        }
        [Authorize]
        public Guid GetSecret()
        {
            if (httpContextAccessor.HttpContext == null) return new Guid();
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            string secret = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if(string.IsNullOrEmpty(secret)) secret = cookieManager.Get("BackstageRefreshToken");
            return string.IsNullOrEmpty(secret) ? new Guid() : Guid.Parse(secret);
        }
        public async Task<bool> CheckedWebSiteId(long id) {
            bool check = false;
            long userId = await GetUserId();
            check = await isSystemUser();
            if(check) return true;
            if (userId != 0) {
                var userDetail = db.Users.Where(u => u.Id == userId).Where(u => !u.IsDeleted);
                if (userDetail.Any())
                {
                    var data = db.MappingUserAndWebsites.Where(m => m.UserId == userId).Where(m => m.WebsiteId == id);
                    if (data.Any()) check = true;
                }
            }
            return check;
        }
        public async Task<bool> isSystemUser()
        {
            var userId = await GetUserId();
            var data = db.MappingUserAndRoles.Include(e => e.Role).Where(e => e.UserId == userId && e.Role!.Type == RoleTypeEnum.系統維護);
            return data.Any();
        }
        public async Task<bool> CheckedWebSiteId(long userId,long websiteId) {
            bool check = false;
            try {
                var data = await db.MappingUserAndWebsites.Where(m => m.UserId == userId).Where(m => m.WebsiteId == websiteId).FirstOrDefaultAsync();
                if (data != null) check = true;
            }catch(Exception) {
                check = false;
            }
            return check;
        }
        public async Task SaveChanges(FullAuditedEntity entity) {
            await setOptionParameter(entity);
            db.SaveChanges();
        }
        public async Task SaveChanges(IEnumerable<object> entities) {
            var list = entities.ToList();

            foreach (var x in list)
            {
                await setOptionParameter((FullAuditedEntity)x);
            }

            await db.SaveChangesAsync();
        }
        public async Task setOptionParameter(FullAuditedEntity entity) {
            var user = await GetUser();
            setOptionParameter(entity,user.Id);
        }
        public void setOptionParameter(FullAuditedEntity entity,long userId) {
            var entry = db.Entry(entity);
            var now = DateTime.Now;
            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatorUserId = userId;
                    entity.CreationTime = now;
                    break;

                case EntityState.Modified:
                    if (entity.IsDeleted)
                    {
                        entity.DeleterUserId = userId;
                        entity.DeletionTime = now;
                    }
                    else
                    {
                        entity.LastModifierUserId = userId;
                        entity.LastModificationTime = now;
                    }
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.DeleterUserId = userId;
                    entity.DeletionTime = now;
                    break;
            }
        }
        public async Task SetLogs(string Paramater, string response) {
            var user = await GetUser();
            var routeData = httpContextAccessor.HttpContext.GetRouteData();
			var Action = routeData.Values["action"]?.ToString();
			var Controller = routeData.Values["controller"]?.ToString();
            var WebsiteID = await GetWebsiteId();

            if (!WebsiteID.IsNullOrEmpty())
            {
                db.AuditLogs.Add(new Core.Models.AuditLog
                {
                    ClientIpAddress = GetClientIP(),
                    BrowserInfo = httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString(),
                    ExecutionTime = DateTime.Now,
                    MethodName = Action,
                    Parameters = Paramater,
                    ServiceName = Controller,
                    ReturnValue = response,
                    UserId = user.Id,
                    ClientName = user.UserName,
                    FK_WebsiteId = await GetWebsiteId()
                });
                db.SaveChanges();
            }
        }
        public async Task SetLogs(long? UsetId,long? WebsiteId, string Paramater, string response)
        {
			var user = await GetUser(UsetId??0);
			var routeData = httpContextAccessor.HttpContext.GetRouteData();
			var Action = routeData.Values["action"]?.ToString();
			var Controller = routeData.Values["controller"]?.ToString();
			db.AuditLogs.Add(new Core.Models.AuditLog
            {
                ClientIpAddress = GetClientIP(),
                BrowserInfo = httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString(),
                ExecutionTime = DateTime.Now,
                MethodName = Action,
                Parameters = Paramater,
                ServiceName = Controller,
                ReturnValue = response,
                UserId = UsetId,
                FK_WebsiteId = WebsiteId,
				ClientName = user==null?"": user.UserName
			});
            db.SaveChanges();
        }
    }
}
