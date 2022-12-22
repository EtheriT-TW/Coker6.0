using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        private readonly string ApplicationName;
        private readonly CokerDbContext db;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        public WebMenuApplication(
            CokerDbContext db,
            IHttpContextAccessor httpContextAccessor,
            LoginUserData loginUserData,
            IMapper mapper)
        {
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.ApplicationName = "WebMenu";
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
            await loginUserData.SetLogs(ApplicationName, "GetAll", "", JsonConvert.SerializeObject(response));
            return response;
        }

        private async Task<List<MenuItemDto>> GetChild(long? id) {
            try {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var menus = await db.WebMenus
                            .Where(m => m.FK_TopNodeId == id)
                            .Where(m => m.FK_WebsiteId == WebsiteID)
                            .Where(m => !m.IsDeleted)
                            .OrderBy(m => m.SerNO)
                            .ThenBy(m => m.Id)
                            .ToListAsync();
                List<MenuItemDto> result = mapper.Map<List<MenuItemDto>>(menus);
                foreach (var m in result) {
                    m.Children = await GetChild(m.Id);
                    if (m.Children.Count == 0) m.Children = null;
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
                if (dto.Id == 0)
                {
                    long newId = await Create(dto);
                    response.Message = newId.ToString();
                }
                else await Update(dto);
                response.Success = true;
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(ApplicationName, "CreateOrEdit",JsonConvert.SerializeObject(dto),JsonConvert.SerializeObject(response));
            return response;
        }
        private async Task<long> Create(MenuItemDto dto)
        {
            long WebsiteID = await loginUserData.GetWebsiteId();
            var user = await loginUserData.GetUser();
            WebMenu menu= mapper.Map<WebMenu>(dto);
            menu.CreatorUserId = user.Id;
            menu.FK_WebsiteId = WebsiteID;
            db.WebMenus.Add(menu);
            await loginUserData.SaveChanges(menu);
            return menu.Id;
        }
        private async Task Update(MenuItemDto dto) {
            var menu = await db.WebMenus.FirstOrDefaultAsync(e => e.Id == dto.Id);
            var user = await loginUserData.GetUser();
            if (menu == null) throw new Exception("查無資料");
            mapper.Map(dto, menu);
            menu.LastModificationTime = DateTime.Now;
            menu.LastModifierUserId = user.Id;
            await loginUserData.SaveChanges(menu);
        }
        public async Task<ResponseMessageDto> importConten(MenuContenDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var user = await loginUserData.GetUser();
                var menu = await db.WebMenus.FirstOrDefaultAsync(e => e.Id == dto.Id);
                mapper.Map(dto, menu);
                menu.LastModificationTime = DateTime.Now;
                menu.LastModifierUserId = user.Id;
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
                var user = await loginUserData.GetUser();
                var menu = await db.WebMenus.FirstOrDefaultAsync(e => e.Id == dto.Id);
                mapper.Map(dto, menu);
                db.SaveChanges();
                menu.LastModificationTime = DateTime.Now;
                menu.LastModifierUserId = user.Id;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> Delete(DataDelectDto dto) {
            ResponseMessageDto response = new ResponseMessageDto { Success = true };
            try {
                var user = await loginUserData.GetUser();
                long siteID = await loginUserData.GetWebsiteId();
                var item = await db.WebMenus
                        .Where(e => e.Id == dto.Id)
                        .Where(e => e.FK_WebsiteId == siteID)
                        .FirstOrDefaultAsync();
                if (item == null) throw new Exception("資料不存在");
                else { 
                    item.IsDeleted = true;
                    await loginUserData.SaveChanges(item);
                }
            }catch(Exception ex)
            {
                response.Success = false;
            }
            return response;
        }
        public async Task<ResponseMessageDto> updateSerNo(UpdateSerNoListDto dto) {
            ResponseMessageDto response = new ResponseMessageDto { Success = true };
            try {
                var o = (from s in dto.list select s.Id).ToList();
                var result = db.WebMenus.Where(e => o.Contains(e.Id));
                await result.ForEachAsync(e => {
                    var item = dto.list.Find( o => o.Id == e.Id);
                    mapper.Map(item, e);
                });
                await loginUserData.SaveChanges(result);
            }
            catch (Exception ex) {
                response.Success = false;
                response.Error = ex.ToString();
            }
            
            return response;
        }
    }
}
