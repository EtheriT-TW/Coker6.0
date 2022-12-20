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
        private readonly ILoginUserDataApplication loginUserDataApplication;
        private readonly IMapper mapper;
        public WebMenuApplication(
            CokerDbContext db,
            IHttpContextAccessor httpContextAccessor,
            ILoginUserDataApplication loginUserDataApplication,
            IMapper mapper)
        {
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.loginUserDataApplication = loginUserDataApplication;
            this.mapper = mapper;
        }
        public async Task<SiteMapDto> GetAll()
        {
            SiteMapDto response = new SiteMapDto { Success = false };
            try {
                response.Maps = await GetChild(null);
                response.Success = true;
            }
            catch(Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }

        private async Task<List<MenuItemDto>> GetChild(long? id) {
            try {
                long WebsiteID = await loginUserDataApplication.GetWebsiteId();
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
            long WebsiteID = await loginUserDataApplication.GetWebsiteId();
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            var theUser = await db.Users.FirstOrDefaultAsync(u => u.Account == name);
            WebMenu menu= mapper.Map<WebMenu>(dto);
            menu.CreatorUserId = theUser.Id;
            menu.FK_WebsiteId = WebsiteID;
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
