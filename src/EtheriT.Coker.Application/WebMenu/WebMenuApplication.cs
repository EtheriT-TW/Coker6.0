using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Permissions;
using EtheriT.Coker.Application.Processor;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Dto.enumType.WebMenu;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.JsonObject;
using EtheriT.Coker.Application.Shared.Dto.Processor;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;

namespace EtheriT.Coker.Application
{
    public class WebMenuApplication : IWebMenuApplication
    {
        private readonly string ApplicationName;
        private readonly CokerDbContext db;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LoginUserData loginUserData;
        private readonly StringHandler stringHandler;
        private readonly IMapper mapper;
        private readonly IConfiguration Configuration;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly IJsonObjectAppService jsonObjectAppService;
        private readonly IPermissionsAppService permissionsAppService;
        private readonly IWebsiteCacheStateAppService websiteCacheStateAppService;
        private readonly IHtmlProcessor htmlProcessor;
        private readonly IHtmlSanitizeService htmlSanitizeService;
        public WebMenuApplication(
            CokerDbContext db,
            IHttpContextAccessor httpContextAccessor,
            LoginUserData loginUserData,
            IMapper mapper,
            IConfiguration Configuration,
            IFileUploadAppService fileUploadAppService,
            IJsonObjectAppService jsonObjectAppService,
            IPermissionsAppService permissionsAppService,
            IWebsiteCacheStateAppService websiteCacheStateAppService,
            IHtmlProcessor htmlProcessor,
            StringHandler stringHandler,
            IHtmlSanitizeService htmlSanitizeService
        )
        {
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.Configuration = Configuration;
            this.ApplicationName = "WebMenu";
            this.fileUploadAppService = fileUploadAppService;
            this.jsonObjectAppService = jsonObjectAppService;
            this.permissionsAppService = permissionsAppService;
            this.websiteCacheStateAppService = websiteCacheStateAppService;
            this.htmlProcessor = htmlProcessor;
            this.stringHandler = stringHandler;
            this.htmlSanitizeService = htmlSanitizeService;

        }
        public async Task<MenuEditorTreeDto> GetAll()
        {
            MenuEditorTreeDto response = new MenuEditorTreeDto { Success = false };
            try
            {
                response.Maps = await GetEditorMenuTreeAsync();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<SiteMapDto> GetDisplayAll(long WebsiteID)
        {
            SiteMapDto response = new SiteMapDto { Success = false };
            try
            {
                var cacheResult = await GetOrRefreshDisplayMenuCacheAsync(WebsiteID, true);

                response.Message = cacheResult.Json;
                response.Maps = JsonConvert.DeserializeObject<List<MenuItemDto>>(cacheResult.Json) ?? new List<MenuItemDto>();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<SiteMapDto> GetSiteMap()
        {
            SiteMapDto response = new SiteMapDto { Success = false };
            try
            {
                var siteId = loginUserData.GetFrontWebsiteId();
                var child = loginUserData.GetFrontChildOrgName();
                response = await GetDisplayAll(siteId);
                response.Maps = FilterMainMenuItems(response.Maps);
                if (child != null && child.Any())
                {
                    child.ForEach(item =>
                    {
                        item = item.ToLower().Trim();
                    });
                    foreach (var e in response.Maps)
                    {
                        var link = (e.LinkUrl ?? "").Replace("/", "").ToLower().Trim();
                        if (child.Contains(link))
                        {
                            var website = await db.Websites.Where(e => e.OrgName.ToLower() == link).FirstOrDefaultAsync();
                            if (website != null)
                            {
                                var map = await GetDisplayAll(website.Id);
                                e.Children = map.Success ? FilterMainMenuItems(map.Maps) : null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }

        private static List<MenuItemDto> FilterMainMenuItems(IEnumerable<MenuItemDto>? items)
        {
            if (items == null) return new List<MenuItemDto>();

            return items
                .Where(item => item.Visible
                    && item.IsFromShelves
                    && item.ShowToMenu
                    && (item.PageType == PageTypeEnum.一般頁面
                        || item.PageType == PageTypeEnum.結構頁面))
                .Select(item =>
                {
                    var children = FilterMainMenuItems(item.Children);
                    item.Children = children.Count > 0 ? children : null;
                    return item;
                })
                .ToList();
        }

        public async Task CheckDisplayAll(long WebsiteID)
        {
            try
            {
                await GetOrRefreshDisplayMenuCacheAsync(WebsiteID, true);
            }
            catch (Exception e)
            {

            }
        }

        private async Task<(string Json, long Version)> GetOrRefreshDisplayMenuCacheAsync(long websiteId, bool saveWhenRebuild = true)
        {
            var currentVersion = await websiteCacheStateAppService.EnsureVersionByWebsiteIdAsync(websiteId, WebsiteCacheKeys.Menu, 1);

            var header = await db.JsonObjects
                .Where(e => e.FK_WebsiteId == websiteId && e.CacheKey == WebsiteCacheKeys.Menu)
                .FirstOrDefaultAsync();

            bool cacheHit = header != null
                && !string.IsNullOrWhiteSpace(header.Json)
                && header.Version == currentVersion;

            if (cacheHit)
            {
                return (header!.Json, currentVersion);
            }

            var jsonStr = await GetDisplayChildAndSaveCache(null, websiteId);

            if (saveWhenRebuild)
            {
                await jsonObjectAppService.AddUp(new JsonObjectAddDto
                {
                    FK_WebsiteId = websiteId,
                    CacheKey = WebsiteCacheKeys.Menu,
                    CacheVersion = currentVersion,
                    Json = jsonStr
                });
            }

            return (jsonStr, currentVersion);
        }
        private async Task<List<MenuEditorTreeItemDto>> GetEditorMenuTreeAsync()
        {
            var websiteId = await loginUserData.GetWebsiteId();
            var userId = await loginUserData.GetUserId();
            var roleIds = await loginUserData.GetUserRoleIds();
            var isSuperUser = await permissionsAppService.IsPowerUserPermissions();

            HashSet<long>? allowedMenuIds = null;
            if (!isSuperUser)
            {
                var permittedIds = await db.PermissionDetail
                    .AsNoTracking()
                    .Where(permission => permission.FK_WebsiteId == websiteId)
                    .Where(permission => permission.FK_UserId == userId
                        || (permission.FK_RoleId != null && roleIds.Contains(permission.FK_RoleId.Value)))
                    .Where(permission => permission.Type == (int)PermissionDetailsTypeEnum.選單)
                    .Where(permission => permission.IsGranted && permission.FK_TargetId != null)
                    .Select(permission => permission.FK_TargetId!.Value)
                    .Distinct()
                    .ToListAsync();

                // 沿用既有規則：完全沒有設定權限時顯示全部；有設定時才套用允許清單。
                if (permittedIds.Count > 0)
                    allowedMenuIds = permittedIds.ToHashSet();
            }

            var menuQuery = db.WebMenus
                .AsNoTracking()
                .Where(menu => !menu.IsDeleted && menu.FK_WebsiteId == websiteId);

            if (allowedMenuIds != null)
                menuQuery = menuQuery.Where(menu => allowedMenuIds.Contains(menu.Id));

            // 僅投影編輯器需要的欄位，避免把每一頁的大型 HTML、CSS 與 PageText 載入記憶體。
            var menuRows = await menuQuery
                .OrderBy(menu => menu.SerNO)
                .ThenBy(menu => menu.Id)
                .Select(menu => new
                {
                    Item = new MenuEditorTreeItemDto
                    {
                        Id = menu.Id,
                        Title = menu.Title,
                        icon = menu.icon,
                        Visible = menu.Visible,
                        SerNO = menu.SerNO,
                        FK_TopNodeId = menu.FK_TopNodeId,
                        FK_RootNodeId = menu.FK_RootNodeId
                    },
                    menu.PageType,
                    HasPageText = menu.PageText != null && menu.PageText != "",
                    HasMediaHtml = menu.Html != null
                        && (menu.Html.Contains("<img") || menu.Html.Contains("<iframe") || menu.Html.Contains("<video"))
                })
                .ToListAsync();

            var menus = menuRows.Select(row =>
            {
                row.Item.icon = NormalizeMenuIcon(row.Item.icon);
                row.Item.hasContan = row.PageType != PageTypeEnum.結構頁面
                    && (row.HasPageText || row.HasMediaHtml);
                return row.Item;
            }).ToList();

            if (menus.Count == 0) return menus;

            var menuIds = menus.Select(menu => menu.Id).ToList();
            var permissionRows = await db.PermissionDetail
                .AsNoTracking()
                .Where(permission => permission.FK_WebsiteId == websiteId
                    && permission.FK_TargetId != null
                    && menuIds.Contains(permission.FK_TargetId.Value)
                    && permission.IsGranted
                    && (permission.Type == (int)PermissionDetailsTypeEnum.選單
                        || permission.Type == (int)PermissionDetailsTypeEnum.選單會員))
                .Select(permission => new { TargetId = permission.FK_TargetId!.Value, permission.Type })
                .Distinct()
                .ToListAsync();

            var backstagePermissionIds = permissionRows
                .Where(permission => permission.Type == (int)PermissionDetailsTypeEnum.選單)
                .Select(permission => permission.TargetId)
                .ToHashSet();
            var frontPermissionIds = permissionRows
                .Where(permission => permission.Type == (int)PermissionDetailsTypeEnum.選單會員)
                .Select(permission => permission.TargetId)
                .ToHashSet();

            foreach (var menu in menus)
            {
                menu.HasBackstagePermission = backstagePermissionIds.Contains(menu.Id);
                menu.HasFrontPermission = frontPermissionIds.Contains(menu.Id);
                menu.Children = new List<MenuEditorTreeItemDto>();
            }

            var menuMap = menus.ToDictionary(menu => menu.Id);
            foreach (var menu in menus)
            {
                if (menu.FK_TopNodeId != null && menuMap.TryGetValue(menu.FK_TopNodeId.Value, out var parent))
                    parent.Children!.Add(menu);
            }

            foreach (var menu in menus.Where(menu => menu.Children!.Count == 0))
                menu.Children = null;

            return menus.Where(menu => menu.FK_TopNodeId == null).ToList();
        }

        public async Task<MenuEditorDetailDto> GetEditorDetail(long id)
        {
            var response = new MenuEditorDetailDto();

            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var menu = await db.WebMenus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.Id == id
                        && item.FK_WebsiteId == websiteId
                        && !item.IsDeleted);

                if (menu == null)
                    throw new Exception("查無選單資料");

                if (!await CanEditMenuAsync(websiteId, id))
                    throw new Exception("您沒有此選單的編輯權限");

                var item = mapper.Map<MenuItemDto>(menu);
                item.icon = NormalizeMenuIcon(item.icon);
                item.RouterName ??= string.Empty;
                item.LinkUrl ??= string.Empty;
                var menuIds = new List<long> { id };

                var imageFiles = await fileUploadAppService.getImgsFiles(new FileGetImgsInputDto
                {
                    Sid = menuIds,
                    Type = (int)FileBindTypeEnum.選單圖,
                    Size = 1
                });
                var image = imageFiles.FirstOrDefault();
                if (image != null)
                {
                    item.ImgId = image.Id;
                    item.ImgUrl = image.Link;
                    item.ImgName = image.Name;
                }

                var overImageFiles = await fileUploadAppService.getImgsFiles(new FileGetImgsInputDto
                {
                    Sid = menuIds,
                    Type = (int)FileBindTypeEnum.選單覆蓋,
                    Size = 1
                });
                var overImage = overImageFiles.FirstOrDefault();
                if (overImage != null)
                {
                    item.OverImgId = overImage.Id;
                    item.OverImgUrl = overImage.Link;
                    item.OverImgName = overImage.Name;
                }

                if ((item.icon ?? "").StartsWith("IconId", StringComparison.OrdinalIgnoreCase))
                {
                    var iconParts = item.icon!.Split(':', 2);
                    if (iconParts.Length == 2
                        && long.TryParse(iconParts[1], out var iconId)
                        && iconId > 0)
                    {
                        var iconMap = await fileUploadAppService.GetImgFileMapByIdAsync(
                            new List<long> { iconId },
                            1);
                        item.IconId = iconId.ToString();
                        if (iconMap.TryGetValue(iconId, out var iconUrl))
                            item.IconUrl = iconUrl;
                    }
                }

                var permissionTypes = await db.PermissionDetail
                    .AsNoTracking()
                    .Where(permission => permission.FK_WebsiteId == websiteId
                        && permission.FK_TargetId == id
                        && permission.IsGranted
                        && (permission.Type == (int)PermissionDetailsTypeEnum.選單
                            || permission.Type == (int)PermissionDetailsTypeEnum.選單會員))
                    .Select(permission => permission.Type)
                    .Distinct()
                    .ToListAsync();

                item.HasBackstagePermission =
                    permissionTypes.Contains((int)PermissionDetailsTypeEnum.選單);
                item.HasFrontPermission =
                    permissionTypes.Contains((int)PermissionDetailsTypeEnum.選單會員);

                response.Item = item;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }

            return response;
        }

        private async Task<bool> CanEditMenuAsync(long websiteId, long menuId)
        {
            if (await permissionsAppService.IsPowerUserPermissions())
                return true;

            var userId = await loginUserData.GetUserId();
            var roleIds = await loginUserData.GetUserRoleIds();
            var permittedIds = await db.PermissionDetail
                .AsNoTracking()
                .Where(permission => permission.FK_WebsiteId == websiteId)
                .Where(permission => permission.FK_UserId == userId
                    || (permission.FK_RoleId != null && roleIds.Contains(permission.FK_RoleId.Value)))
                .Where(permission => permission.Type == (int)PermissionDetailsTypeEnum.選單)
                .Where(permission => permission.IsGranted && permission.FK_TargetId != null)
                .Select(permission => permission.FK_TargetId!.Value)
                .Distinct()
                .ToListAsync();

            return permittedIds.Count == 0 || permittedIds.Contains(menuId);
        }

        private async Task<string> GetDisplayChildAndSaveCache(long? id, long WebsiteID)
        {
            var menus = await GetDisplayChild(id, WebsiteID, false, true);
            string jsonStr = JsonConvert.SerializeObject(menus);
            return jsonStr;
        }
        private async Task<List<MenuItemDto>> GetDisplayChild(long? id, long WebsiteID, bool getDirectoryMenuData = false, bool ShowToMenu = false)
        {
            try
            {
                IQueryable<WebMenu>? dataQuery = db.WebMenus.Include(e => e.Website).Where(m => m.FK_TopNodeId == id)
                            .Where(m => m.FK_WebsiteId == WebsiteID)
                            .Where(m => !m.IsDeleted)
                            .Where(m => !m.RemovedFromShelves);
                if (!getDirectoryMenuData) dataQuery = dataQuery.Where(e => e.Visible);
                if (ShowToMenu)
                {
                    dataQuery = dataQuery.Where(e => e.ShowToMenu).Where(e => e.PageType == PageTypeEnum.一般頁面 || e.PageType == PageTypeEnum.結構頁面);
                }
                var menus = await dataQuery
                            .OrderBy(m => m.SerNO)
                            .ThenBy(m => m.Id)
                            .ToListAsync();
                List<MenuItemDto> result = mapper.Map<List<MenuItemDto>>(menus);
                foreach (var m in result)
                {
                    m.ImgUrl = await fileUploadAppService.getImgUrl(m.ImgId, WebsiteID);
                    if (m.OverImgId != null) m.OverImgUrl = await fileUploadAppService.getImgUrl(m.OverImgId, WebsiteID);
                    if (m.icon.StartsWith("IconId"))
                    {
                        if (m.icon.Split(":")[1] != "")
                        {
                            var iconimage = await fileUploadAppService.getImgUrl(long.Parse(m.icon.Split(":")[1]), (long)WebsiteID);
                            m.IconImage = iconimage;
                        }
                        else m.icon = "empty";
                    }
                    m.Children = await GetDisplayChild(m.Id, WebsiteID, getDirectoryMenuData, ShowToMenu);
                    if (m.Children.Count == 0) m.Children = null;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("資料錯誤");
            }
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions, long? mid = null)
        {
            var websiteId = await loginUserData.GetWebsiteId();
            var dataQuery = db.WebMenus
                .AsNoTracking()
                .Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId)
                .OrderByDescending(e => mid.HasValue && e.Id == mid.Value)
                .ThenBy(e => e.Id)
                .Select(e => new MenuGetAllListDto
                {
                    IsSelected = mid.HasValue && e.Id == mid.Value,
                    Id = e.Id,
                    Title = e.Title ?? string.Empty,
                    Link = e.RouterName ?? string.Empty,
                    Items = string.Empty
                });

            // 先讓 DevExtreme 在 SQL 端完成篩選、排序、計數與分頁，避免載入大型 HTML/CSS 欄位。
            var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
            var pageRows = ((IEnumerable<object>)output.data).Cast<MenuGetAllListDto>().ToList();
            var pageIds = pageRows.Select(e => e.Id).ToList();

            if (pageIds.Count > 0)
            {
                // 僅查詢當頁選單的直接子選單；一次 SQL 取回，取代逐筆遞迴 GetChild 的 N+1 查詢。
                var childRows = await db.WebMenus
                    .AsNoTracking()
                    .Where(e => !e.IsDeleted
                        && e.FK_WebsiteId == websiteId
                        && e.FK_TopNodeId != null
                        && pageIds.Contains(e.FK_TopNodeId.Value))
                    .OrderBy(e => e.SerNO)
                    .ThenBy(e => e.Id)
                    .Select(e => new
                    {
                        ParentId = e.FK_TopNodeId!.Value,
                        Title = e.Title ?? string.Empty
                    })
                    .ToListAsync();

                var childMap = childRows
                    .GroupBy(e => e.ParentId)
                    .ToDictionary(
                        group => group.Key,
                        group =>
                        {
                            var children = group.ToList();
                            var summary = string.Join("、", children.Take(3).Select(e => e.Title));
                            return children.Count > 3 ? $"{summary}..." : summary;
                        });

                foreach (var row in pageRows)
                    row.Items = childMap.TryGetValue(row.Id, out var items) ? items : string.Empty;
            }

            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<MenuGetAllListDto> GetSelectData(long Mid)
        {
            try
            {
                var WebstieId = await loginUserData.GetWebsiteId();
                var results = await db.WebMenus.Where(e => e.Id == Mid && !e.IsDeleted && e.FK_WebsiteId == WebstieId).FirstOrDefaultAsync();
                if (results != null)
                {
                    MenuGetAllListDto output = mapper.Map(results, new MenuGetAllListDto());
                    var childTitles = await db.WebMenus
                        .AsNoTracking()
                        .Where(menu => !menu.IsDeleted
                            && menu.FK_WebsiteId == WebstieId
                            && menu.FK_TopNodeId == output.Id)
                        .OrderBy(menu => menu.SerNO)
                        .ThenBy(menu => menu.Id)
                        .Select(menu => menu.Title ?? string.Empty)
                        .Take(4)
                        .ToListAsync();
                    if (childTitles.Count > 0)
                    {
                        output.Items = string.Join("、", childTitles.Take(3));
                        if (childTitles.Count > 3) output.Items += "...";
                    }
                    return output;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<MenuItemDto> GetDisplayOne(DataIdWebsiteIdDto dto)
        {
            try
            {
                var output = await (from w in db.WebMenus
                                    where w.Id == dto.Id
                                    where !w.IsDeleted && w.FK_WebsiteId == dto.WebsiteId && !w.RemovedFromShelves
                                    select new MenuItemDto
                                    {
                                        Id = w.Id,
                                        Title = w.Title,
                                        RouterName = w.RouterName,
                                        Children = new List<MenuItemDto>()
                                    }).FirstOrDefaultAsync();
                if (output != null)
                {
                    var children = await GetDisplayChild(dto.Id, dto.WebsiteId, dto.showUnvisible);

                    if (children.Count > 0)
                    {
                        var sids = children.Select(c => c.Id).ToList();
                        var imgDtos = await fileUploadAppService.getImgsFiles(new FileGetImgsInputDto
                        {
                            Sid = sids,
                            Size = 1,
                            Type = (int)FileBindTypeEnum.選單圖
                        });

                        var overDtos = await fileUploadAppService.getImgsFiles(new FileGetImgsInputDto
                        {
                            Sid = sids,
                            Size = 1,
                            Type = (int)FileBindTypeEnum.選單覆蓋
                        });

                        var imgBySid = imgDtos?.GroupBy(x => x.Sid).ToDictionary(g => g.Key, g => g.First())
                                         ?? new Dictionary<long, FileGetImgDto>();
                        var overBySid = overDtos?.GroupBy(x => x.Sid).ToDictionary(g => g.Key, g => g.First())
                                         ?? new Dictionary<long, FileGetImgDto>();

                        for (int i = 0; i < children.Count; i++)
                        {
                            var e = children[i];

                            if (imgBySid.TryGetValue(e.Id, out var img))
                            {
                                e.ImgId = img.Id;
                                e.ImgUrl = img.Link;
                            }

                            if (overBySid.TryGetValue(e.Id, out var over))
                            {
                                e.OverImgId = over.Id;
                                e.OverImgUrl = over.Link;
                            }
                        }
                    }

                    output.Children = children;
                }
                return output;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<GetMenuBreadDto>> GetMenuBread(long Id)
        {
            var output = new List<GetMenuBreadDto>();

            var result = await db.WebMenus.Where(e => e.Id == Id && !e.IsDeleted).FirstOrDefaultAsync();
            if (result != null && result.PageType != PageTypeEnum.首頁)
            {
                var site = await db.Websites.Where(e => e.Id == result.FK_WebsiteId).FirstOrDefaultAsync();
                if (site != null)
                {
                    output.Add(new GetMenuBreadDto
                    {
                        Title = "Home",
                        Link = $"/{site.OrgName}/home",
                    });
                    var parentid = result.FK_RootNodeId;
                    if (parentid != null)
                    {
                        output.AddRange(await this.GetBread((long)parentid));
                    }
                    output.Add(new GetMenuBreadDto
                    {
                        Title = result.Title,
                        Link = $"/{site.OrgName}/{result.RouterName}",
                    });
                }
            }
            return output;
        }
        private async Task<List<GetMenuBreadDto>> GetBread(long Id)
        {
            var output = new List<GetMenuBreadDto>();
            var result = await db.WebMenus.Where(e => e.Id == Id && !e.IsDeleted).FirstOrDefaultAsync();
            if (result != null)
            {
                var parentid = result.FK_RootNodeId;
                var orgName = await loginUserData.GetWebsiteOrgName(result.FK_WebsiteId);
                if (parentid != null)
                {
                    output.AddRange(await this.GetMenuBread((long)parentid));
                }
                output.Add(new GetMenuBreadDto
                {
                    Title = result.Title,
                    Link = string.IsNullOrEmpty(result.RouterName) ? result.LinkUrl! : string.IsNullOrEmpty(htmlProcessor.text(stringHandler.HtmlDecode(result.Html))) ? "" : $"/{orgName}/{result.RouterName}",
                });
            }

            return output;
        }
        public async Task<ResponseMessageDto> CreateOrEdit(MenuItemDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                dto.icon = NormalizeMenuIcon(dto.icon);

                if (!string.IsNullOrEmpty(dto.RouterName))
                {
                    var siteId = await loginUserData.GetWebsiteId();
                    var menu = await db.WebMenus.Where(e => e.RouterName == dto.RouterName && e.FK_WebsiteId == siteId).FirstOrDefaultAsync();
                    if (menu != null && menu.Id != dto.Id) throw new Exception("此路由名稱已被使用，請更換其他名稱");
                }

                if (dto.Id == 0)
                {
                    long newId = await Create(dto);
                    response.Message = newId.ToString();
                }
                else await Update(dto);
                await websiteCacheStateAppService.TouchAsync(WebsiteCacheKeys.Menu);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }

        private static string NormalizeMenuIcon(string? icon)
        {
            var value = icon?.Trim();
            return string.IsNullOrEmpty(value)
                || value.Equals("material-symbols-outlined empty", StringComparison.OrdinalIgnoreCase)
                    ? "empty"
                    : value;
        }

        private async Task<long> Create(MenuItemDto dto)
        {
            long WebsiteID = await loginUserData.GetWebsiteId();
            var user = await loginUserData.GetUser();
            WebMenu menu = mapper.Map<WebMenu>(dto);
            menu.CreatorUserId = user.Id;
            menu.FK_WebsiteId = WebsiteID;
            db.WebMenus.Add(menu);
            await loginUserData.SaveChanges(menu);
            return menu.Id;
        }
        private async Task Update(MenuItemDto dto)
        {
            var menu = await db.WebMenus.FirstOrDefaultAsync(e => e.Id == dto.Id);
            var user = await loginUserData.GetUser();
            if (menu == null) throw new Exception("查無資料");
            mapper.Map(dto, menu);
            if (!string.IsNullOrEmpty(dto.IconUrl)) menu.icon = $"IconId:{dto.IconId}";
            menu.LastModificationTime = DateTime.Now;
            menu.LastModifierUserId = user.Id;
            await loginUserData.SaveChanges(menu);
        }
        public async Task<GetMenuContenDto> GetConten(SearchIDDto dto)
        {
            GetMenuContenDto results = new GetMenuContenDto();
            try
            {
                long siteId = await loginUserData.GetWebsiteId();
                var menu = await db.WebMenus.Where(e => e.FK_WebsiteId == siteId)
                                    .Where(e => e.Id == dto.Id)
                                    .Where(e => !e.IsDeleted)
                                    .FirstOrDefaultAsync();
                if (menu != null)
                {
                    results.Conten = mapper.Map<MenuSaveContenDto>(menu);
                    results.Conten.SaveHtml = HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(results.Conten.SaveHtml));
                    results.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.Error = ex.Message;
            }
            return results;
        }
        public async Task<GetFrontContenOutputDto> GetParentConten(GetFrontContenInputDto dto)
        {
            if (dto.siteId == null)
            {
                dto.siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            }
            GetFrontContenOutputDto result = new GetFrontContenOutputDto();
            try
            {
                var side = await db.Websites.Where(e => e.Id == dto.siteId).FirstOrDefaultAsync();
                var menu = await db.WebMenus.Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == dto.siteId).Where(e => e.RouterName == dto.key).FirstOrDefaultAsync();
                if (menu != null)
                {
                    var parent = await db.WebMenus.Where(e => !e.IsDeleted).Where(e => e.Id == menu.FK_TopNodeId).FirstOrDefaultAsync();
                    if (side != null)
                    {
                        result.SiteName = side.Title;
                        if (parent != null)
                        {
                            var sanitized = await EnsureMenuDisplayContentSanitizedAsync(parent);
                            mapper.Map(parent, result);
                            result.Html = stringHandler.HtmlEncode(sanitized.Html);
                            result.Css = sanitized.Css;
                            result.LastModificationTime = null;
                            result.CurrentUrl = $"/{parent.RouterName}";
                        }
                    }
                }
            }
            catch { }
            return result;
        }
        public async Task<GetFrontContenOutputDto> GetFrontConten(GetFrontContenInputDto dto)
        {
            if (dto.siteId == null)
            {
                dto.siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            }
            GetFrontContenOutputDto result = new GetFrontContenOutputDto();
            try
            {
                var side = await db.Websites.Where(e => e.Id == dto.siteId).FirstOrDefaultAsync();
                var menu = await db.WebMenus.Where(e => !e.IsDeleted).Where(e => !e.RemovedFromShelves).Where(e => e.FK_WebsiteId == dto.siteId)
                        .Where(e =>
                            e.RouterName.ToLower() == dto.key.ToLower() ||
                            (e.PageType == PageTypeEnum.購物車 && dto.key.ToLower() == "shoppingcar") ||
                            (e.PageType == PageTypeEnum.會員 && dto.key.ToLower() == "member") ||
                            (e.PageType == PageTypeEnum.搜尋 && dto.key.ToLower() == "search")
                        )
                        .FirstOrDefaultAsync();
                if (side != null)
                {
                    result.SiteName = side.Title;
                    if (menu != null)
                    {
                        var sanitized = await EnsureMenuDisplayContentSanitizedAsync(menu);
                        mapper.Map(menu, result);
                        result.Html = stringHandler.HtmlEncode(sanitized.Html);
                        result.Css = sanitized.Css;
                        result.LastModificationTime = null;
                        result.Html = result.Html.Replace("&lt;body&gt;", "").Replace("&lt;/body&gt;", "").Replace("&lt;content&gt;", "").Replace("&lt;/content&gt;", "");
                        result.CurrentUrl = $"/{menu.RouterName}";
                        result.VisibleFooter = menu.VisibleFooter;
                        result.VisibleHeader = menu.VisibleHeader;
                        var html = stringHandler.HtmlDecode(result.Html);
                        var images = htmlProcessor.Find(htmlProcessor.LoadHtml(html), "img");
                        if (images != null && images.Any())
                        {
                            result.ImageUrl = images[0].Attributes["src"].Value;
                        }
                    }
                }
            }
            catch { }
            return result;
        }
        public async Task<ResponseMessageDto> importConten(MenuSaveContenDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                dto.SaveHtml = HttpUtility.HtmlEncode(dto.SaveHtml);
                MenuContenDto importDto = mapper.Map<MenuContenDto>(dto);
                var s = await SaveContenInternal(dto, writeAuditLog: false);
                var user = await loginUserData.GetUser();
                var menu = await db.WebMenus.FirstOrDefaultAsync(e => e.Id == dto.Id);
                if (menu != null)
                {
                    string Orgname = await loginUserData.GetWebsiteOrgName();
                    importDto.Html = stringHandler.HtmlDecode(importDto.Html);
                    importDto.Html = htmlProcessor.RemoveNode(importDto.Html ?? "", ".backstageType");

                    importDto.Html = (importDto.Html ?? "").Replace($"/upload/{Orgname}/", "/upload/");
                    importDto.Css = (importDto.Css ?? "").Replace($"/upload/{Orgname}/", "/upload/");

                    var sanitized = await SanitizeMenuPublishedContentAsync(
                        menu.FK_WebsiteId,
                        menu.Id,
                        importDto.Html ?? "",
                        importDto.Css ?? "",
                        true
                    );

                    menu.PageText = htmlProcessor.text(sanitized.Html);
                    importDto.Html = stringHandler.HtmlEncode(sanitized.Html);
                    importDto.Css = sanitized.Css;
                    mapper.Map(importDto, menu);
                    await loginUserData.SaveChanges(menu);
                    response.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }

        private Task<HtmlSanitizeResult> SanitizeMenuPublishedContentAsync(
            long websiteId,
            long menuId,
            string html,
            string css,
            bool force = false)
        {
            return htmlSanitizeService.EnsurePublicContentAsync(new HtmlSanitizeInput
            {
                WebsiteId = websiteId,
                SourceType = HtmlSanitizeSourceType.選單,
                SourceId = menuId,
                ContentKey = "Published",
                SanitizePolicy = "PublicHtml",
                Html = html ?? "",
                Css = css ?? "",
                Force = force
            });
        }

        private async Task<(string Html, string Css)> EnsureMenuDisplayContentSanitizedAsync(WebMenu menu)
        {
            var publishedHtml = stringHandler.HtmlDecode(menu.Html ?? "");
            var restoredHtml = htmlSanitizeService.RepairLegacyPublishedHtml(
                publishedHtml,
                stringHandler.HtmlDecode(menu.SaveHtml ?? "")
            );
            var repairedLegacyHtml = !string.Equals(
                publishedHtml,
                restoredHtml,
                StringComparison.Ordinal
            );

            var sanitized = await SanitizeMenuPublishedContentAsync(
                menu.FK_WebsiteId,
                menu.Id,
                restoredHtml,
                menu.Css ?? "",
                repairedLegacyHtml
            );

            if (sanitized.WasSanitized)
            {
                menu.Html = stringHandler.HtmlEncode(sanitized.Html);
                menu.Css = sanitized.Css;
                menu.PageText = htmlProcessor.text(sanitized.Html);
                await loginUserData.SaveChanges(menu);
            }

            return (sanitized.Html, sanitized.Css);
        }
        public Task<ResponseMessageDto> saveConten(MenuSaveContenDto dto)
        {
            return SaveContenInternal(dto, writeAuditLog: true);
        }
        private async Task<ResponseMessageDto> SaveContenInternal(MenuSaveContenDto dto, bool writeAuditLog)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                dto.SaveHtml = HttpUtility.HtmlEncode(dto.SaveHtml);
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
            if (writeAuditLog)
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }
        public async Task<ResponseMessageDto> Delete(DataDelectDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto { Success = true };
            try
            {
                var user = await loginUserData.GetUser();
                long siteID = await loginUserData.GetWebsiteId();
                var item = await db.WebMenus.Include(e => e.FK_ChildNodes)
                        .Where(e => e.Id == dto.Id)
                        .Where(e => e.FK_WebsiteId == siteID)
                        .FirstOrDefaultAsync();
                if (item == null) throw new Exception("資料不存在");
                else if (item.FK_ChildNodes != null && item.FK_ChildNodes.Any()) throw new Exception("該選單還有其他子選單，無法刪除");
                else
                {
                    item.IsDeleted = true;
                    await loginUserData.SaveChanges(item);

                    if (item.ImgId != null)
                    {
                        var delete_image = await fileUploadAppService.deleteFileById(new FileDeleteDto()
                        {
                            Sid = item.Id,
                            Fid = new List<long> { item.ImgId.Value },
                            Type = (int)FileBindTypeEnum.選單圖,
                        });
                    }

                    if (item.OverImgId != null)
                    {
                        var delete_overImage = await fileUploadAppService.deleteFileById(new FileDeleteDto()
                        {
                            Sid = item.Id,
                            Fid = new List<long> { item.OverImgId.Value },
                            Type = (int)FileBindTypeEnum.選單覆蓋,
                        });
                    }
                }
                await websiteCacheStateAppService.TouchAsync(WebsiteCacheKeys.Menu);
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
                response.Success = false;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> updateSerNo(UpdateSerNoListDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto { Success = true };
            try
            {
                long webSiteId = await loginUserData.GetWebsiteId();
                var o = (from s in dto.list select s.Id).ToList();
                var result = db.WebMenus.Where(e => o.Contains(e.Id) && e.FK_WebsiteId == webSiteId);
                foreach (var e in dto.list)
                {
                    var item = await result.Where(m => m.Id == e.Id).FirstOrDefaultAsync();
                    if (item != null)
                    {
                        mapper.Map(e, item);
                        await loginUserData.SaveChanges(item);
                    }
                }
                await websiteCacheStateAppService.TouchAsync(WebsiteCacheKeys.Menu);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.ToString();
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> SetVisible(SetVisibleDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                long webSiteId = await loginUserData.GetWebsiteId();
                var menu = await db.WebMenus.Where(e => e.Id == dto.Id && e.FK_WebsiteId == webSiteId).FirstOrDefaultAsync();
                if (menu != null)
                {
                    menu.Visible = dto.IsVisible;
                    await loginUserData.SaveChanges(menu);
                    await websiteCacheStateAppService.TouchAsync(WebsiteCacheKeys.Menu);
                    response.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.ToString();
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public PageTypeDto GetPageTypeList()
        {
            PageTypeDto response = new PageTypeDto { Success = true };

            try
            {
                response.Type = Enum.GetValues(typeof(PageTypeEnum))
                    .Cast<PageTypeEnum>()
                    .Select(GetPageTypeOption)
                    .ToList();
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Error = e.Message;
            }

            return response;
        }

        private static PageTypeOptionDto GetPageTypeOption(PageTypeEnum type)
        {
            return type switch
            {
                PageTypeEnum.一般頁面 => new PageTypeOptionDto
                {
                    Key = type.ToString(),
                    Value = (int)type,
                    Description = "一般內容頁",
                    RouterName = "",
                    ShowRouterName = true,
                    ShowLinkUrl = true
                },

                PageTypeEnum.首頁 => new PageTypeOptionDto
                {
                    Key = type.ToString(),
                    Value = (int)type,
                    Description = "網站首頁",
                    RouterName = "Home",
                    ShowRouterName = false,
                    ShowLinkUrl = true
                },

                PageTypeEnum.購物車 => new PageTypeOptionDto
                {
                    Key = type.ToString(),
                    Value = (int)type,
                    Description = "購物車功能頁",
                    RouterName = "ShoppingCar",
                    ShowRouterName = false,
                    ShowLinkUrl = true
                },

                PageTypeEnum.會員 => new PageTypeOptionDto
                {
                    Key = type.ToString(),
                    Value = (int)type,
                    Description = "會員功能頁",
                    RouterName = "Member",
                    ShowRouterName = false,
                    ShowLinkUrl = true
                },

                PageTypeEnum.搜尋 => new PageTypeOptionDto
                {
                    Key = type.ToString(),
                    Value = (int)type,
                    Description = "搜尋功能頁",
                    RouterName = "Search",
                    ShowRouterName = false,
                    ShowLinkUrl = true
                },

                PageTypeEnum.跳頁 => new PageTypeOptionDto
                {
                    Key = type.ToString(),
                    Value = (int)type,
                    Description = "設定連結或建立路徑轉址",
                    RouterName = "",
                    ShowRouterName = true,
                    ShowLinkUrl = true
                },

                PageTypeEnum.結構頁面 => new PageTypeOptionDto
                {
                    Key = type.ToString(),
                    Value = (int)type,
                    Description = "分類或繼承用途",
                    RouterName = "",
                    ShowRouterName = false,
                    ShowLinkUrl = false
                },

                _ => new PageTypeOptionDto
                {
                    Key = type.ToString(),
                    Value = (int)type,
                    Description = null,
                    RouterName = "",
                    ShowRouterName = false,
                    ShowLinkUrl = true
                }
            };
        }
        public async Task insertMenus(List<SelectDto> menus)
        {
            long webSite = await loginUserData.GetWebsiteId();
            long userId = await loginUserData.GetUserId();
            List<WebMenu> newMenus = new List<WebMenu>();
            menus.ForEach(e =>
            {
                WebMenu menu = new WebMenu
                {
                    Title = e.Name,
                    RouterName = e.Name,
                    Visible = true,
                    SerNO = 500,
                    Popular = 0,
                    PageType = PageTypeEnum.一般頁面,
                    icon = "empty",
                    PopularVisible = false,
                    LanBar = false,
                    FK_WebsiteId = webSite,
                    CreationTime = DateTime.Now,
                    CreatorUserId = userId,
                    IsDeleted = false,
                    VisibleFooter = true,
                    VisibleHeader = true,
                    VisibleTitle = true,
                    ShowToMenu = true,
                    RemovedFromShelves = false
                };
                newMenus.Add(menu);
            });
            db.WebMenus.AddRange(newMenus);
            await db.SaveChangesAsync();
            await websiteCacheStateAppService.TouchAsync(WebsiteCacheKeys.Menu);
        }
        public async Task<bool> checkHasShoppingCar(long siteId)
        {
            var item = db.WebMenus.Where(e => !e.IsDeleted && e.FK_WebsiteId == siteId && !e.RemovedFromShelves && e.PageType == PageTypeEnum.購物車);
            return item.Any();
        }
        public async Task<bool> checkHasMember(long siteId)
        {
            var item = db.WebMenus.Where(e => !e.IsDeleted && e.FK_WebsiteId == siteId && !e.RemovedFromShelves && e.PageType == PageTypeEnum.會員);
            return item.Any();
        }
        public async Task<long> GetRootId(string name)
        {
            name = name.ToLower().Trim();
            var siteId = loginUserData.GetFrontWebsiteId();
            var menu = await db.WebMenus.Include(e => e.FK_RootNode).Where(e => e.FK_WebsiteId == siteId &&
                ((e.RouterName == name && e.PageType == PageTypeEnum.一般頁面) || (name == "home" && new List<string?> { "/", "/home" }.Contains(e.LinkUrl)))).FirstOrDefaultAsync();
            if (menu != null && menu.FK_RootNode != null) return menu.FK_RootNode.Id;
            else if (menu != null) return menu.Id;
            return 0;
        }
        public async Task<List<JumpRuleDto>> GetJumpRulesAsync()
        {
            var WebsiteID = loginUserData.GetFrontWebsiteId();
            var list = await db.WebMenus
                .Where(x => !string.IsNullOrEmpty(x.RouterName)
                         && !string.IsNullOrEmpty(x.LinkUrl)
                         && x.PageType == PageTypeEnum.跳頁
                         && x.FK_WebsiteId == WebsiteID
                         && !x.RemovedFromShelves) // 視你的欄位
                .Select(x => new JumpRuleDto
                {
                    RouteName = x.RouterName,
                    TargetUrl = x.LinkUrl!,
                })
                .ToListAsync();
            return list;
        }
    }
}
