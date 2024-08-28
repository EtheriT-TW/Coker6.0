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
using EtheriT.Coker.Application.Permissions;
using System.Collections.Generic;
using EtheriT.Coker.Application.Shared.Dto.Files;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared.Dto.Search;

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
        private readonly IPermissionsAppService permissionsAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly ICustSearchAppService custSearchAppService;
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
            IPermissionsAppService permissionsAppService,
            IFileUploadAppService fileUploadAppService,
            ICustSearchAppService custSearchAppService,
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
            this.permissionsAppService = permissionsAppService;
            this.fileUploadAppService = fileUploadAppService;
            this.custSearchAppService = custSearchAppService;
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
        private async Task<DirectoryReleInfoGetDto> TechCertReleInfo(DirectoryReleInfoInputDto dto)
        {
            var output = new DirectoryReleInfoGetDto { ReleInfos = new List<DirectoryReleInfoDto>() };
            long SearchId = dto.Ids.Count > 0 ? dto.Ids[0] : 0;
            Regex imgRegex = new Regex("(?:src=[\\S]*quot;)[\\S]*(?:quot;)", RegexOptions.IgnoreCase);
            int page = dto.Page ?? 0;
            if (SearchId == 0) return output;
            int shownum = dto.ShowNum ?? 0;
            if (shownum <= 0) shownum = 12;
            int skip = (page - 1) * shownum - 1;
            if (skip < 0) skip = 0;
            long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
            var dataQuery = db.Prod_TechCerts.Include(e => e.Prod).Include(e => e.TechnicalCertificate)
                .Where(e => !e.IsDeleted && !e.Prod.IsDeleted && !e.TechnicalCertificate.IsDeleted)
                .Where(e => e.FK_TCId == SearchId).Select(e => e.FK_PId);

            IQueryable<Prod>? prods = db.Prods.Include(e => e.Website)
                .Where(e => !e.IsDeleted).Where(e => !e.RemovedFromShelves)
                .Where(e => e.FK_WebsiteId == WebsiteID)
                .Where(e => dataQuery.Contains(e.Id));
            output.TotalCount = prods.Count();
            output.TotalPage = (int)Math.Ceiling(output.TotalCount / (double)shownum);
            var dataMargin = prods.OrderBy(e => e.Ser_No)
                       .ThenByDescending(e => e.Id)
                       .Skip(skip).Take(shownum);
            var list = await (from p in dataMargin
                              select new DirectoryReleInfoDto
                              {
                                  Id = p.Id,
                                  Title = p.Title,
                                  Link = $"/product/{p.Id}",
                                  OrgName = p.Website != null ? p.Website.OrgName : "",
                                  type = DirectoryTypeEnum.商品,
                                  SerNo = p.Ser_No,
                                  ItemNo = p.ItemNo,
                                  Description = p.Description,
                                  MainImage = p.Html,
                                  tags = (from t in db.Tags.Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == WebsiteID)
                                          join m in db.Tag_Associates.Where(e => !e.IsDeleted) on t.Id equals m.FK_TId
                                          where m.FK_AId == p.Id
                                          select new TagGetSelectedDto
                                          {
                                              Tag_Name = t.Title,
                                              FK_TId = p.Id,
                                          }).ToList(),
                              }).ToListAsync();
            for (int i = 0; i < list.Count; i++)
            {
                var data = list[i];
                var imgs = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
                {
                    Sid = data.Id,
                    Size = 1,
                    Type = (int)FileBindTypeEnum.產品
                });
                if (imgs.Any()) data.MainImage = imgs[0].Link;
                else data.MainImage = imgRegex.Match(data.MainImage ?? "").Value.Replace("quot;", "").Replace("src=&", "").Replace("&", "").Replace("amp;", "");
                var s = await db.Prod_Stocks.Where(e => e.FK_Pid == data.Id).Where(e => !e.IsDeleted).Select(e => e.Id).ToListAsync();
                var p = await db.Prod_Prices.Where(x => s.Contains(x.FK_PSId)).Where(e => !e.IsDeleted).ToListAsync();
                double min = p.Min(e => e.Price) ?? 0;
                double max = p.Max(e => e.Price) ?? 0;
                if (min == max) data.Price = $"{max}";
                else data.Price = $"{min} ~ {max}";
            }
            output.ReleInfos = list;

            return output;
        }
        private async Task<DirectoryReleInfoGetDto> SearchReleInfo(DirectoryReleInfoInputDto dto)
        {
            long SearchId = dto.Ids.Count > 0 ? dto.Ids[0] : 0;
            long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
            if (!string.IsNullOrEmpty(dto.SearchText)) await custSearchAppService.SaveSearchLog(new SaveSearchLogDto
            {
                Key = dto.SearchText,
                FK_CustSearchId = SearchId,
                FK_WebsiteId = dto.SiteId ?? 0
            });
            if (SearchId == 3) return await SearchProd(dto);
            var output = new DirectoryReleInfoGetDto { ReleInfos = new List<DirectoryReleInfoDto>() };
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
                            //.Where(e => e.RouterName.ToLower() != "home")
                            .Where(e => !string.IsNullOrEmpty(e.Html))
                            .Where(e =>
                                (e.Title ?? "").Contains(dto.SearchText ?? "") ||
                                (e.Html ?? "").Contains(dto.SearchText ?? "")
                            );
            var data2 = db.Article.Include(e => e.Website).Where(e => !e.IsDeleted)
                            .Where(e => e.FK_WebsiteId == WebsiteID)
                            .Where(e => !string.IsNullOrEmpty(e.Html))
                            .Where(e => e.Visible)
                            .Where(e =>
                                e.permanent ||
                                (e.StartTime.Value <= DateTime.Now && e.EndTime.Value >= DateTime.Now)
                            )
                            .Where(e =>
                                (e.Title ?? "").Contains(dto.SearchText ?? "") ||
                                (e.Description ?? "").Contains(dto.SearchText ?? "") ||
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
        private async Task<DirectoryReleInfoGetDto> SearchProd(DirectoryReleInfoInputDto dto)
        {
            var output = new DirectoryReleInfoGetDto { ReleInfos = new List<DirectoryReleInfoDto>() };
            if (string.IsNullOrEmpty(dto.SearchText)) return output;
            long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
            Regex imgRegex = new Regex("(?:src=[\\S]*quot;)[\\S]*(?:quot;)", RegexOptions.IgnoreCase);
            int page = dto.Page ?? 0;
            int shownum = dto.ShowNum ?? 0;
            if (shownum <= 0) shownum = 12;
            int skip = (page - 1) * shownum - 1;
            if (skip < 0) skip = 0;
            IQueryable<Prod>? prods = db.Prods.Include(e => e.Website)
                .Where(e => !e.IsDeleted).Where(e => !e.RemovedFromShelves)
                .Where(e => e.Visible)
                .Where(e => e.FK_WebsiteId == WebsiteID)
                .Where(e =>
                    e.Title.Contains(dto.SearchText ?? "") ||
                    e.Description.Contains(dto.SearchText ?? "") ||
                    (e.Html ?? "").Contains(dto.SearchText ?? "") ||
                    (e.ItemNo != null && e.ItemNo.Contains(dto.SearchText ?? "")) ||
                    db.Tag_Associates.Include(t => t.Tag)
                        .Where(t => t.Tag != null && t.Tag.FK_WebsiteId == WebsiteID && t.Type == (int)TagAssociateTypeEnum.商品 && !t.Tag.IsDeleted && !t.IsDeleted)
                        .Where(t => t.Tag != null && t.Tag.Title.Contains(dto.SearchText ?? ""))
                        .Select(t => t.FK_AId).Contains(e.Id) ||
                    db.Prod_TechCerts.Include(t => t.TechnicalCertificate)
                        .Where(t => !t.TechnicalCertificate.IsDeleted && t.TechnicalCertificate.FK_WebsiteId == WebsiteID)
                        .Where(t => !string.IsNullOrEmpty(t.TechnicalCertificate.Title) && t.TechnicalCertificate.Title.Contains(dto.SearchText ?? ""))
                        .Select(t => t.FK_PId).Contains(e.Id)
                );
            List<long> dirFilterIds = await prods.Select(e => e.Id).ToListAsync();
            if (dto.DirectoryType != 0)
            {
                prods = prods.Where(e =>
                    db.Tag_Associates.Include(t => t.Tag)
                        .Where(t => t.Tag != null && t.Tag.FK_WebsiteId == WebsiteID && t.Type == (int)TagAssociateTypeEnum.商品 && !t.Tag.IsDeleted && !t.IsDeleted)
                        .Where(d =>
                            db.Tag_Associates.Include(t => t.Tag)
                                .Where(t => t.Tag != null && t.Tag.FK_WebsiteId == WebsiteID && t.Type == (int)TagAssociateTypeEnum.目錄 && !t.Tag.IsDeleted && !t.IsDeleted)
                                .Where(t => t.FK_AId == dto.DirectoryType)
                                .Select(t => t.FK_TId).Contains(d.FK_TId)
                        )
                        .Select(t => t.FK_AId).Contains(e.Id)
                );
            }
            List<long> Ids = await prods.Select(e => e.Id).ToListAsync();
            dto.Filters.ForEach(t =>
            {
                t.Group.ForEach(g =>
                {
                    if (g.Tags.Any())
                    {
                        switch (t.Type)
                        {
                            case DirectorySearchTypeEnum.標籤:

                                if (g.Id != 0)
                                {
                                    var tid = from p in prods
                                              join ta in db.Tag_Associates.Include(e => e.Tag)
                                                              .Where(e => !e.IsDeleted && e.Type == (int)TagAssociateTypeEnum.商品 && !e.Tag.IsDeleted)
                                                      on p.Id equals ta.FK_AId
                                              join tg in db.Tag_TagGroups.Include(e => e.Tag_Group)
                                                              .Where(e => !e.IsDeleted && !e.Tag_Group.IsDeleted && e.Tag_Group.FK_WebsiteId == WebsiteID)
                                                      on ta.FK_TId equals tg.FK_TId
                                              where tg.FK_TGId == g.Id && g.Tags.Contains(ta.FK_TId)
                                              select p.Id;
                                    prods = prods.Where(e => tid.Contains(e.Id));
                                }
                                else
                                {
                                    var tid = from p in prods
                                              join ta in db.Tag_Associates.Include(e => e.Tag)
                                                              .Where(e => !e.IsDeleted && e.Type == (int)TagAssociateTypeEnum.商品 && !e.Tag.IsDeleted)
                                                      on p.Id equals ta.FK_AId
                                              where g.Tags.Contains(ta.FK_TId)
                                              select p.Id;
                                    prods = prods.Where(e => tid.Contains(e.Id));
                                }

                                break;
                            case DirectorySearchTypeEnum.技術文件:
                                var ptid = from p in prods
                                           join t in db.Prod_TechCerts.Include(e => e.TechnicalCertificate)
                                                             .Where(e => !e.IsDeleted && e.TechnicalCertificate.FK_WebsiteId == WebsiteID && !e.TechnicalCertificate.IsDeleted)
                                                 on p.Id equals t.FK_PId
                                           where g.Tags.Contains(t.FK_TCId)
                                           select p.Id;
                                prods = prods.Where(e => ptid.Contains(e.Id));
                                break;
                        }
                    }
                });
            });
            var tagbind = db.Tag_Associates.Include(e => e.Tag).Where(e => !e.IsDeleted)
                    .Where(e => e.Tag != null && e.Tag.FK_WebsiteId == WebsiteID && !e.Tag.IsDeleted)
                    .Where(e => Ids.Contains(e.FK_AId) && e.Type == (int)TagAssociateTypeEnum.商品);
            List<long> tagsId = tagbind.Select(e => e.FK_TId).ToList();
            var allGropTagsId = await db.Tag_TagGroups.Include(e => e.Tag).Where(e => !e.IsDeleted)
                .Where(e => e.Tag != null && !e.Tag.IsDeleted && e.Tag.FK_WebsiteId == WebsiteID)
                .Select(e => e.FK_TId)
                .ToListAsync();
            tagsId = tagsId.FindAll(e => !allGropTagsId.Contains(e));
            output.Filter.Add(new DirectorySearchTypeListDto
            {
                Id = 0,
                Type = DirectorySearchTypeEnum.標籤,
                Name = "其他",
                Tags = (from t in db.Tags.Where(e => !e.IsDeleted)
                        where t.FK_WebsiteId == WebsiteID && tagsId.Contains(t.Id)
                        select new TagGetSelectedDto
                        {
                            FK_TId = t.Id,
                            Tag_Name = t.Title,
                            count = (from b in tagbind.Where(e => e.FK_TId == t.Id) select b).Count()
                        }).ToList()
            });
            output.DirectoryType.AddRange(await (
                from dir in db.Directory.Where(e => !e.IsDeleted && e.FK_WebsiteId == WebsiteID)
                    .Where(d =>
                        db.Tag_Associates
                            .Where(a => !a.IsDeleted && a.Type == (int)TagAssociateTypeEnum.目錄 && a.FK_AId == d.Id)
                            .Where(a =>
                                db.Tag_Associates.Where(p => !p.IsDeleted && p.Type == (int)TagAssociateTypeEnum.商品 && dirFilterIds.Contains(p.FK_AId))
                                .Select(p => p.FK_TId).Contains(a.FK_TId)
                            )
                            .Select(a => a.FK_AId).Contains(d.Id)
                    )
                select new DirectoryListBySearchDto
                {
                    Id = dir.Id,
                    Name = dir.Title
                }
            ).ToListAsync());
            output.Filter.AddRange(await (from tg in db.Tag_Groups
                                          where !tg.IsDeleted && tg.FK_WebsiteId == WebsiteID
                                          select new DirectorySearchTypeListDto
                                          {
                                              Id = tg.Id,
                                              Type = DirectorySearchTypeEnum.標籤,
                                              Name = tg.Title,
                                              Tags = (from t in db.Tags.Where(e => !e.IsDeleted)
                                                      join m in db.Tag_TagGroups.Where(e => !e.IsDeleted) on t.Id equals m.FK_TId
                                                      where t.FK_WebsiteId == WebsiteID && m.FK_TGId == tg.Id
                                                      select new TagGetSelectedDto
                                                      {
                                                          FK_TId = t.Id,
                                                          Tag_Name = t.Title,
                                                          count = (from b in tagbind.Where(e => e.FK_TId == t.Id) select b).Count()
                                                      }).ToList()
                                          }).ToListAsync());
            var tec = db.Prod_TechCerts.Include(p => p.Prod).Include(t => t.TechnicalCertificate)
                    .Where(e => !e.IsDeleted && e.Prod.FK_WebsiteId == WebsiteID)
                    .Where(e => !e.Prod.IsDeleted && Ids.Contains(e.FK_PId))
                    .Where(e => !e.TechnicalCertificate.IsDeleted && e.TechnicalCertificate.FK_WebsiteId == WebsiteID);
            var tecIds = await tec.Select(e => e.FK_TCId).ToListAsync();
            output.Filter.Add(new DirectorySearchTypeListDto
            {
                Id = 0,
                Type = DirectorySearchTypeEnum.技術文件,
                Name = "技術文件",
                Tags = (
                            from t in db.TechnicalCertificates.Where(e => !e.IsDeleted && e.FK_WebsiteId == WebsiteID)
                            where tecIds.Contains(t.Id)
                            select new TagGetSelectedDto
                            {
                                FK_TId = t.Id,
                                Tag_Name = t.Title ?? "",
                                count = (from b in tec.Where(e => e.FK_TCId == t.Id) select b).Count()
                            }
                        ).ToList()
            }
                );
            output.TotalCount = prods.Count();
            output.TotalPage = (int)Math.Ceiling(output.TotalCount / (double)shownum);
            var dataMargin = prods.OrderBy(e => e.Ser_No)
                       .ThenByDescending(e => e.Id)
                       .Skip(skip).Take(shownum);
            var list = await (from p in dataMargin
                              select new DirectoryReleInfoDto
                              {
                                  Id = p.Id,
                                  Title = p.Title,
                                  Link = $"/product/{p.Id}",
                                  OrgName = p.Website != null ? p.Website.OrgName : "",
                                  type = DirectoryTypeEnum.商品,
                                  SerNo = p.Ser_No,
                                  ItemNo = p.ItemNo,
                                  Description = p.Description,
                                  MainImage = p.Html,
                                  Status = p.Status,
                                  StatusName = ((ProdStatusEnum)p.Status).ToString(),
                                  tags = (from t in db.Tags.Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == WebsiteID)
                                          join m in db.Tag_Associates.Where(e => !e.IsDeleted) on t.Id equals m.FK_TId
                                          where m.FK_AId == p.Id && m.Type == (int)TagAssociateTypeEnum.商品
                                          select new TagGetSelectedDto
                                          {
                                              Tag_Name = t.Title,
                                              FK_TId = p.Id,
                                          }).ToList(),
                              }).ToListAsync();
            for (int i = 0; i < list.Count; i++)
            {
                var data = list[i];
                var imgs = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
                {
                    Sid = data.Id,
                    Size = 1,
                    Type = (int)FileBindTypeEnum.產品
                });
                if (string.IsNullOrEmpty(data.Description)) getSearchDescription(data.MainImage, dto.SearchText);
                else getSearchDescription(data.Description, dto.SearchText);
                if (imgs.Any()) data.MainImage = imgs[0].Link;
                else data.MainImage = imgRegex.Match(data.MainImage ?? "").Value.Replace("quot;", "").Replace("src=&", "").Replace("&", "").Replace("amp;", "");


                var s = await db.Prod_Stocks.Where(e => e.FK_Pid == data.Id).Where(e => !e.IsDeleted).Select(e => e.Id).ToListAsync();
                var p = await db.Prod_Prices.Where(x => s.Contains(x.FK_PSId)).Where(e => !e.IsDeleted).ToListAsync();
                double min = p.Min(e => e.Price) ?? 0;
                double max = p.Max(e => e.Price) ?? 0;
                if (min == max) data.Price = $"{max}";
                else data.Price = $"{min} ~ {max}";
            }
            output.ReleInfos = list;
            return output;
        }
        private string getSearchDescription(string? conten, string findstr)
        {
            if (string.IsNullOrEmpty(conten)) return "";
            List<string> replaceRul = new List<string> {
                @"<span(.|\n)*?class=""material-symbols-outlined(.|\n)*?>(.|\n)*?/span>",
                @"<i(.|\n)*?class=""material-symbols-outlined(.|\n)*?>(.|\n)*?/i>",
                @"<(.|\n)*?>",
                @"\n"
            };
            string s = Regex.Replace(stringHandler.HtmlDecode(conten), @$"({String.Join("|", replaceRul.ToArray())})", "");
            int index = s.IndexOf(findstr) - 10;
            if (index < 0) index = 0;
            s = s.Substring(index);
            if (index != 0) s = $" ... {s}";
            if (string.IsNullOrEmpty(findstr)) return s;
            return s.Replace(findstr, $"<span class='bg-warning text-dark'>{findstr}</span>");
        }
        public async Task<DirectoryReleInfoGetDto> GetReleInfo(DirectoryReleInfoInputDto dto)
        {
            var corr = new List<CorrDTAID>();
            if ((dto.Type ?? "").ToLower() == "search") return await SearchReleInfo(dto);
            if ((dto.Type ?? "").ToLower() == "techcert") return await TechCertReleInfo(dto);
            var DataIds = new List<long>();
            long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
            List<long> siteIds = await db.MappingWebsiteRelationship.Where(e => e.FatherId == WebsiteID).Where(e => !e.IsDeleted).Select(e => e.Id).ToListAsync();
            siteIds.Add(WebsiteID);
            var output = new DirectoryReleInfoGetDto();
            var db_d = db.Directory.Where(e => dto.Ids.Contains(e.Id) && e.FK_WebsiteId == WebsiteID && !e.IsDeleted).ToList();
            db_d.ForEach(e =>
            {
                corr.Add(new CorrDTAID
                {
                    DirectoryId = e.Id,
                    DirectoryName = e.Title,
                });
            });

            if (db_d != null)
            {
                var tags = await db.Tag_Associates.Include(e => e.Tag)
                    .Where(e => dto.Ids.Contains(e.FK_AId))
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.Type == (int)TagAssociateTypeEnum.目錄)
                    .Where(e => siteIds.Contains(e.Tag.FK_WebsiteId))
                    .ToListAsync();
                
                tags.ForEach(t =>
                {
                    corr[corr.FindIndex(c => c.DirectoryId == t.FK_AId)].TagId = t.FK_TId;
                });
                
                var notTags = await db.Tag_Associates.Include(e => e.Tag)
                    .Where(e => dto.Ids.Contains(e.FK_AId))
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.Type == (int)TagAssociateTypeEnum.目錄拒絕)
                    .Where(e => siteIds.Contains(e.Tag.FK_WebsiteId))
                    .ToListAsync();
                var notTagIds = notTags.Select(e => e.FK_TId).ToList() ?? new List<long>();
                var FKTIds = tags.Select(e => e.FK_TId).ToList();
                if (tags != null)
                {
                    List<long> allIds = new List<long>();
                    switch ((DirectoryTypeEnum)db_d[0].Type)
                    {
                        case DirectoryTypeEnum.商品:
                            var pd_notId = await (db.Tag_Associates.Where(e => notTagIds.Contains(e.FK_TId) && e.Type == (int)TagAssociateTypeEnum.商品 && !e.IsDeleted)).Select(e => e.FK_AId).ToListAsync();
                            foreach (var id in FKTIds)
                            {
                                var pd_as = await (db.Tag_Associates.Where(e => e.FK_TId == id && !pd_notId.Contains(e.FK_AId) && e.Type == (int)TagAssociateTypeEnum.商品 && !e.IsDeleted)).ToListAsync();
                                if (!allIds.Any()) allIds = pd_as.Select(e => e.FK_AId).ToList();
                                else
                                {
                                    allIds = pd_as.Where(e => allIds.Contains(e.FK_AId)).Select(e => e.FK_AId).ToList();
                                }
                            }
                            if (allIds.Any())
                            {
                                DataIds = db.Prods
                                    .Where(e => allIds.Contains(e.Id))
                                    .Where(e => !e.IsDeleted)
                                    .Where(e => !e.RemovedFromShelves)
                                    .Where(e => e.Visible)
                                    .Where(e => siteIds.Contains(e.FK_WebsiteId))
                                    .Where(e => e.permanent || (DateTime.Now >= e.StartTime && DateTime.Now <= e.EndTime))
                                    .OrderBy(e => e.Ser_No).ThenByDescending(e => e.Status == 5).ThenBy(e => e.ItemNo).ThenBy(e => e.Title).ThenByDescending(e => e.Id)
                                    .Select(e => e.Id).ToList();
                            }
                            break;
                        case DirectoryTypeEnum.文章:

                            var db_as = await db.Tag_Associates
                                    .Where(e => !e.IsDeleted)
                                    .Where(e => e.Type == (int)TagAssociateTypeEnum.文章)
                                    .Where(e => FKTIds.Contains(e.FK_TId))
                                    .ToListAsync();
                            var tempcorr = new List<CorrDTAID>();
                            db_as.ForEach(a =>
                            {
                                var dindex = corr.FindIndex(c => c.TagId == a.FK_TId);
                                if(dindex!= -1)
                                {
                                    tempcorr.Add(new CorrDTAID
                                    {
                                        DirectoryId = corr[dindex].DirectoryId,
                                        DirectoryName = corr[dindex].DirectoryName,
                                        TagId = corr[dindex].TagId,
                                        ArticleId = a.FK_AId,
                                    });
                                }
                            });
                            corr = tempcorr;

                            if (db_as != null)
                            {
                                allIds = db_as.Select(e => e.FK_AId).ToList();
                                DataIds = db.Article
                                    .Where(e => allIds.Contains(e.Id) && !notTagIds.Contains(e.Id))
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
                    switch ((DirectoryTypeEnum)db_d[0].Type)
                    {
                        case DirectoryTypeEnum.商品:
                            var tempproddata = await productAppService.GetDirectoryReleInfo(new DirectoryReleInfoInputDto
                            {
                                Ids = DataIds.Skip((page - 1) * shownum).Take(shownum).ToList<long>(),
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
                                MaxLen = dto.MaxLen,
                                Target = dto.Target,
                                FindNearest = dto.FindNearest,
                                Longitude = dto.Longitude,
                                Latitude = dto.Latitude
                            });
                            if (temparticledata != null)
                            {
                                if (dto.FindNearest == true)
                                {
                                    foreach (var item in temparticledata)
                                    {
                                        var dindex = corr.FindIndex(c => c.ArticleId == item.Id);
                                        if(dindex != -1)
                                        {
                                            item.Dirname = corr[dindex].DirectoryName;
                                        }
                                    }
                                }
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
                WebsiteId = websiteid,
                showUnvisible = dto.showUnvisible
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
                bool isSuperUser = await permissionsAppService.IsPowerUserPermissions();
                IQueryable<Core.Models.Directory> result = db.Directory.Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == WebsiteID);
                if (!isSuperUser)
                {
                    var per = await db.PermissionDetail.Where(e => e.FK_WebsiteId == WebsiteID)
                        .Where(e => e.FK_UserId == UserID || (e.FK_RoleId != null && RoleIds.Contains(e.FK_RoleId.Value)))
                        .Where(e => e.Type == (int)PermissionDetailsTypeEnum.目錄)
                        .Where(e => e.IsGranted).Select(e => e.FK_TargetId).ToListAsync();
                    if (per != null && per.Any()) result = result.Where(e => per.Contains(e.Id));
                }
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
        public async Task<JsonResult> GetDirectoryArticlesList(long id, DataSourceLoadOptions loadOptions)
        {
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
                }
                throw new Exception("目錄不存在");
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            return new JsonResult(new { error }, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetDirectoryProductsList(long id, DataSourceLoadOptions loadOptions)
        {
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
        public async Task<JsonResult> GetDirectoryMenusList(long id, DataSourceLoadOptions loadOptions)
        {
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