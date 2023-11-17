using AutoMapper.Execution;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Permissions;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Permissions
{
    public class PermissionsAppService : IPermissionsAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly string controllerName;
        public PermissionsAppService(
            CokerDbContext db,
            LoginUserData loginUserData
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            controllerName = "Permissions";
        }
        public async Task<GetPermissionsOutputDto> GetPermissionsUserData()
        {
            long websideId = await loginUserData.GetWebsiteId();
            GetPermissionsOutputDto output = new GetPermissionsOutputDto();
            try
            {
                var result = await (
                    from o in db.Roles.Where(e => !e.IsDeleted)
                    where o.FK_WebsiteId == websideId && o.Type == (int)RoleTypeEnum.後台
                    select new PermissionsRoleDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Members = (
                            from u in db.Users.Where(e => !e.IsDeleted)
                            join m in db.MappingUserAndRoles.Where(e => !e.IsDeleted) on u.Id equals m.UserId
                            join web in db.MappingUserAndWebsites on u.Id equals web.UserId
                            where o.Id == m.RoleId && web.Id == websideId
                            select new PermissionsUserDto
                            {
                                Id = u.Id,
                                Name = u.Name,
                            }
                        ).ToList()
                    }).ToListAsync();
                var Members = result.Select(e => e.Members).ToList();
                List<long> Ids = new List<long>();
                Members.ForEach(e =>
                {
                    Ids.AddRange(e.Select(o => o.Id).ToList());
                });

                var noneRole = new PermissionsRoleDto
                {
                    Id = 0,
                    Name = "無群組",
                    Members = await (
                        from u in db.Users.Where(e => !e.IsDeleted)
                        join web in db.MappingUserAndWebsites.Where(e => !e.IsDeleted) on u.Id equals web.UserId
                        where web.WebsiteId == websideId && !Ids.Contains(u.Id)
                        select new PermissionsUserDto
                        {
                            Id = u.Id,
                            Name = u.Name,
                        }
                    ).ToListAsync()
                };
                output.Data.Add(noneRole);
                output.Data.AddRange(result);
                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "GetPermissionsUserData", "", JsonConvert.SerializeObject(output));
            return output;
        }
        public async Task<ResponseMessageDto> RemoveMappingUserAndWebsite(DataDelectDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var siteId = await loginUserData.GetWebsiteId();
                var mapWeb = await db.MappingUserAndWebsites.Where(e => !e.IsDeleted).Where(e => e.WebsiteId == siteId).Where(e => e.UserId == dto.Id).FirstOrDefaultAsync();
                if (mapWeb != null)
                {
                    db.Remove(mapWeb);
                    await db.SaveChangesAsync();
                }
                var mapRole = db.MappingUserAndRoles.Include(e => e.Role)
                    .Where(e => e.UserId == dto.Id)
                    .Where(e => e.Role != null && e.Role.FK_WebsiteId == siteId);
                if (mapRole.Any())
                {
                    db.Remove(mapRole);
                    await db.SaveChangesAsync();
                }

                var permissions = db.Permissions.Where(e => e.FK_WebsiteId == siteId).Where(e => e.FK_UserId == dto.Id);
                if (permissions.Any())
                {
                    db.Remove(permissions);
                    await db.SaveChangesAsync();
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "RemoveMappingUserAndWebsite", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> MappingUserAndWebsite(AddMapingUserAndWebsiteDto dto) {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                if (dto.UsetId == null || dto.UsetId == 0) {
                    var user = await db.Users.Where(e => !e.IsDeleted).Where(e => e.Account == dto.emailOrAccount || e.Email == dto.emailOrAccount).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        dto.UsetId = user.Id;
                    }
                }
                if (dto.UsetId != null) {
                    var theUser = await db.Users.Where(e => !e.IsDeleted).Where(e => e.Id == dto.UsetId).FirstOrDefaultAsync();
                    var mapWebsite = await db.MappingUserAndWebsites.Where(e => e.UserId == dto.UsetId).Where(e => e.WebsiteId == websiteId).FirstOrDefaultAsync();
                    if (theUser == null) throw new Exception("查無使用者");
                    if (mapWebsite == null)
                    {
                        MappingUserAndWebsite map = new MappingUserAndWebsite
                        {
                            WebsiteId = websiteId,
                            UserId = dto.UsetId.Value,
                        };
                        db.MappingUserAndWebsites.Add(map);
                        await loginUserData.SaveChanges(map);
                    }
                    else throw new Exception("該使用者已有授權管理該網站");
                    if (dto.RoleId != 0)
                    {
                        var mapRole = db.MappingUserAndRoles.Where(e => e.UserId == dto.UsetId).Where(e => e.RoleId == dto.RoleId);
                        if (!mapRole.Any())
                        {
                            MappingUserAndRole mappingUserAndRole = new MappingUserAndRole
                            {
                                Id = dto.RoleId,
                                UserId = dto.UsetId.Value,
                            };
                            db.MappingUserAndRoles.Add(mappingUserAndRole);
                            await loginUserData.SaveChanges(mappingUserAndRole);
                        }
                    }
                    response.Message = JsonConvert.SerializeObject(new PermissionsUserDto { Id=theUser.Id,Name = theUser.Name });
                    response.Success = true;
                }
                else throw new Exception("查無使用者");
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> AddRole(AddRoleDto dto) {
            ResponseMessageDto response = new ResponseMessageDto();
            try {
                var websiteId = await loginUserData.GetWebsiteId();
                var role = await db.Roles.Where(e => e.FK_WebsiteId == websiteId).Where(e => !e.IsDeleted).Where(e => e.Name == dto.Name).FirstOrDefaultAsync();
                if (role != null) throw new Exception("該角色名稱已存在!");
                else {
                    Role myRole = new Role
                    {
                        Name = dto.Name,
                        Type = 2,
                        FK_WebsiteId = websiteId,
                    };
                    db.Roles.Add(myRole);
                    await loginUserData.SaveChanges(myRole);
                    response.Message = JsonConvert.SerializeObject(new PermissionsUserDto { Id = myRole.Id, Name = myRole.Name });
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "AddRole", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
    }
}
