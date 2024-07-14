using AutoMapper;
using AutoMapper.Execution;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Permissions;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private readonly StringHandler stringHandler;
        private readonly IMapper mapper;
        private readonly string controllerName;
        public PermissionsAppService(
            CokerDbContext db,
            StringHandler stringHandler,
            LoginUserData loginUserData,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.stringHandler = stringHandler;
            this.mapper = mapper;
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
                        IsSuperUser = o.IsSuperUser,
                        Members = (
                            from u in db.Users.Where(e => !e.IsDeleted)
                            join m in db.MappingUserAndRoles.Where(e => !e.IsDeleted) on u.Id equals m.UserId
                            join web in db.MappingUserAndWebsites on u.Id equals web.UserId
                            where o.Id == m.RoleId && web.WebsiteId == websideId
                            select new PermissionsUserDto
                            {
                                Id = u.Id,
                                Name = stringHandler.privacyName(u.Name),
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
                            Name = stringHandler.privacyName(u.Name),
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
        public async Task<GetUserPermissionsRsponseDto> GetPermissions(SavePermissionsDto dto)
        {
            GetUserPermissionsRsponseDto response = new GetUserPermissionsRsponseDto();
            try
            {
                long SiteId = await loginUserData.GetWebsiteId();
                var uadateItems = await db.Permissions
                    .Where(e => e.FK_UserId == dto.FK_UserId)
                    .Where(e => e.FK_RoleId == dto.FK_RoleId)
                    .Where(e => e.FK_WebsiteId == SiteId)
                    .ToListAsync();
                response.items = mapper.Map<List<SavePermissionsItem>>(uadateItems);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "GetPermissions", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<List<SavePermissionsItem>> GetLoginUserPermissions()
        {
            long SiteId = await loginUserData.GetWebsiteId();
            long UserId = await loginUserData.GetUserId();
            var UserItems = await (from p in db.Permissions
                                   join u in db.Users on p.FK_UserId equals u.Id
                                   where p.FK_WebsiteId == SiteId && u.Id == UserId
                                   select p).ToListAsync();
            var RoleItems = await (from p in db.Permissions
                                   join r in db.Roles on p.FK_RoleId equals r.Id
                                   join m in db.MappingUserAndRoles on r.Id equals m.RoleId
                                   join u in db.Users on m.UserId equals u.Id
                                   where p.FK_WebsiteId == SiteId && u.Id == UserId
                                   select p).ToListAsync();
            List<Core.Models.Permissions> Items = new List<Core.Models.Permissions>();
            if (UserItems.Any() && !RoleItems.Any()) Items.AddRange(UserItems);
            if (!UserItems.Any() && RoleItems.Any()) Items.AddRange(RoleItems);
            return mapper.Map<List<SavePermissionsItem>>(Items);
        }
        public async Task<bool> IsPowerUserPermissions() {
            List<long> Roles = await loginUserData.GetUserRoleIds();
            var p = await db.Roles.Where(e => Roles.Contains(e.Id))
                    .Where(e => e.IsSuperUser)
                    .FirstOrDefaultAsync();
            return p != null;
		}

		public async Task<ResponseMessageDto> SavePermissions(SavePermissionsDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                long SiteId = await loginUserData.GetWebsiteId();
                var Names = dto.Items.Select(x => x.Name).ToList() ?? new List<string>();
                var uadateItems = await db.Permissions
                            .Where(e => e.FK_UserId == dto.FK_UserId)
                            .Where(e => e.FK_RoleId == dto.FK_RoleId)
                            .Where(e => e.FK_WebsiteId == SiteId)
                            .Where(e => Names.Contains(e.Name))
                            .Select(e => new SavePermissionsItem { Name = e.Name, IsGranted = e.IsGranted }).ToListAsync();
                var updateName = uadateItems.Select(x => x.Name).ToList() ?? new List<string>();
                var addItems = dto.Items.FindAll(e => !updateName.Contains(e.Name));
                if (uadateItems.Any())
                    await UpdatePermissions(new SavePermissionsDto { FK_UserId = dto.FK_UserId, FK_RoleId = dto.FK_RoleId, SiteId = SiteId, Items = uadateItems });
                if (addItems.Any())
                    await AddPermissions(new SavePermissionsDto { FK_UserId = dto.FK_UserId, FK_RoleId = dto.FK_RoleId, SiteId = SiteId, Items = addItems });
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "SavePermissions", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        private async Task AddPermissions(SavePermissionsDto dto)
        {
            long userId = await loginUserData.GetUserId();
            List<Core.Models.Permissions> permissions = new List<Core.Models.Permissions>();
            dto.Items.ForEach(e =>
            {
                permissions.Add(new Core.Models.Permissions
                {
                    Name = e.Name,
                    IsGranted = e.IsGranted,
                    FK_UserId = dto.FK_UserId,
                    FK_RoleId = dto.FK_RoleId,
                    FK_WebsiteId = dto.SiteId ?? 0,
                    CreationTime = DateTime.Now,
                    CreatorUserId = userId
                });
            });
            db.Permissions.AddRange(permissions);
            await db.SaveChangesAsync();
        }
        private async Task UpdatePermissions(SavePermissionsDto dto)
        {
            long userId = await loginUserData.GetUserId();
            List<string> Names = dto.Items.Select(e => e.Name).ToList();
            var date = db.Permissions
                        .Where(e => e.FK_UserId == dto.FK_UserId)
                        .Where(e => e.FK_RoleId == dto.FK_RoleId)
                        .Where(e => Names.Contains(e.Name))
                        .Where(e => e.FK_WebsiteId == dto.SiteId);
            db.Permissions.RemoveRange(date);
            await db.SaveChangesAsync();
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
                    db.RemoveRange(mapRole);
                    await db.SaveChangesAsync();
                }

                var permissions = db.Permissions.Where(e => e.FK_WebsiteId == siteId).Where(e => e.FK_UserId == dto.Id);
                if (permissions.Any())
                {
                    db.RemoveRange(permissions);
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
        public async Task<ResponseMessageDto> MappingUserAndWebsite(AddMapingUserAndWebsiteDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                if (dto.UsetId == null || dto.UsetId == 0)
                {
                    var user = await db.Users.Where(e => !e.IsDeleted).Where(e => e.Account == dto.emailOrAccount || e.Email == dto.emailOrAccount).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        dto.UsetId = user.Id;
                    }
                }
                if (dto.UsetId != null)
                {
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
                                RoleId = dto.RoleId,
                                UserId = dto.UsetId.Value,
                            };
                            db.MappingUserAndRoles.Add(mappingUserAndRole);
                            await loginUserData.SaveChanges(mappingUserAndRole);
                        }
                    }
                    response.Message = JsonConvert.SerializeObject(new PermissionsUserDto { Id = theUser.Id, Name = theUser.Name });
                    response.Success = true;
                }
                else throw new Exception("查無使用者");
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "MappingUserAndWebsite", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> AddUserToRole(AddUserToRoleDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var userId = await loginUserData.GetUserId();
                var all = await (
                    from m in db.MappingUserAndRoles.Where(x => !x.IsDeleted)
                    join r in db.Roles.Where(x => !x.IsDeleted).Where(e => e.FK_WebsiteId == websiteId) on m.RoleId equals r.Id
                    where r.Id == dto.RoleId && dto.Users.Contains(m.UserId)
                    select m
                ).Select(e => e.UserId).ToListAsync();
                var addUser = dto.Users.FindAll(e => !all.Contains(e));
                if (addUser != null)
                {
                    List<MappingUserAndRole> mappings = new List<MappingUserAndRole>();
                    addUser.ForEach(x =>
                    {
                        mappings.Add(new MappingUserAndRole
                        {
                            RoleId = dto.RoleId,
                            UserId = x
                        });
                    });
                    db.AddRange(mappings);
                    mappings.ForEach(e =>
                    {
                        loginUserData.setOptionParameter(e, userId);
                    });
                    db.SaveChanges();
                }
                else throw new Exception("沒有可加入的使用者");
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "AddUserToRole", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> RemoveUserToRole(AddUserToRoleDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var userId = await loginUserData.GetUserId();
                var all = await (
                    from m in db.MappingUserAndRoles.Where(x => !x.IsDeleted)
                    join r in db.Roles.Where(x => !x.IsDeleted).Where(e => e.FK_WebsiteId == websiteId) on m.RoleId equals r.Id
                    where r.Id == dto.RoleId && dto.Users.Contains(m.UserId)
                    select m
                ).ToListAsync();
                if (all != null)
                {
                    db.RemoveRange(all);
                    db.SaveChanges();
                }
                else throw new Exception("沒有可加入的使用者");
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "AddUserToRole", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> AddRole(AddRoleDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var role = await db.Roles.Where(e => e.FK_WebsiteId == websiteId).Where(e => !e.IsDeleted).Where(e => e.Name == dto.Name).FirstOrDefaultAsync();
                if (role != null) throw new Exception("該角色名稱已存在!");
                else
                {
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
        public async Task<ResponseMessageDto> EditRole(AddRoleDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var role = await db.Roles.Where(e => e.FK_WebsiteId == websiteId).Where(e => !e.IsDeleted).Where(e => e.Id == dto.Id).FirstOrDefaultAsync();
                var powerRole = await db.Roles.Where(e => e.FK_WebsiteId == websiteId).Where(e => !e.IsDeleted).Where(e => e.IsSuperUser).FirstOrDefaultAsync();
                if (role == null) throw new Exception("該角色不存在!");
                else if(powerRole != null && powerRole.Id != dto.Id) throw new Exception("總管理者角色僅能唯一!");
                else
                {
                    role.Name = dto.Name;
                    role.IsSuperUser = dto.IsSuperUser;
                    await loginUserData.SaveChanges(role);
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "EditRole", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }

        public async Task<ResponseMessageDto> DeleteRole(DataDelectDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var role = await db.Roles.Where(e => e.FK_WebsiteId == websiteId).Where(e => !e.IsDeleted).Where(e => e.Id == dto.Id).FirstOrDefaultAsync();
                if (role == null) throw new Exception("該角色不存在!");
                else
                {
                    role.IsDeleted = true;
                    await loginUserData.SaveChanges(role);
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(controllerName, "DeleteRole", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> GetPagePermission(GetPagePermissionInputDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var items = await db.PermissionDetail.Where(e => e.FK_WebsiteId == websiteId).Where(e => e.Type == (int)dto.Type && e.FK_TargetId == dto.PageId).ToListAsync();
                var userPerm = items.Where(e => e.FK_UserId != null && e.IsGranted).Select(e => e.FK_UserId).ToList();
                var RoPerm = items.Where(e => e.FK_RoleId != null && e.IsGranted).Select(e => e.FK_RoleId).ToList();
                response.Object = new PagePermissionOutputDto
                {
                    Users = await (from m in db.MappingUserAndWebsites
                                   join u in db.Users on m.UserId equals u.Id
                                   where m.WebsiteId == websiteId
                                   select new PermissionsUserCheckDto
                                   {
                                       Id = u.Id,
                                       Name = u.Name,
                                       IsChecked = userPerm.Contains(u.Id)
                                   }).ToListAsync(),
                    Roles = await (from r in db.Roles
                                   where r.FK_WebsiteId == websiteId && !r.IsSuperUser
                                   select new PermissionsRoleCheckDto
                                   {
                                       Id = r.Id,
                                       Name = r.Name,
                                       IsChecked = RoPerm.Contains(r.Id)
                                   }
                                   ).ToListAsync(),
                };
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> SavePagePermission(SavePagePermissionInputDto dto) {
            ResponseMessageDto response = new ResponseMessageDto();
            try {
                var websiteId = await loginUserData.GetWebsiteId();
                var userId = await loginUserData.GetUserId();
                List<PermissionDetail> pagePermissions = new List<PermissionDetail>();
                List<PermissionDetail> EditPagePermissions = new List<PermissionDetail>();
                var n = await db.PermissionDetail
                    .Where(e => e.FK_WebsiteId == websiteId)
                    .Where(e => e.FK_TargetId == dto.PageId)
                    .Where(e => e.Type == (int)dto.Type).ToListAsync();
                if (n != null) {
                    n.ForEach(e => {
                        if (e.FK_UserId != null && dto.Users.Contains(e.FK_UserId.Value)) e.IsGranted = true;
                        else if (e.FK_RoleId != null && dto.Roles.Contains(e.FK_RoleId.Value)) e.IsGranted = true;
                        else e.IsGranted = false;
                        EditPagePermissions.Add(e);
                    });
                    await db.SaveChangesAsync();
                }

                dto.Users.ForEach(id => {
                    string key = string.Empty;
                    switch (dto.Type)
                    {
                        case PermissionDetailsTypeEnum.選單:
                            key = "Menu";
                            break;
                        case PermissionDetailsTypeEnum.目錄:
                            key = "Directory";
                            break;
                        case PermissionDetailsTypeEnum.產品:
                            key = "Product";
                            break;
                        case PermissionDetailsTypeEnum.文章:
                            key = "Article";
                            break;
                    }
                    if (!EditPagePermissions.Exists(item => item.FK_UserId == id)) {
                        pagePermissions.Add(new PermissionDetail
                        {
                            FK_UserId = id,
                            FK_WebsiteId = websiteId,
                            FK_TargetId= dto.PageId,
                            CreationTime = DateTime.Now,
                            CreatorUserId = userId,
                            Type = (int)dto.Type,
                            Name = $"{key}.Edit",
                            IsGranted = true
                        });
                    }
                });

                dto.Roles.ForEach(id => {
                    string key = string.Empty;
                    switch (dto.Type)
                    {
                        case PermissionDetailsTypeEnum.選單:
                            key = "Menu";
                            break;
                        case PermissionDetailsTypeEnum.目錄:
                            key = "Directory";
                            break;
                        case PermissionDetailsTypeEnum.產品:
                            key = "Product";
                            break;
                        case PermissionDetailsTypeEnum.文章:
                            key = "Article";
                            break;
                    }
                    if (!EditPagePermissions.Exists(item => item.FK_RoleId == id))
                    {
                        pagePermissions.Add(new PermissionDetail
                        {
                            FK_RoleId = id,
                            FK_WebsiteId = websiteId,
                            FK_TargetId = dto.PageId,
                            CreationTime = DateTime.Now,
                            CreatorUserId = userId,
                            Type = (int)dto.Type,
                            Name = $"{key}.Edit",
                            IsGranted = true
                        });
                    }
                });
                db.PermissionDetail.AddRange(pagePermissions);
                await db.SaveChangesAsync();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }finally {
                await loginUserData.SetLogs(controllerName, "SavePagePermission",JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }
    }
}
