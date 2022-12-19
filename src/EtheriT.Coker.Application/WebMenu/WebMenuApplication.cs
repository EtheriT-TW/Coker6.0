using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public class WebMenuApplication : IWebMenuApplication
    {

        private readonly CokerDbContext db;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;
        private readonly long WebsiteID;
        public WebMenuApplication(
            CokerDbContext db,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
            if (!long.TryParse(httpContextAccessor.HttpContext.Request.Cookies["WebSiteId"], out WebsiteID)) WebsiteID = 0;
        }
        public async Task<SiteMapDto> GetAll()
        {
            SiteMapDto response= new SiteMapDto(); ;
            try {
                response.Maps = await GetChild(0);
            }catch(Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            return response;
        }

        private async Task<List<MenuItemDto>> GetChild(long id) {
            try {
                var menus = await db.WebMenus
                            .Where(m => m.FK_TopNodeId == id)
                            .Where(m => m.FK_WebsiteId == WebsiteID)
                            .ToListAsync();
                List<MenuItemDto> result = mapper.Map<List<MenuItemDto>>(menus);
                foreach (var m in result) {
                    m.Childs = await GetChild(m.Id);
                }
                return result;
            }
            catch(Exception ex) {
                throw new Exception("資料錯誤");
            }
        }

        public async Task<ResponseMessageDto> CreateOrEdit(MenuItemDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try {
                if (dto.Id == 0) await Create(dto);
                else await Update(dto);
                response.Success = true;
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            return response;
        }
        private async Task Create(MenuItemDto dto)
        {
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            var theUser = await db.Users.FirstOrDefaultAsync(u => u.Account == name);
            WebMenu menu= mapper.Map<WebMenu>(dto);
            menu.CreatorUserId = theUser.Id;
            db.WebMenus.Add(menu);
            db.SaveChanges();
        }
        private async Task Update(MenuItemDto dto) {
            var menu = await db.WebMenus.FirstOrDefaultAsync(e => e.Id == dto.Id);
            if (menu == null) throw new Exception("查無資料");
            mapper.Map(dto, menu);
            db.SaveChanges();
        }

        public async Task<ResponseMessageDto> importConten(MenuContenDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var menu = await db.WebMenus.FirstOrDefaultAsync(e => e.Id == dto.Id);
                mapper.Map(dto, menu);
                db.SaveChanges();
                response.Success = true;
            }
            catch (Exception ex) {
                response.Success = false;
                response.Error = ex.Message;
            }
            return response;
        }

        public async Task<ResponseMessageDto> saveConten(MenuSaveContenDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var menu = await db.WebMenus.FirstOrDefaultAsync(e => e.Id == dto.Id);
                mapper.Map(dto, menu);
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
    }
}
