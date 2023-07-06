using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Web;

namespace EtheriT.Coker.Application
{
    public class WebMenuApplication : IWebMenuApplication
    {
        private readonly string ApplicationName;
        private readonly CokerDbContext db;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly IConfiguration Configuration;
        private readonly IFileUploadAppService fileUploadAppService;
        public WebMenuApplication(
            CokerDbContext db,
            IHttpContextAccessor httpContextAccessor,
            LoginUserData loginUserData,
            IMapper mapper,
            IConfiguration Configuration,
            IFileUploadAppService fileUploadAppService
            )
        {
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.Configuration = Configuration;
            this.ApplicationName = "WebMenu";
            this.fileUploadAppService = fileUploadAppService;
        }
        public async Task<SiteMapDto> GetAll()
        {
            SiteMapDto response = new SiteMapDto { Success = false };
            try
            {
                response.Maps = await GetChild(null);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(ApplicationName, "GetAll", "", JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<SiteMapDto> GetDisplayAll(long WebsiteID)
        {
            SiteMapDto response = new SiteMapDto { Success = false };
            try
            {
                response.Maps = await GetDisplayChild(null, WebsiteID);
                foreach (var map in response.Maps)
                {
                    var imgurl = await fileUploadAppService.getImgUrl(map.ImgId, (long)WebsiteID);
                    map.ImgUrl = imgurl;
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        private async Task<List<MenuItemDto>> GetChild(long? id)
        {
            try
            {
                var WebsiteID = await loginUserData.GetWebsiteId();
                var orgName = await loginUserData.GetWebsiteOrgName();
                var menus = await db.WebMenus
                            .Where(m => m.FK_TopNodeId == id)
                            .Where(m => m.FK_WebsiteId == WebsiteID)
                            .Where(m => !m.IsDeleted)
                            .OrderBy(m => m.SerNO)
                            .ThenBy(m => m.Id)
                            .ToListAsync();
                List<MenuItemDto> result = mapper.Map<List<MenuItemDto>>(menus);
                foreach (var m in result)
                {
                    m.Children = await GetChild(m.Id);
                    if (m.ImgId != null)
                    {
                        var data = await fileUploadAppService.getImgFiles(new FileGetImgInputDto()
                        {
                            Sid = m.Id,
                            Type = 2,
                            Size = 1,
                        });
                        m.ImgUrl = data[0].Link;
                        m.ImgName = data[0].Name;
                    }
                    if (m.OverImgId != null)
                    {
                        var data = await fileUploadAppService.getImgFiles(new FileGetImgInputDto()
                        {
                            Sid = m.Id,
                            Type = 3,
                            Size = 1,
                        });
                        m.OverImgUrl = data[0].Link;
                        m.OverImgName = data[0].Name;
                    }
                    if (m.icon.StartsWith("IconId"))
                    {
                        var data = await fileUploadAppService.getImgFiles(new FileGetImgInputDto()
                        {
                            Sid = m.Id,
                            Type = 9,
                            Size = 1,
                        });
                        if (data != null && data.Any()) {
							m.IconId = m.icon.Split(":")[1];
							m.IconUrl = data[0].Link;
						}
						m.icon = "empty";
					}
                    if (m.Children.Count == 0) m.Children = null;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private async Task<List<MenuItemDto>> GetDisplayChild(long? id, long WebsiteID)
        {
            try
            {
                var menus = await db.WebMenus
                            .Where(m => m.FK_TopNodeId == id)
                            .Where(m => m.FK_WebsiteId == WebsiteID)
                            .Where(m => !m.IsDeleted)
                            .Where(m => m.Visible)
                            .OrderBy(m => m.SerNO)
                            .ThenBy(m => m.Id)
                            .ToListAsync();
                List<MenuItemDto> result = mapper.Map<List<MenuItemDto>>(menus);
                foreach (var m in result)
                {
                    if (m.icon.StartsWith("IconId"))
                    {
                        var iconimage = await fileUploadAppService.getImgUrl(long.Parse(m.icon.Split(":")[1]), (long)WebsiteID);
                        m.IconImage = iconimage;
                    }
                    m.Children = await GetDisplayChild(m.Id, WebsiteID);
                    if (m.Children.Count == 0) m.Children = null;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("資料錯誤");
            }
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var WebstieId = await loginUserData.GetWebsiteId();
                var results = await db.WebMenus.Where(e => !e.IsDeleted && e.FK_WebsiteId == WebstieId).ToListAsync();
                if (results.Count > 0)
                {
                    var outputlist = new List<MenuGetAllListDto>();
                    for (var i = 0; i < results.Count; i++)
                    {
                        MenuGetAllListDto outputdata = mapper.Map(results[i], new MenuGetAllListDto());
                        var outputdata_child = await this.GetChild(outputdata.Id);
                        if (outputdata_child.Count > 0)
                        {
                            outputdata.Items = "";
                            for (var j = 0; j < outputdata_child.Count; j++)
                            {
                                if (j >= 3)
                                {
                                    outputdata.Items += "...";
                                    break;
                                }
                                outputdata.Items += outputdata.Items == "" ? outputdata_child[j].Title : $"、{outputdata_child[j].Title}";
                            }
                        }
                        outputlist.Add(outputdata);
                    }
                    var output = DataSourceLoader.Load(outputlist, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                return new JsonResult(new List<ArticleDataGetDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                    var outputdata_child = await this.GetChild(output.Id);
                    if (outputdata_child.Count > 0)
                    {
                        output.Items = "";
                        for (var j = 0; j < outputdata_child.Count; j++)
                        {
                            if (j >= 3)
                            {
                                output.Items += "...";
                                break;
                            }
                            output.Items += output.Items == "" ? outputdata_child[j].Title : $"、{outputdata_child[j].Title}";
                        }
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
                                    where !w.IsDeleted && w.FK_WebsiteId == dto.WebsiteId
                                    select new MenuItemDto
                                    {
                                        Id = w.Id,
                                        Title = w.Title,
                                        RouterName = w.RouterName,
                                        Children = new List<MenuItemDto>()
                                    }).FirstOrDefaultAsync();
                if (output != null) output.Children = await this.GetDisplayChild(dto.Id, dto.WebsiteId);
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
            if (result != null && result.PageType != (int)PageTypeEnum.首頁)
            {
                output.Add(new GetMenuBreadDto
                {
                    Title = "Home",
                    Link = "home",
                });
                var parentid = result.FK_RootNodeId;
                if (parentid != null)
                {
                    output.AddRange(await this.GetBread((long)parentid));
                }
                output.Add(new GetMenuBreadDto
                {
                    Title = result.Title,
                    Link = result.RouterName,
                });
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
                if (parentid != null)
                {
                    output.AddRange(await this.GetMenuBread((long)parentid));
                }
                output.Add(new GetMenuBreadDto
                {
                    Title = result.Title,
                    Link = result.RouterName,
                });
            }

            return output;
        }
        public async Task<ResponseMessageDto> CreateOrEdit(MenuItemDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                if (dto.Id == 0)
                {
                    long newId = await Create(dto);
                    response.Message = newId.ToString();
                }
                else await Update(dto);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(ApplicationName, "CreateOrEdit", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
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
            if (dto.IconUrl != null) menu.icon = $"IconId:{dto.IconId}";
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
                var menu = await db.WebMenus.Where(e => e.FK_WebsiteId == dto.siteId).Where(e => e.RouterName == dto.key).FirstOrDefaultAsync();
                if (side != null)
                {
                    result.SiteName = side.Title;
                    if (menu != null)
                    {
                        mapper.Map(menu, result);
                        result.Html = result.Html.Replace("&lt;body&gt;", "").Replace("&lt;/body&gt;", "");
                        result.CurrentUrl = $"/{menu.RouterName}";
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
                var s = await saveConten(dto);
                var user = await loginUserData.GetUser();
                var menu = await db.WebMenus.FirstOrDefaultAsync(e => e.Id == dto.Id);
                if (menu != null)
                {
                    string Orgname = await loginUserData.GetWebsiteOrgName();
                    importDto.Html = (importDto.Html ?? "").Replace($"/upload/{Orgname}/", "/upload/");
                    importDto.Css = (importDto.Css ?? "").Replace($"/upload/{Orgname}/", "/upload/");
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
            return response;
        }
        public async Task<ResponseMessageDto> saveConten(MenuSaveContenDto dto)
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
            return response;
        }
        public async Task<ResponseMessageDto> Delete(DataDelectDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto { Success = true };
            try
            {
                var user = await loginUserData.GetUser();
                long siteID = await loginUserData.GetWebsiteId();
                var item = await db.WebMenus
                        .Where(e => e.Id == dto.Id)
                        .Where(e => e.FK_WebsiteId == siteID)
                        .FirstOrDefaultAsync();
                if (item == null) throw new Exception("資料不存在");
                else
                {
                    item.IsDeleted = true;
                    await loginUserData.SaveChanges(item);

                    if (item.ImgId != null)
                    {
                        var m_imgid_list = new List<long>();
                        m_imgid_list.Add((long)item.ImgId);
                        var delete_image = await fileUploadAppService.deleteFileById(new FileDeleteDto()
                        {
                            Sid = item.Id,
                            Fid = m_imgid_list,
                            Type = (int)FileBindTypeEnum.選單圖,
                        });
                    }

                    if (item.OverImgId != null)
                    {
                        var o_imgid_list = new List<long>();
                        o_imgid_list.Add((long)item.OverImgId);
                        var delete_overImage = await fileUploadAppService.deleteFileById(new FileDeleteDto()
                        {
                            Sid = item.Id,
                            Fid = o_imgid_list,
                            Type = (int)FileBindTypeEnum.選單覆蓋,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
            }
            return response;
        }
        public async Task<ResponseMessageDto> updateSerNo(UpdateSerNoListDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto { Success = true };
            try
            {
                var o = (from s in dto.list select s.Id).ToList();
                var result = db.WebMenus.Where(e => o.Contains(e.Id));
                foreach (var e in dto.list)
                {
                    var item = await result.Where(m => m.Id == e.Id).FirstOrDefaultAsync();
                    if (item != null)
                    {
                        mapper.Map(e, item);
                        await loginUserData.SaveChanges(item);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.ToString();
            }
            await loginUserData.SetLogs(ApplicationName, "updateSerNo", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<PageTypeDto> GetPageTypeList()
        {
            PageTypeDto response = new PageTypeDto { Success = true };
            try
            {
                response.Type = Enum.GetValues(typeof(PageTypeEnum))
                .Cast<PageTypeEnum>().Select(e =>
                {
                    return new EnumDictionaryDto { Key = e.ToString(), Value = (int)e };
                }).ToList();
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Error = e.Message;
            }
            return response;
        }
    }
}
