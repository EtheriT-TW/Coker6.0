using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.Application.Shared.Dto;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using System.Linq;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Core.Models;

namespace EtheriT.Coker.Application.Directory
{
    public class DirectoryAppService : IDirectoryAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITagAppService tagAppService;
        private readonly IMapper mapper;
        private readonly IArticleAppService articleAppService;
        private readonly IProductAppService productAppService;
        private readonly IWebMenuApplication webMenuApplicationService;
        private readonly StringHandler stringHandler;
        public DirectoryAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            StringHandler stringHandler,
            IMapper mapper,
            ITagAppService tagAppService,
            IArticleAppService articleAppService,
            IProductAppService productAppService,
            IWebMenuApplication webMenuApplicationService,
            IConfiguration configuration
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.stringHandler = stringHandler;
            this.mapper = mapper;
            this.tagAppService = tagAppService;
            this.articleAppService = articleAppService;
            this.productAppService = productAppService;
            this.webMenuApplicationService = webMenuApplicationService;
        }
        public async Task<ResponseMessageDto> AddUp(DirectoryAddUpDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tag_response = new ResponseMessageDto() { Success = true };

            try
            {
                long userid = await loginUserData.GetUserId();
                var asoid = dto.Id;

                if (dto.Id == 0)
                {
                    if (userid != 0)
                    {
                        long WebsiteID = await loginUserData.GetWebsiteId();
                        Core.Models.Directory newItem = mapper.Map<Core.Models.Directory>(dto);
                        newItem.FK_WebsiteId = WebsiteID;
                        if (dto.Type == 3) newItem.FK_Mid = dto.FK_Mid;
                        db.Directory.Add(newItem);
                        await loginUserData.SaveChanges(newItem);
                        asoid = newItem.Id;
                    }
                    else throw new Exception("查無資料");
                }
                else
                {
                    var db_d = db.Directory.Where(e => e.Id == dto.Id).FirstOrDefault();
                    if (db_d != null)
                    {
                        db_d = mapper.Map(dto, db_d);
                        if (dto.Type == 3) db_d.FK_Mid = dto.FK_Mid;
                        await loginUserData.SaveChanges(db_d);
                    }
                    else throw new Exception("查無資料");
                }

                if (asoid != null && (dto.Type == (int)DirectoryTypeEnum.商品 || dto.Type == (int)DirectoryTypeEnum.文章))
                {
                    var tagitem = new List<TagAssociateDto>();
                    foreach (var data in dto.TagSelected)
                    {
                        tagitem.Add(new TagAssociateDto()
                        {
                            Id = data.Id,
                            FK_AId = (long)asoid,
                            FK_TId = data.FK_TId,
                            Type = (int)TagAssociateTypeEnum.目錄,
                            IsDeleted = data.IsDeleted
                        });
                    }
                    tag_response = await tagAppService.TagAssociateAddDelect(tagitem);
                }

                output.Success = tag_response.Success;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            return output;
        }
        public async Task<DirectoryGetDataDto> GetDataOne(long Id)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Directory.Where(e => e.Id == Id && !e.IsDeleted && e.FK_WebsiteId == WebsiteID);

                if (result != null)
                {
                    var output = await (from e in result
                                        select new DirectoryGetDataDto
                                        {
                                            Id = e.Id,
                                            Title = e.Title,
                                            Description = e.Description,
                                            Visible = e.Visible,
                                            Type = e.Type,
                                            TagDatas = new List<TagGetSelectedDto>(),
                                            FK_MId = e.FK_Mid
                                        }).FirstOrDefaultAsync();

                    var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                    {
                        Fk_Aid = output.Id,
                        Type = (int)TagAssociateTypeEnum.目錄,
                    }
                    );

                    if (tagDatas != null)
                    {
                        output.TagDatas = tagDatas;
                    }

                    return output;
                }
                else throw new Exception("查無文章資料");
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private async Task<DirectoryReleInfoGetDto> SearchReleInfo(DirectoryReleInfoInputDto dto)
        {
            var output = new DirectoryReleInfoGetDto { ReleInfos = new List<DirectoryReleInfoDto>() };
            long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
            long SearchId = dto.Ids.Count > 0 ? dto.Ids[0] : 0;
            int page = (int)dto.Page;
            int shownum = (int)dto.ShowNum;
            if (string.IsNullOrEmpty(dto.SearchText))
            {
                output.TotalCount = 0;
                output.TotalPage = 0;
                return output;
            }
            Regex imgRegex = new Regex("(?:src=[\\S]*quot;)[\\S]*(?:quot;)", RegexOptions.IgnoreCase);
            var data1 = db.WebMenus.Include(e => e.Website).Where(e => !e.IsDeleted)
                            .Where(e => e.FK_WebsiteId == WebsiteID)
                            .Where(e => e.Visible)
                            .Where(e => e.RouterName.ToLower() != "home")
                            .Where(e =>
                                (e.Title ?? "").Contains(dto.SearchText ?? "") ||
                                (e.Html ?? "").Contains(dto.SearchText ?? "")
                            );
            var data2 = db.Article.Include(e => e.Website).Where(e => !e.IsDeleted)
                            .Where(e => e.FK_WebsiteId == WebsiteID)
                            .Where(e => e.Visible)
                            .Where(e => 
                                e.permanent||
                                ( e.StartTime.Value <= DateTime.Now && e.EndTime.Value >= DateTime.Now)
                            )
                            .Where(e =>
                                (e.Title ?? "").Contains(dto.SearchText ?? "") ||
                                (e.Html ?? "").Contains(dto.SearchText ?? "")
                            );
            int skip = (page - 1) * shownum - 1;
            if (skip < 0) skip = 0;
            //Regex.Replace(m.Html, @"<(.|\n)*?>", "")
            switch (SearchId)
            {
                case 1:
                    var art1 = await (
                        from a in data2
                        join ta in db.Tag_Associates.Where(e => !e.IsDeleted) on a.Id equals ta.FK_AId
                        join t in db.Tags.Where(e => !e.IsDeleted) on ta.FK_TId equals t.Id
                        where t.FK_WebsiteId == WebsiteID && t.Id == 42
                        select new DirectoryReleInfoDto
                        {
                            Id = a.Id,
                            Title = a.Title,
                            Link = $"/article/",
                            type = DirectoryTypeEnum.文章,
                            OrgName = a.Website.OrgName,
                            SerNo = a.SerNO,
                            Description = a.Description,
                            MainImage = a.Html,
                        }
                    ).ToListAsync();
                    var dataMargin1 = art1.OrderBy(e => e.NodeDate)
                        .ThenBy(e => e.SerNo)
                        .ThenByDescending(e => e.Id)
                        .Skip(skip).Take(shownum);
                    var list1 = dataMargin1.Select(e => new DirectoryReleInfoDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Link = e.type == DirectoryTypeEnum.文章 ? e.Link + e.Id : "/" + e.Link,
                        OrgName = e.OrgName,
                        type = e.type,
                        SerNo = e.SerNo,
                        Description = string.IsNullOrEmpty(e.Description) ? getSearchDescription(e.MainImage, dto.SearchText) : getSearchDescription(e.Description, dto.SearchText),
                        MainImage = imgRegex.Match(e.MainImage ?? "").Value.Replace("quot;", "").Replace("src=&", "").Replace("&", "").Replace("amp;", "")
                    }).ToList();
                    output.TotalCount = art1.Count();
                    output.ReleInfos.AddRange(list1);
                    break;
                case 2:
                    var art2 = await (
                        from a in data2
                        join ta in db.Tag_Associates.Where(e => !e.IsDeleted) on a.Id equals ta.FK_AId
                        join t in db.Tags.Where(e => !e.IsDeleted) on ta.FK_TId equals t.Id
                        where t.FK_WebsiteId == WebsiteID && t.Id == 46
                        select new DirectoryReleInfoDto
                        {
                            Id = a.Id,
                            Title = a.Title,
                            Link = $"/article/",
                            type = DirectoryTypeEnum.文章,
                            OrgName = a.Website.OrgName,
                            SerNo = a.SerNO,
                            Description = a.Description,
                            MainImage = a.Html,
                        }
                    ).ToListAsync();
                    var dataMargin2 = art2.OrderBy(e => e.NodeDate)
                        .ThenBy(e => e.SerNo)
                        .ThenByDescending(e => e.Id)
                        .Skip(skip).Take(shownum);
                    var list2 = dataMargin2.Select(e => new DirectoryReleInfoDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Link = e.type == DirectoryTypeEnum.文章 ? e.Link + e.Id : "/" + e.Link,
                        OrgName = e.OrgName,
                        type = e.type,
                        SerNo = e.SerNo,
                        Description = string.IsNullOrEmpty(e.Description) ? getSearchDescription(e.MainImage, dto.SearchText) : getSearchDescription(e.Description, dto.SearchText),
                        MainImage = imgRegex.Match(e.MainImage ?? "").Value.Replace("quot;", "").Replace("src=&", "").Replace("&", "").Replace("amp;", "")
                    }).ToList();
                    output.TotalCount = art2.Count();
                    output.ReleInfos.AddRange(list2);
                    break;
                default:
                    var menu = await data1.Select(m => new DirectoryReleInfoDto
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Link = Convert.ToString(m.RouterName),
                        OrgName = m.Website.OrgName,
                        type = DirectoryTypeEnum.選單,
                        SerNo = m.SerNO,
                        Description = m.Description,
                        MainImage = m.Html,
                    }).ToListAsync();
                    var art = await data2.Select(a => new DirectoryReleInfoDto
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Link = $"/article/",
                        type = DirectoryTypeEnum.文章,
                        OrgName = a.Website.OrgName,
                        SerNo = a.SerNO,
                        Description = a.Description,
                        MainImage = a.Html,
                    }).ToListAsync();

                    var mCount = menu.Count();
                    var aCount = art.Count();

                    var dataMargin = menu.Union(art).OrderBy(e => e.NodeDate)
                        .ThenBy(e => e.SerNo)
                        .ThenByDescending(e => e.Id)
                        .Skip(skip).Take(shownum);

                    var list = dataMargin.Select(e => new DirectoryReleInfoDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Link = e.type == DirectoryTypeEnum.文章 ? e.Link + e.Id : "/" + e.Link,
                        OrgName = e.OrgName,
                        type = e.type,
                        SerNo = e.SerNo,
                        Description = string.IsNullOrEmpty(e.Description) ? getSearchDescription(e.MainImage, dto.SearchText) : getSearchDescription(e.Description, dto.SearchText),
                        MainImage = imgRegex.Match(e.MainImage ?? "").Value.Replace("quot;", "").Replace("src=&", "").Replace("&", "").Replace("amp;", "")
                    }).ToList();

                    output.TotalCount = mCount + aCount;
                    output.ReleInfos.AddRange(list);
                    break;
            }
            output.TotalPage = (int)Math.Ceiling(output.TotalCount / (double)shownum);
            return output;
        }
        private string getSearchDescription(string? conten,string findstr) {
            string s = Regex.Replace(stringHandler.HtmlDecode(conten), @"<(.|\n)*?>", "");
            int index = s.IndexOf(findstr) - 10;
            if(index<0) index = 0;
            s = s.Substring(index);
            return s.Replace(findstr,$"<span class='text-bg-warning text-dark'>{findstr}</span>");
        }
        public async Task<DirectoryReleInfoGetDto> GetReleInfo(DirectoryReleInfoInputDto dto)
        {
            if ((dto.Type ?? "").ToLower() == "search") return await SearchReleInfo(dto);
            var DataIds = new List<long>();
            long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
            List<long> siteIds = await db.MappingWebsiteRelationship.Where(e => e.FatherId == WebsiteID).Where(e => !e.IsDeleted).Select(e => e.Id).ToListAsync();
            siteIds.Add(WebsiteID);
            var output = new DirectoryReleInfoGetDto();
            var db_d = db.Directory.Where(e => e.Id == dto.Ids[0] && e.FK_WebsiteId == WebsiteID && !e.IsDeleted).FirstOrDefault();

            if (db_d != null)
            {
                var tags = await db.Tag_Associates.Include(e => e.Tag)
                    .Where(e => dto.Ids.Contains(e.FK_AId))
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.Type == (int)TagAssociateTypeEnum.目錄)
                    .Where(e => siteIds.Contains(e.Tag.FK_WebsiteId))
                    .ToListAsync();

                if (tags != null)
                {
                    switch ((DirectoryTypeEnum)db_d.Type)
                    {
                        case DirectoryTypeEnum.商品:
                            var product_datas = new List<ProdGetDataDto>();
                            foreach (var tag in tags)
                            {
                                var db_ps = await (db.Tag_Associates.Where(e => e.FK_TId == tag.FK_TId && e.Type == (int)TagAssociateTypeEnum.商品 && !e.IsDeleted)).ToListAsync();
                                if (db_ps != null)
                                {
                                    foreach (var db_p in db_ps)
                                    {
                                        product_datas.Add(await productAppService.GetProdDataOne(db_p.FK_AId));
                                    }
                                }
                            }
                            foreach (var product_data in product_datas)
                            {
                                if (!DataIds.Contains(product_data.Id))
                                {
                                    DataIds.Add(product_data.Id);
                                }
                            }
                            break;
                        case DirectoryTypeEnum.文章:
                            var FKTIds = tags.Select(e => e.FK_TId).ToList();
                            var db_as = await db.Tag_Associates
                                    .Where(e => !e.IsDeleted)
                                    .Where(e => e.Type == (int)TagAssociateTypeEnum.文章)
                                    .Where(e => FKTIds.Contains(e.FK_TId))
                                    .ToListAsync();
                            if (db_as != null)
                            {
                                var allIds = db_as.Select(e => e.FK_AId).ToList();
                                DataIds = db.Article
                                    .Where(e => allIds.Contains(e.Id))
                                    .Where(e => !e.IsDeleted)
                                    .Where(e => e.Visible)
                                    .Where(e => siteIds.Contains(e.FK_WebsiteId))
                                    .Where(e => e.permanent || (DateTime.Now >= e.StartTime && DateTime.Now <= e.EndTime))
                                    .Select(e => e.Id).ToList();
                            }
                            break;
                        default:
                            break;
                    }

                    var page = (int)dto.Page;
                    var shownum = (int)dto.ShowNum;
                    if (dto.MaxLen == null) dto.MaxLen = 0;
                    if (DataIds.Count < dto.MaxLen || dto.MaxLen == 0)
                    {
                        output.TotalPage = (int)Math.Ceiling(DataIds.Count / (double)shownum);
                        output.TotalCount = DataIds.Count;
                    }
                    else
                    {
                        output.TotalPage = (int)Math.Ceiling(dto.MaxLen.Value / (double)shownum);
                        output.TotalCount = dto.MaxLen.Value;
                    }
                    switch ((DirectoryTypeEnum)db_d.Type)
                    {
                        case DirectoryTypeEnum.商品:
                            var tempproddata = await productAppService.GetDirectoryReleInfo(new DirectoryReleInfoInputDto
                            {
                                Ids = DataIds.Skip((page - 1) * shownum - 1).Take(shownum).ToList<long>(),
                                SiteId = WebsiteID
                            });
                            if (tempproddata != null)
                            {
                                output.ReleInfos = tempproddata;
                            }

                            break;
                        case DirectoryTypeEnum.文章:
                            var temparticledata = await articleAppService.GetDirectoryReleInfo(new DirectoryReleInfoInputDto
                            {
                                Ids = DataIds,
                                Page = page,
                                ShowNum = shownum,
                                SiteId = WebsiteID,
                                MaxLen = dto.MaxLen
                            });
                            if (temparticledata != null)
                            {
                                output.ReleInfos = temparticledata;
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
            return output;
        }
        public async Task<MenuItemDto> GetReleMenu(DataIdWebsiteIdDto dto)
        {
            var websiteid = dto.WebsiteId;
            if (websiteid == 0) websiteid = await loginUserData.GetWebsiteId();
            var output = await (from e in db.Directory where e.Id == dto.Id && !e.IsDeleted select e.FK_Mid).FirstOrDefaultAsync();
            if (output != null) return await webMenuApplicationService.GetDisplayOne(new DataIdWebsiteIdDto()
            {
                Id = (long)output,
                WebsiteId = websiteid
            });
            return null;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                long UserID = await loginUserData.GetUserId();
                List<long> RoleIds = await loginUserData.GetUserRoleIds();
                var per = await db.PermissionDetail.Where(e => e.FK_WebsiteId == WebsiteID)
                        .Where(e => e.FK_UserId == UserID || (e.FK_RoleId != null && RoleIds.Contains(e.FK_RoleId.Value)))
                        .Where(e => e.Type == 3)
                        .Where(e => e.IsGranted).Select(e => e.FK_TargetId).ToListAsync();
                IQueryable<Core.Models.Directory> result = db.Directory.Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == WebsiteID);
                if (per!= null && per.Any()) result = result.Where(e => per.Contains(e.Id));
                if (result != null)
                {
                    var dataQuery = from e in result
                                    where new List<int> { 1, 2, 3 }.Contains(e.Type)
                                    select new DirectoryGetListDto
                                    {
                                        Id = e.Id,
                                        Title = e.Title,
                                        Description = e.Description,
                                        Type = ((DirectoryTypeEnum)e.Type).ToString(),
                                        Visible = e.Visible,
                                        Items = "",
                                        FK_Mid = e.FK_Mid,
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    if (output != null)
                    {
                        foreach (var data in output.data)
                        {
                            switch (data.GetType().GetProperty("Type").GetValue(data, null))
                            {
                                case "商品":
                                case "文章":
                                    var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                                    {
                                        Fk_Aid = (long)data.GetType().GetProperty("Id").GetValue(data, null),
                                        Type = (int)TagAssociateTypeEnum.目錄
                                    });
                                    var tag_text = "";
                                    foreach (var tagData in tagDatas) tag_text += tag_text == "" ? tagData.Tag_Name : $"、{tagData.Tag_Name}";
                                    data.GetType().GetProperty("Items").SetValue(data, tag_text == "" ? "無" : tag_text);
                                    break;
                                case "選單":
                                    var mid = data.GetType().GetProperty("FK_Mid").GetValue(data, null);
                                    MenuGetAllListDto webmenu_data;
                                    var webmenu = "";
                                    if (mid != null)
                                    {
                                        webmenu_data = await webMenuApplicationService.GetSelectData((long)mid);
                                        if (webmenu_data != null) webmenu = webmenu_data.Items == null ? webmenu_data.Title : $"{webmenu_data.Title}({webmenu_data.Items})";
                                    }
                                    data.GetType().GetProperty("Items").SetValue(data, webmenu);
                                    break;
                            }
                        }
                    }
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無目錄資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<DirectoryGetListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tagdeleteresponse = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var result = db.Directory.Where(e => e.Id == Id).FirstOrDefault();

                if (result != null)
                {
                    var tagids = await db.Tag_Associates.Where(e => e.FK_AId == Id && e.Type == (int)TagAssociateTypeEnum.目錄 && !e.IsDeleted).ToListAsync();

                    if (tagids != null)
                    {
                        foreach (var tagid in tagids)
                        {

                            tagdeleteresponse = await tagAppService.TagAssociateDelete(tagid.Id);
                        }
                    }

                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    result.DeleterUserId = usetId;
                    db.SaveChanges();

                    output.Success = tagdeleteresponse.Success;
                }
                else throw new Exception("查無目錄資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            return output;
        }
        public async Task<JsonResult> GetDirectoryArticlesList(long id, DataSourceLoadOptions loadOptions) {
            long WebsiteID = await loginUserData.GetWebsiteId();
            string error = string.Empty;
            try
            {
                var db_d = db.Directory.Where(e => e.Id == id && e.FK_WebsiteId == WebsiteID && !e.IsDeleted).FirstOrDefault();
                var DataIds = new List<long>();
                if (db_d != null)
                {
                    var d_tags = await db.Tag_Associates.Include(e => e.Tag)
                        .Where(e => e.FK_AId == id)
                        .Where(e => !e.IsDeleted)
                        .Where(e => e.Type == (int)TagAssociateTypeEnum.目錄)
                        .Where(e => e.Tag.FK_WebsiteId == WebsiteID)
                        .ToListAsync();
                    

                    if (d_tags != null)
                    {
                        var tlist = d_tags.Select(e => e.FK_TId).ToList();
                        var a_tags = await db.Tag_Associates.Include(e => e.Tag)
                            .Where(e => tlist.Contains(e.FK_TId))
                            .Where(e => !e.IsDeleted)
                            .Where(e => e.Type == (int)TagAssociateTypeEnum.文章)
                            .Where(e => e.Tag.FK_WebsiteId == WebsiteID)
                            .ToListAsync();
                        if (!a_tags.Any()) throw new Exception("資料不存在");
                        var aids = a_tags.Select(e => e.FK_AId).ToList();
                        var dataQuery = from p in db.Article.Where(e => !e.IsDeleted)
                                        where aids.Contains(p.Id)
                                        select new DirectoryReleInfoDto
                                        {
                                            Id = p.Id,
                                            Title = p.Title,
                                            Description = p.Description,
                                            SerNo = p.SerNO,
                                            NodeDate = p.NodeDate,
                                            LastModificationTime = p.LastModificationTime ?? p.CreationTime
                                        };
                        var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                        return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                    }
                    else new Exception("無綁定標籤");
                }throw new Exception("目錄不存在");
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            return new JsonResult(new { error }, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetDirectoryProductsList(long id, DataSourceLoadOptions loadOptions) {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var db_d = db.Directory.Where(e => e.Id == id && e.FK_WebsiteId == WebsiteID && !e.IsDeleted).FirstOrDefault();
                var DataIds = new List<long>();
                if (db_d != null)
                {
                    var tags = await db.Tag_Associates.Include(e => e.Tag)
                        .Where(e => e.FK_AId == id)
                        .Where(e => !e.IsDeleted)
                        .Where(e => e.Type == (int)TagAssociateTypeEnum.目錄)
                        .Where(e => e.Tag.FK_WebsiteId == WebsiteID)
                        .ToListAsync();

                    if (tags != null)
                    {
                        var dataQuery = from p in db.Prods.Where(e => !e.IsDeleted)
                                     join t in tags on p.Id equals t.FK_AId
                                     select new DirectoryReleInfoDto
                                     {
                                         Id = p.Id,
                                         Title = p.Title,
                                         Description = p.Description,
                                     };
                        var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                        return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                    }
                }
            }
            catch (Exception e)
            {

            }
            return new JsonResult(new List<DirectoryReleInfoDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetDirectoryMenusList(long id, DataSourceLoadOptions loadOptions) {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
            }
            catch (Exception e)
            {

            }
            return new JsonResult(new List<DirectoryGetListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
    }
}