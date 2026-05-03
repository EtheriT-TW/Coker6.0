using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Permissions;
using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        private readonly ITokenAppService tokenAppService;
        private readonly IWebMenuApplication webMenuApplicationService;
        private readonly IPermissionsAppService permissionsAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly ICustSearchAppService custSearchAppService;
        private readonly StringHandler stringHandler;
        private readonly IConfiguration configuration;
        private readonly IHtmlProcessor htmlProcessor;
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
            ITokenAppService tokenAppService,
            IConfiguration configuration,
            IHtmlProcessor htmlProcessor
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
            this.tokenAppService = tokenAppService;
            this.configuration = configuration;
            this.htmlProcessor = htmlProcessor;
        }
        public async Task<ResponseMessageDto> AddUp(DirectoryAddUpDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tag_response = new ResponseMessageDto() { Success = true };
            ResponseMessageDto ad_response = new ResponseMessageDto() { Success = true };

            try
            {
                long userid = await loginUserData.GetUserId();
                long WebsiteID = 0;
                if (userid != 0) { WebsiteID = await loginUserData.GetWebsiteId(); }
                var asoid = dto.Id;

                if (dto.Id == 0)
                {
                    if (userid != 0)
                    {
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

                if (asoid != null && (dto.Type != (int)DirectoryTypeEnum.選單))
                {
                    var oldtaglist = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                    {
                        Fk_Aid = (long)asoid,
                        Type = TagAssociateTypeEnum.目錄,
                    });
                    var tagitem = new List<TagAssociateDto>();
                    var newtagid = new List<long>();
                    foreach (var data in dto.TagSelected)
                    {
                        var temp = oldtaglist.FindAll(o => o.FK_TId == data.FK_TId);
                        if (temp.Count == 0) { newtagid.Add(data.FK_TId); }
                        tagitem.Add(new TagAssociateDto()
                        {
                            Id = data.Id,
                            FK_AId = (long)asoid,
                            FK_TId = data.FK_TId,
                            Type = TagAssociateTypeEnum.目錄,
                            IsDeleted = data.IsDeleted
                        });
                    }
                    tag_response = await tagAppService.TagAssociateAddDelect(tagitem);
                    if (tag_response.Success & newtagid.Count > 0)
                    {
                        if (oldtaglist != null)
                        {
                            var adids = await GetReleAdId(oldtaglist.Select(e => e.FK_TId).ToList());
                            var adlist = await (from a in db.Advertise.Where(e => !e.IsDeleted)
                                                where adids.Contains(a.Id)
                                                select new DirectoryReleInfoDto
                                                {
                                                    Id = a.Id,
                                                    Title = a.Title,
                                                    Description = a.Describe,
                                                    StartTime = a.StartDate,
                                                    EndTime = a.EndDate,
                                                    SerNo = a.SerNO,
                                                    Visible = a.Visible,
                                                    ClickTimes = a.Clicks,
                                                    ExposureTimes = a.Exposure,
                                                    LastModificationTime = a.LastModificationTime ?? a.CreationTime
                                                }).ToListAsync();
                            if (adlist.Count > 0)
                            {
                                var adtagitem = new List<TagAssociateDto>();
                                foreach (var newtag in newtagid)
                                {
                                    foreach (var ad in adlist)
                                    {
                                        adtagitem.Add(new TagAssociateDto()
                                        {
                                            Id = ad.Id,
                                            FK_AId = (long)ad.Id,
                                            FK_TId = newtag,
                                            Type = TagAssociateTypeEnum.廣告,
                                            IsDeleted = false,
                                        });
                                    }
                                }
                                ad_response = await tagAppService.TagAssociateAddDelect(adtagitem);
                            }
                        }
                    }
                }
                output.Success = tag_response.Success & ad_response.Success;
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
                                            SortBy = e.SortBy,
                                            FK_MId = e.FK_Mid
                                        }).FirstOrDefaultAsync();

                    var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                    {
                        Fk_Aid = output.Id,
                        Type = TagAssociateTypeEnum.目錄,
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
            int skip = shownum == -500 ? 0 : (page - 1) * shownum - 1;
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
            var dataMargin = prods
                       .OrderBy(e => e.Ser_No).ThenByDescending(e => e.Status == ProdStatusEnum.新品).ThenBy(e => e.ItemNo).ThenBy(e => e.Title).ThenByDescending(e => e.Id)
                       .ThenByDescending(e => e.Id)
                       .Skip(skip).Take(shownum);
            var pageProdIds = await dataMargin
                .Select(p => p.Id)
                .ToListAsync();

            var list = await productAppService.GetDirectoryReleInfo(new DirectoryReleInfoInputDto
            {
                Ids = pageProdIds,
                SiteId = WebsiteID
            }) ?? new List<DirectoryReleInfoDto>();

            var map = list.ToDictionary(x => x.Id);

            output.ReleInfos = pageProdIds
                .Where(id => map.ContainsKey(id))
                .Select(id => map[id])
                .ToList();

            return output;
        }
        #region ===== Search =====
        // ✅ 介面不變：SearchReleInfo(DirectoryReleInfoInputDto dto)
        // ✅ SearchProd 不再需要（可刪除/覆蓋）
        // ✅ 只有需要的 Query 才會被建立（依 SearchId）
        // ✅ 修正 OrderBy / ThenBy、skip 計算
        // ✅ 共用：搜尋條件、Filters/DirectoryType 產生、使用者加權排序（無行為資料就回到預設）
        // ✅ 文章：只有「標籤」Filter；選單：無 Filter；商品：標籤 + 技術文件 + DirectoryType
        // ✅ 商品排序規則（Status / Ser_No / ItemNo / Id）不變，只修正 ThenBy 串接與 EF 可翻譯性

        private async Task<DirectoryReleInfoGetDto> SearchReleInfo(DirectoryReleInfoInputDto dto)
        {
            // ---------- 基本參數 ----------
            long searchId = (dto.Ids != null && dto.Ids.Count > 0) ? dto.Ids[0] : 0;
            long websiteId = (dto.SiteId == null || dto.SiteId == 0) ? await loginUserData.GetWebsiteId() : dto.SiteId.Value;

            var output = new DirectoryReleInfoGetDto
            {
                ReleInfos = new List<DirectoryReleInfoDto>(),
                DirectoryType = new List<DirectoryListBySearchDto>(),
                Filter = new List<DirectorySearchTypeListDto>()
            };

            // 沒有 keyword 就直接回空（沿用你原本語意）
            if (string.IsNullOrWhiteSpace(dto.SearchText))
            {
                output.TotalCount = 0;
                output.TotalPage = 0;
                return output;
            }

            // 記錄搜尋 log（沿用你原本語意）
            await custSearchAppService.SaveSearchLog(new SaveSearchLogDto
            {
                Key = dto.SearchText,
                FK_CustSearchId = searchId,
                FK_WebsiteId = dto.SiteId ?? 0
            });

            // 分頁修正（原本 -1 會造成跳筆）
            int page = (dto.Page.HasValue && dto.Page.Value > 0) ? dto.Page.Value : 1;
            int showNum = (dto.ShowNum.HasValue ? dto.ShowNum.Value : 12);
            if (showNum <= 0 && showNum != -500) showNum = 12;

            int skip = (showNum == -500) ? 0 : (page - 1) * showNum;

            // 圖片 fallback（沿用你原本 regex）
            Regex imgRegex = new Regex("(?:src=[\\S]*quot;)[\\S]*(?:quot;)", RegexOptions.IgnoreCase);

            // ---------- 決定本次要查哪些類型（依 SearchId） ----------
            bool includeMenu = false;
            bool includeArticle = false;
            bool includeProd = false;

            switch (searchId)
            {
                case 3:
                    includeProd = true;
                    break;

                case 1:
                case 2:
                    includeArticle = true;
                    break;

                default: // 0 或其他：預設搜尋選單 + 文章
                    includeMenu = true;
                    includeArticle = true;
                    break;
            }
            bool hasAnyFilter = HasAnyEffectiveFilter(dto);

            if (hasAnyFilter || (searchId == 0 && dto.DirectoryType > 0))
            {
                includeMenu = false;
            }

            // ---------- 取得使用者加權資料（可為空） ----------
            var userCtx = await GetUserSearchContextAsync();

            // ---------- 建立 Query（只建需要的） ----------
            IQueryable<WebMenu>? menuQ = null;
            IQueryable<Core.Models.Article>? articleQ = null;
            IQueryable<Prod>? prodQ = null;

            if (includeMenu)
            {
                // ✅ base query 不 Include(Website)（Count/Union 更輕，回查詳細資料仍 Include）
                menuQ = db.WebMenus
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Where(x => x.FK_WebsiteId == websiteId)
                    .Where(x => x.Visible)
                    .Where(x => !string.IsNullOrEmpty(x.RouterName))
                    .Where(x => !string.IsNullOrEmpty(x.Html))
                    .Where(x =>
                        (x.Title ?? "").Contains(dto.SearchText!) ||
                        (x.Html ?? "").Contains(dto.SearchText!)
                    );
            }

            if (includeArticle)
            {
                // ✅ base query 不 Include(Website)（Count/Union 更輕，回查詳細資料仍 Include）
                articleQ = db.Article
                    .AsNoTracking()
                    //.Include(x => x.Website) // ❌ 拿掉
                    .Where(x => !x.IsDeleted)
                    .Where(x => x.FK_WebsiteId == websiteId)
                    .Where(x => x.Visible)
                    .Where(x => !string.IsNullOrEmpty(x.Html))
                    .Where(x =>
                        x.permanent ||
                        (
                            x.StartTime.HasValue && x.EndTime.HasValue &&
                            x.StartTime.Value <= DateTime.Now &&
                            x.EndTime.Value >= DateTime.Now
                        )
                    )
                    .Where(x =>
                        (x.Title ?? "").Contains(dto.SearchText!) ||
                        (x.Description ?? "").Contains(dto.SearchText!) ||
                        (x.Html ?? "").Contains(dto.SearchText!) ||
                        db.Tag_Associates
                            .AsNoTracking()
                            //.Include(ta => ta.Tag) // ❌ 拿掉（Any/Where 不需要 Include）
                            .Where(ta => !ta.IsDeleted)
                            .Where(ta => ta.Type == TagAssociateTypeEnum.文章 && ta.FK_AId == x.Id)
                            .Where(ta => ta.Tag != null && !ta.Tag.IsDeleted && ta.Tag.FK_WebsiteId == websiteId)
                            .Where(ta => (ta.Tag!.Title ?? "").Contains(dto.SearchText!))
                            .Any()
                    );

                // SearchId 1/2：固定 tag 條件（你原本是 hardcode tagId 42/46）
                if (searchId == 1) articleQ = ApplyArticleFixedTag(articleQ, websiteId, 42);
                if (searchId == 2) articleQ = ApplyArticleFixedTag(articleQ, websiteId, 46);

                // 額外：日期範圍（若你要用 dto.StartDate/EndDate 過濾文章 NodeDate）
                if (dto.StartDate.HasValue)
                    articleQ = articleQ.Where(a => a.NodeDate.HasValue && a.NodeDate.Value >= dto.StartDate.Value.Date);
                if (dto.EndDate.HasValue)
                    articleQ = articleQ.Where(a => a.NodeDate.HasValue && a.NodeDate.Value <= dto.EndDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            if (includeProd)
            {
                prodQ = BuildProdBaseSearchQuery(dto, websiteId);

                // 額外：日期範圍（商品用 StartTime/EndTime 或你要的邏輯，這裡用你 dto 欄位）
                if (dto.StartDate.HasValue)
                    prodQ = prodQ.Where(p => p.StartTime.HasValue && p.StartTime.Value >= dto.StartDate.Value.Date);
                if (dto.EndDate.HasValue)
                    prodQ = prodQ.Where(p => p.EndTime.HasValue && p.EndTime.Value <= dto.EndDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            // ---------- 先產生 Filters / DirectoryType（依類型） ----------
            if (includeProd && prodQ != null)
            {
                await FillProdFiltersAndDirectoryTypeAsync(output, dto, websiteId, prodQ);
                // 把 dto.Filters / dto.DirectoryType 套到 prodQ（真正過濾結果）
                prodQ = ApplyProdFilters(prodQ, dto, websiteId);
            }
            else if (includeArticle && articleQ != null)
            {
                await FillArticleTagFiltersAsync(output, websiteId, articleQ);
                // 文章只支援「標籤」filters（你說的 C）
                articleQ = ApplyArticleFilters(articleQ, dto, websiteId);
            }
            // menu 沒有 filters（你說的 D）

            // ---------- ✅ 統一計算總筆數：改成 union Count 一次 ----------
            var unionForCount = BuildUnionQuery(menuQ, articleQ, prodQ, websiteId, userCtx, dto);
            int totalCount = await unionForCount.CountAsync();

            output.TotalCount = totalCount;

            if (showNum == -500) showNum = totalCount;
            output.TotalPage = (showNum <= 0) ? 0 : (int)Math.Ceiling(totalCount / (double)showNum);

            // ---------- 沒資料直接回 ----------
            if (totalCount <= 0)
                return output;

            // ---------- 建立「統一排序用」的 Union Row ----------
            var union = BuildUnionQuery(menuQ, articleQ, prodQ, websiteId, userCtx, dto);

            // 只取當頁 ids（穩定排序：最後一定要有 TieBreaker）
            var pageRows = await union
                .Skip(skip)
                .Take(showNum)
                .ToListAsync();

            // ---------- 回查詳細資料並組 DTO ----------
            var menuIds = pageRows.Where(r => r.Type == DirectoryTypeEnum.選單).Select(r => r.Id).ToList();
            var articleIds = pageRows.Where(r => r.Type == DirectoryTypeEnum.文章).Select(r => r.Id).ToList();
            var prodIds = pageRows.Where(r => r.Type == DirectoryTypeEnum.商品).Select(r => r.Id).ToList();

            var menus = (menuIds.Count == 0) ? new List<WebMenu>() :
                await db.WebMenus.AsNoTracking().Include(x => x.Website)
                    .Where(x => menuIds.Contains(x.Id))
                    .ToListAsync();

            var articles = (articleIds.Count == 0) ? new List<Core.Models.Article>() :
                await db.Article.AsNoTracking().Include(x => x.Website)
                    .Where(x => articleIds.Contains(x.Id))
                    .ToListAsync();

            var prodDirectoryItems = (prodIds.Count == 0)
                ? new List<DirectoryReleInfoDto>()
                : await productAppService.GetDirectoryReleInfo(new DirectoryReleInfoInputDto
                {
                    Ids = prodIds,
                    SiteId = websiteId
                }) ?? new List<DirectoryReleInfoDto>();

            var prodDirectoryMap = prodDirectoryItems.ToDictionary(x => x.Id);

            // 文章圖片批次（沿用你原本）
            var articleImages = (articleIds.Count == 0) ? new List<FileGetImgDto>() :
                await fileUploadAppService.getImgsFiles(new FileGetImgsInputDto
                {
                    Sid = articleIds,
                    Type = (int)FileBindTypeEnum.文章管理,
                    Size = 3
                });

            // ✅ 商品主圖改成批次：避免每筆商品 await getImgFiles（N+1）
            var prodMainImageMap = new Dictionary<long, string>();
            if (prodIds.Count > 0)
            {
                var prodImgs = await fileUploadAppService.getImgsFiles(new FileGetImgsInputDto
                {
                    Sid = prodIds,
                    Type = (int)FileBindTypeEnum.產品,
                    Size = 1
                });

                // 每個 Sid 取第一張（getImgsFiles 本身已做 Sid order + serno order，但這裡再保險）
                prodMainImageMap = prodImgs
                    .Where(x => x.Sid > 0 && !string.IsNullOrEmpty(x.Link))
                    .GroupBy(x => x.Sid)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Id).Select(x => x.Link).First());
            }

            // 標籤：文章/商品（只對當頁 ids）
            var articleTags = await GetTagsForObjectsAsync(websiteId, TagAssociateTypeEnum.文章, articleIds);
            var prodTags = await GetTagsForObjectsAsync(websiteId, TagAssociateTypeEnum.商品, prodIds);

            // ✅ 用 Dictionary 取代 FirstOrDefault（避免 O(n^2)）
            var menuMap = menus.ToDictionary(x => x.Id);
            var articleMap = articles.ToDictionary(x => x.Id);

            // 組裝輸出（依 pageRows 原順序）
            foreach (var row in pageRows)
            {
                if (row.Type == DirectoryTypeEnum.選單)
                {
                    if (!menuMap.TryGetValue(row.Id, out var m)) continue;

                    output.ReleInfos.Add(new DirectoryReleInfoDto
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Link = "/" + Convert.ToString(m.RouterName),
                        OrgName = m.Website?.OrgName ?? "",
                        type = DirectoryTypeEnum.選單,
                        SerNo = m.SerNO,
                        Description = string.IsNullOrEmpty(m.Description)
                            ? getSearchDescription(m.Html, dto.SearchText)
                            : getSearchDescription(m.Description, dto.SearchText),
                        MainImage = imgRegex.Match(m.Html ?? "").Value.Replace("quot;", "").Replace("src=&", "").Replace("&", "").Replace("amp;", "")
                    });
                }
                else if (row.Type == DirectoryTypeEnum.文章)
                {
                    if (!articleMap.TryGetValue(row.Id, out var a)) continue;

                    var imgs = articleImages.Where(i => i.Sid == a.Id).ToList();
                    output.ReleInfos.Add(new DirectoryReleInfoDto
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Link = "/article/" + a.Id,
                        OrgName = a.Website?.OrgName ?? "",
                        type = DirectoryTypeEnum.文章,
                        SerNo = a.SerNO,
                        NodeDate = a.NodeDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Description = string.IsNullOrEmpty(a.Description)
                            ? getSearchDescription(a.Html, dto.SearchText)
                            : getSearchDescription(a.Description, dto.SearchText),
                        MainImage = (imgs.Count > 0)
                            ? imgs[0].Link
                            : imgRegex.Match(a.Html ?? "").Value.Replace("quot;", "").Replace("src=&", "").Replace("&", "").Replace("amp;", ""),
                        tags = articleTags.TryGetValue(a.Id, out var t) ? t : null
                    });
                }
                else if (row.Type == DirectoryTypeEnum.商品)
                {
                    if (!prodDirectoryMap.TryGetValue(row.Id, out var dtoItem)) continue;

                    var filtersStr = "";
                    if (dto.Filters != null && dto.Filters.Count > 0)
                    {
                        var json = JsonConvert.SerializeObject(dto.Filters);
                        if (!string.IsNullOrWhiteSpace(json))
                            filtersStr = "?filter=" + json;
                    }

                    dtoItem.Link = $"/product/{dtoItem.Id}{filtersStr}";

                    output.ReleInfos.Add(dtoItem);
                }
            }

            return output;

            // ====================== local helpers ======================

            IQueryable<Core.Models.Article> ApplyArticleFixedTag(IQueryable<Core.Models.Article> q, long wId, long fixedTagId)
            {
                // 你原本是 join tag_associate / tags；這裡用 Any 讓 SQL 比較乾淨
                return q.Where(a =>
                    db.Tag_Associates.AsNoTracking()
                        .Where(ta => !ta.IsDeleted && ta.Type == TagAssociateTypeEnum.文章 && ta.FK_AId == a.Id)
                        .Join(db.Tags.AsNoTracking().Where(t => !t.IsDeleted && t.FK_WebsiteId == wId),
                              ta => ta.FK_TId,
                              t => t.Id,
                              (ta, t) => t.Id)
                        .Any(tid => tid == fixedTagId)
                );
            }
            // ---------- local helpers（放在 SearchReleInfo 內） ----------
            static bool HasAnyEffectiveFilter(DirectoryReleInfoInputDto dto)
            {
                if (dto.DirectoryType > 0) return true;

                if (dto.Filters == null || dto.Filters.Count == 0) return false;

                // 你目前 filters 結構：Filter -> Group -> Tags
                foreach (var f in dto.Filters)
                {
                    if (f?.Group == null) continue;
                    foreach (var g in f.Group)
                    {
                        if (g?.Tags != null && g.Tags.Count > 0)
                            return true;
                    }
                }
                return false;
            }
        }

        private sealed class UserSearchContext
        {
            public Guid? UUID { get; set; }
            public List<long> RelevantTagIds { get; set; } = new();
        }

        private async Task<UserSearchContext> GetUserSearchContextAsync()
        {
            var ctx = new UserSearchContext();

            Guid uuid;
            try
            {
                uuid = await tokenAppService.GetUUID();
                ctx.UUID = uuid;
            }
            catch
            {
                // 沒 token 就回空（走預設排序）
                return ctx;
            }

            // 分群（優先）
            var grouping = await db.UserGroupingDetails
                .AsNoTracking()
                .Include(e => e.userGrouping)
                .FirstOrDefaultAsync(e => e.UUID == uuid);

            if (grouping != null)
            {
                ctx.RelevantTagIds = await db.Tag_Associates.AsNoTracking()
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.Type == TagAssociateTypeEnum.使用者分群 && e.FK_AId == grouping.FK_GropingId)
                    .Select(e => e.FK_TId)
                    .ToListAsync();

                if (ctx.RelevantTagIds.Count > 0) return ctx;
            }

            // 個人標籤（前 5）
            ctx.RelevantTagIds = await db.UserTagStatistics
                .AsNoTracking()
                .Where(ut => ut.UUID == uuid)
                .OrderByDescending(ut => ut.Weight)
                .Take(5)
                .Select(ut => ut.FK_TagId)
                .ToListAsync();

            return ctx;
        }

        private IQueryable<Prod> BuildProdBaseSearchQuery(DirectoryReleInfoInputDto dto, long websiteId)
        {
            // ✅ 只做 keyword 搜尋，不做 filters（filters 在 ApplyProdFilters）
            var q = db.Prods
                .AsNoTracking()
                .Include(e => e.Website)
                .Where(e => !e.IsDeleted)
                .Where(e => !e.RemovedFromShelves)
                .Where(e => e.Visible)
                .Where(e => e.FK_WebsiteId == websiteId);

            var kw = dto.SearchText ?? "";
            q = q.Where(e =>
                e.Title.Contains(kw) ||
                e.Introduction.Contains(kw) ||
                e.Description.Contains(kw) ||
                (e.Html ?? "").Contains(kw) ||
                (e.ItemNo != null && e.ItemNo.Contains(kw)) ||
                db.Tag_Associates.AsNoTracking()
                    .Include(t => t.Tag)
                    .Where(t => !t.IsDeleted && t.Type == TagAssociateTypeEnum.商品)
                    .Where(t => t.Tag != null && !t.Tag.IsDeleted && t.Tag.FK_WebsiteId == websiteId)
                    .Where(t => (t.Tag!.Title ?? "").Contains(kw))
                    .Select(t => t.FK_AId).Contains(e.Id) ||
                db.Prod_TechCerts.AsNoTracking()
                    .Include(t => t.TechnicalCertificate)
                    .Where(t => !t.IsDeleted)
                    .Where(t => t.TechnicalCertificate != null && !t.TechnicalCertificate.IsDeleted && t.TechnicalCertificate.FK_WebsiteId == websiteId)
                    .Where(t => !string.IsNullOrEmpty(t.TechnicalCertificate!.Title) && t.TechnicalCertificate!.Title.Contains(kw))
                    .Select(t => t.FK_PId).Contains(e.Id)
            );

            return q;
        }

        private IQueryable<Prod> ApplyProdFilters(IQueryable<Prod> prods, DirectoryReleInfoInputDto dto, long websiteId)
        {
            // ✅ DirectoryType（你目前用 long；0 = 不限制）
            // 你說不能只用 !=0 判斷語意，但現階段 input 就是 0/目錄id，我們仍採 0=all
            if (dto.DirectoryType > 0)
            {
                var dirId = dto.DirectoryType;

                // 目錄 tag -> 商品 tag -> 商品
                prods = prods.Where(p =>
                    db.Tag_Associates.AsNoTracking()
                        .Where(a => !a.IsDeleted && a.Type == TagAssociateTypeEnum.商品)
                        .Where(a => a.Tag != null && !a.Tag.IsDeleted && a.Tag.FK_WebsiteId == websiteId)
                        .Where(prodTag =>
                            db.Tag_Associates.AsNoTracking()
                                .Where(d => !d.IsDeleted && d.Type == TagAssociateTypeEnum.目錄 && d.FK_AId == dirId)
                                .Select(d => d.FK_TId)
                                .Contains(prodTag.FK_TId)
                        )
                        .Select(a => a.FK_AId)
                        .Contains(p.Id)
                );
            }

            if (dto.Filters == null || dto.Filters.Count == 0)
                return prods;

            foreach (var f in dto.Filters)
            {
                foreach (var g in f.Group ?? new List<DirectoryGroupFilterDto>())
                {
                    if (g.Tags == null || g.Tags.Count == 0) continue;

                    if (f.Type == DirectorySearchTypeEnum.標籤)
                    {
                        // groupId != 0：限制在某 TagGroup
                        if (g.Id != 0)
                        {
                            var ids =
                                from p in prods
                                join ta in db.Tag_Associates.AsNoTracking()
                                        .Include(e => e.Tag)
                                        .Where(e => !e.IsDeleted && e.Type == TagAssociateTypeEnum.商品 && e.Tag != null && !e.Tag.IsDeleted)
                                    on p.Id equals ta.FK_AId
                                join tg in db.Tag_TagGroups.AsNoTracking()
                                        .Include(e => e.Tag_Group)
                                        .Where(e => !e.IsDeleted && e.Tag_Group != null && !e.Tag_Group.IsDeleted && e.Tag_Group.FK_WebsiteId == websiteId)
                                    on ta.FK_TId equals tg.FK_TId
                                where tg.FK_TGId == g.Id && g.Tags.Contains(ta.FK_TId)
                                select p.Id;

                            prods = prods.Where(p => ids.Contains(p.Id));
                        }
                        else
                        {
                            var ids =
                                from p in prods
                                join ta in db.Tag_Associates.AsNoTracking()
                                        .Include(e => e.Tag)
                                        .Where(e => !e.IsDeleted && e.Type == TagAssociateTypeEnum.商品 && e.Tag != null && !e.Tag.IsDeleted)
                                    on p.Id equals ta.FK_AId
                                where g.Tags.Contains(ta.FK_TId)
                                select p.Id;

                            prods = prods.Where(p => ids.Contains(p.Id));
                        }
                    }
                    else if (f.Type == DirectorySearchTypeEnum.技術文件)
                    {
                        var ids =
                            from p in prods
                            join t in db.Prod_TechCerts.AsNoTracking()
                                    .Include(e => e.TechnicalCertificate)
                                    .Where(e => !e.IsDeleted && e.TechnicalCertificate != null && !e.TechnicalCertificate.IsDeleted && e.TechnicalCertificate.FK_WebsiteId == websiteId)
                                on p.Id equals t.FK_PId
                            where g.Tags.Contains(t.FK_TCId)
                            select p.Id;

                        prods = prods.Where(p => ids.Contains(p.Id));
                    }
                }
            }

            return prods;
        }

        IQueryable<Core.Models.Article> ApplyArticleFilters(
            IQueryable<Core.Models.Article> articles,
            DirectoryReleInfoInputDto dto,
            long websiteId)
        {
            // ✅ DirectoryType（文章也要支援，語意與商品一致：目錄 tag -> 文章 tag -> 文章）
            if (dto.DirectoryType > 0)
            {
                var dirId = dto.DirectoryType;

                articles = articles.Where(a =>
                    db.Tag_Associates.AsNoTracking()
                        .Where(x => !x.IsDeleted && x.Type == TagAssociateTypeEnum.文章)
                        .Where(x => x.Tag != null && !x.Tag.IsDeleted && x.Tag.FK_WebsiteId == websiteId)
                        .Where(artTag =>
                            db.Tag_Associates.AsNoTracking()
                                .Where(d => !d.IsDeleted && d.Type == TagAssociateTypeEnum.目錄 && d.FK_AId == dirId)
                                .Select(d => d.FK_TId)
                                .Contains(artTag.FK_TId)
                        )
                        .Select(x => x.FK_AId)
                        .Contains(a.Id)
                );
            }

            // 文章只支援標籤 Filter（你原本的邏輯保留）
            if (dto.Filters == null || dto.Filters.Count == 0) return articles;

            var tagFilter = dto.Filters.FirstOrDefault(x => x.Type == DirectorySearchTypeEnum.標籤);
            if (tagFilter == null) return articles;

            var tagIds = tagFilter.Group
                .SelectMany(g => g.Tags ?? new List<long>())
                .Distinct()
                .ToList();

            if (tagIds.Count == 0) return articles;

            articles = articles.Where(a =>
                db.Tag_Associates.AsNoTracking()
                    .Where(ta => !ta.IsDeleted && ta.Type == TagAssociateTypeEnum.文章 && ta.FK_AId == a.Id)
                    .Where(ta => ta.Tag != null && !ta.Tag.IsDeleted && ta.Tag.FK_WebsiteId == websiteId)
                    .Where(ta => tagIds.Contains(ta.FK_TId))
                    .Any()
            );

            return articles;
        }

        private IQueryable<UnionRow> BuildUnionQuery(
            IQueryable<WebMenu>? menuQ,
            IQueryable<Core.Models.Article>? articleQ,
            IQueryable<Prod>? prodQ,
            long websiteId,
            UserSearchContext? userCtx,
            DirectoryReleInfoInputDto dto)
        {
            IReadOnlyList<long> relevantTagIds = userCtx != null ? userCtx.RelevantTagIds : new List<long>();

            IQueryable<UnionRow>? union = null;

            // 1) WebMenu
            if (menuQ != null)
            {
                var q =
                    from m in menuQ
                    select new UnionRow
                    {
                        Id = m.Id,
                        Type = DirectoryTypeEnum.選單,
                        NodeDate = null,
                        SerNo = m.SerNO,
                        ItemNo = null,
                        MatchCount = 0,

                        SortType = 1,
                        SortSoldOut = 0,
                        SortDiscontinued = 0,
                    };

                union = union == null ? q : union.Union(q);
            }

            // 2) Article
            if (articleQ != null)
            {
                var q =
                    from a in articleQ
                    select new UnionRow
                    {
                        Id = a.Id,
                        Type = DirectoryTypeEnum.文章,
                        NodeDate = a.NodeDate,
                        SerNo = a.SerNO,
                        ItemNo = null,

                        MatchCount = (relevantTagIds.Count == 0)
                            ? 0
                            : db.Tag_Associates
                                .Where(t => !t.IsDeleted
                                            && t.Type == TagAssociateTypeEnum.文章
                                            && t.FK_AId == a.Id
                                            && relevantTagIds.Contains(t.FK_TId))
                                .Count(),

                        SortType = 1,
                        SortSoldOut = 0,
                        SortDiscontinued = 0,
                    };

                union = union == null ? q : union.Union(q);
            }

            // 3) Product
            if (prodQ != null)
            {
                var q =
                    from p in prodQ
                    select new UnionRow
                    {
                        Id = p.Id,
                        Type = DirectoryTypeEnum.商品,
                        NodeDate = null,
                        SerNo = p.Ser_No,
                        ItemNo = p.ItemNo,

                        MatchCount = (relevantTagIds.Count == 0)
                            ? 0
                            : db.Tag_Associates
                                .Where(t => !t.IsDeleted
                                            && t.Type == TagAssociateTypeEnum.商品
                                            && t.FK_AId == p.Id
                                            && relevantTagIds.Contains(t.FK_TId))
                                .Count(),

                        // ✅ 商品優先 + 商品狀態排序鍵（投影成欄位）
                        SortType = 0,
                        SortSoldOut = (p.Status == ProdStatusEnum.售完) ? 1 : 0,
                        SortDiscontinued = (p.Status == ProdStatusEnum.停產) ? 1 : 0,
                    };

                union = union == null ? q : union.Union(q);
            }

            // 回傳空集合（不依賴 DbSet<UnionRow>）
            if (union == null)
            {
                if (menuQ != null) return menuQ.Select(_ => new UnionRow()).Where(_ => false);
                if (articleQ != null) return articleQ.Select(_ => new UnionRow()).Where(_ => false);
                if (prodQ != null) return prodQ.Select(_ => new UnionRow()).Where(_ => false);
                return Enumerable.Empty<UnionRow>().AsQueryable();
            }

            // ✅ 關鍵：ORDER BY 全部改成「欄位」，避免 SQL Server 判定為常數運算式
            var ordered =
                union
                    .OrderBy(x => x.SortType)                 // 商品優先
                    .ThenBy(x => x.SortSoldOut)               // 售完排後（升冪：0 在前、1 在後）
                    .ThenBy(x => x.SortDiscontinued)          // 停產排後
                    .ThenByDescending(x => x.MatchCount)      // 使用者匹配數（商品+文章）
                    .ThenByDescending(x => x.NodeDate)        // 文章日期
                    .ThenBy(x => x.SerNo)                     // 通用 SerNo
                    .ThenBy(x => x.ItemNo)                    // 商品 ItemNo
                    .ThenByDescending(x => x.Id);             // 最終穩定

            return ordered;
        }

        private async Task FillProdFiltersAndDirectoryTypeAsync(
            DirectoryReleInfoGetDto output,
            DirectoryReleInfoInputDto dto,
            long websiteId,
            IQueryable<Prod> baseProdQ)
        {
            // 這裡只做「可選項」的蒐集（不改結果），並且避免撈全表：只對 keyword 結果範圍做彙整
            // ⚠ 只要你 keyword 結果本身就有上萬筆，任何彙整都會重；但這些是 UI 必要資訊，只能做「彙整 SQL」。

            // 先拿 keyword 結果 ids（用 subquery，不 ToList 全部）
            var prodIdsQ = baseProdQ.Select(p => p.Id);

            // 1) 標籤 Filter（含 TagGroup + Other）
            //    只取商品關聯到的 tag，並計算 count
            var tagBindQ = db.Tag_Associates.AsNoTracking()
                .Include(e => e.Tag)
                .Where(e => !e.IsDeleted)
                .Where(e => e.Type == TagAssociateTypeEnum.商品)
                .Where(e => e.Tag != null && !e.Tag.IsDeleted && e.Tag.FK_WebsiteId == websiteId)
                .Where(e => prodIdsQ.Contains(e.FK_AId));

            // TagGroup ids（用於分 Other）
            var groupedTagIds = await db.Tag_TagGroups.AsNoTracking()
                .Include(e => e.Tag)
                .Where(e => !e.IsDeleted)
                .Where(e => e.Tag != null && !e.Tag.IsDeleted && e.Tag.FK_WebsiteId == websiteId)
                .Select(e => e.FK_TId)
                .Distinct()
                .ToListAsync();

            // Other（不在任何 TagGroup 裡）
            var otherTagAgg = await tagBindQ
                .Where(tb => !groupedTagIds.Contains(tb.FK_TId))
                .GroupBy(tb => new { tb.FK_TId, Title = tb.Tag!.Title })
                .Select(g => new TagGetSelectedDto
                {
                    FK_TId = g.Key.FK_TId,
                    Tag_Name = g.Key.Title ?? "",
                    count = g.Count()
                })
                .ToListAsync();

            if (otherTagAgg.Count > 0)
            {
                output.Filter.Add(new DirectorySearchTypeListDto
                {
                    Id = 0,
                    Type = DirectorySearchTypeEnum.標籤,
                    Name = L.get("Other"),
                    Tags = otherTagAgg
                });
            }

            // 每個 TagGroup 的 tags + count
            var tagGroups = await db.Tag_Groups.AsNoTracking()
                .Where(tg => !tg.IsDeleted && tg.FK_WebsiteId == websiteId)
                .Select(tg => new { tg.Id, tg.Title })
                .ToListAsync();

            foreach (var tg in tagGroups)
            {
                var tags = await (
                    from t in db.Tags.AsNoTracking().Where(x => !x.IsDeleted && x.FK_WebsiteId == websiteId)
                    join m in db.Tag_TagGroups.AsNoTracking().Where(x => !x.IsDeleted) on t.Id equals m.FK_TId
                    where m.FK_TGId == tg.Id
                    select new TagGetSelectedDto
                    {
                        FK_TId = t.Id,
                        Tag_Name = t.Title,
                        count = tagBindQ.Count(b => b.FK_TId == t.Id)
                    }
                ).ToListAsync();

                output.Filter.Add(new DirectorySearchTypeListDto
                {
                    Id = tg.Id,
                    Type = DirectorySearchTypeEnum.標籤,
                    Name = tg.Title,
                    Tags = tags
                });
            }

            // 2) 技術文件 Filter（只有商品有）
            var tecBindQ = db.Prod_TechCerts.AsNoTracking()
                .Include(x => x.TechnicalCertificate)
                .Where(x => !x.IsDeleted)
                .Where(x => x.TechnicalCertificate != null && !x.TechnicalCertificate.IsDeleted && x.TechnicalCertificate.FK_WebsiteId == websiteId)
                .Where(x => prodIdsQ.Contains(x.FK_PId));

            var tecAgg = await tecBindQ
                .GroupBy(x => new { x.FK_TCId, Title = x.TechnicalCertificate!.Title })
                .Select(g => new TagGetSelectedDto
                {
                    FK_TId = g.Key.FK_TCId,
                    Tag_Name = g.Key.Title ?? "",
                    count = g.Count()
                })
                .ToListAsync();

            if (tecAgg.Count > 0)
            {
                output.Filter.Add(new DirectorySearchTypeListDto
                {
                    Id = 0,
                    Type = DirectorySearchTypeEnum.技術文件,
                    Name = "技術文件",
                    Tags = tecAgg
                });
            }

            // 3) DirectoryType（目錄篩選用）
            //    依你原本語意：找出「哪些目錄的 tag 與這批商品 tag 有交集」
            //    注意：只做可選項，不套用 dto.DirectoryType（那是結果過濾）
            var prodTagIdsQ = tagBindQ.Select(x => x.FK_TId).Distinct();

            output.DirectoryType = await (
                from dir in db.Directory.AsNoTracking()
                    .Where(d => !d.IsDeleted && d.FK_WebsiteId == websiteId)
                where db.Tag_Associates.AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Type == TagAssociateTypeEnum.目錄 && a.FK_AId == dir.Id)
                    .Select(a => a.FK_TId)
                    .Any(dirTagId => prodTagIdsQ.Contains(dirTagId))
                select new DirectoryListBySearchDto
                {
                    Id = dir.Id,
                    Name = dir.Title
                }
            ).ToListAsync();
        }

        async Task FillArticleTagFiltersAsync(
    DirectoryReleInfoGetDto output,
    long websiteId,
    IQueryable<Core.Models.Article> articleQ)
        {
            // 文章：標籤 Filter + DirectoryType（目錄篩選用）
            var artIdsQ = articleQ.Select(a => a.Id);

            var tagBindQ = db.Tag_Associates.AsNoTracking()
                .Include(e => e.Tag)
                .Where(e => !e.IsDeleted)
                .Where(e => e.Type == TagAssociateTypeEnum.文章)
                .Where(e => e.Tag != null && !e.Tag.IsDeleted && e.Tag.FK_WebsiteId == websiteId)
                .Where(e => artIdsQ.Contains(e.FK_AId));

            // 1) 標籤 Filter（維持你原本）
            var tagAgg = await tagBindQ
                .GroupBy(tb => new { tb.FK_TId, Title = tb.Tag!.Title })
                .Select(g => new TagGetSelectedDto
                {
                    FK_TId = g.Key.FK_TId,
                    Tag_Name = g.Key.Title ?? "",
                    count = g.Count()
                })
                .ToListAsync();

            if (tagAgg.Count > 0)
            {
                output.Filter.Add(new DirectorySearchTypeListDto
                {
                    Id = 0,
                    Type = DirectorySearchTypeEnum.標籤,
                    Name = "標籤",
                    Tags = tagAgg
                });
            }

            // 2) DirectoryType（目錄篩選用）
            //    與商品同語意：找出「哪些目錄的 tag 與這批文章 tag 有交集」
            //    只做可選項，不套用 dto.DirectoryType（套用在 ApplyArticleFilters）
            var artTagIdsQ = tagBindQ.Select(x => x.FK_TId).Distinct();

            output.DirectoryType = await (
                from dir in db.Directory.AsNoTracking()
                    .Where(d => !d.IsDeleted && d.FK_WebsiteId == websiteId)
                where db.Tag_Associates.AsNoTracking()
                    .Where(a => !a.IsDeleted && a.Type == TagAssociateTypeEnum.目錄 && a.FK_AId == dir.Id)
                    .Select(a => a.FK_TId)
                    .Any(dirTagId => artTagIdsQ.Contains(dirTagId))
                select new DirectoryListBySearchDto
                {
                    Id = dir.Id,
                    Name = dir.Title
                }
            ).ToListAsync();
        }

        private async Task<Dictionary<long, List<TagGetSelectedDto>>> GetTagsForObjectsAsync(
            long websiteId,
            TagAssociateTypeEnum type,
            List<long> objectIds)
        {
            var dict = new Dictionary<long, List<TagGetSelectedDto>>();
            if (objectIds == null || objectIds.Count == 0) return dict;

            var rows = await (
                from ta in db.Tag_Associates.AsNoTracking().Include(x => x.Tag)
                where !ta.IsDeleted
                where ta.Type == type
                where objectIds.Contains(ta.FK_AId)
                where ta.Tag != null && !ta.Tag.IsDeleted && ta.Tag.FK_WebsiteId == websiteId
                select new
                {
                    ObjId = ta.FK_AId,
                    TagId = ta.FK_TId,
                    Title = ta.Tag!.Title
                }
            ).ToListAsync();

            foreach (var r in rows)
            {
                if (!dict.TryGetValue(r.ObjId, out var list))
                {
                    list = new List<TagGetSelectedDto>();
                    dict[r.ObjId] = list;
                }

                list.Add(new TagGetSelectedDto
                {
                    FK_TId = r.TagId,
                    Tag_Name = r.Title ?? ""
                });
            }

            return dict;
        }

        private async Task FillProdPriceAsync(DirectoryReleInfoDto data, long websiteId)
        {
            // 你原本的 showprice 判斷
            var storeReset = await (
                from sd in db.StoreSetDetail
                join ss in db.StoreSet on sd.FK_StoreSetId equals ss.Id
                where sd.FK_WebsiteId == websiteId
                where ss.key == "storeBuyState"
                select sd.value
            ).FirstOrDefaultAsync();

            var showPrice = !(storeReset == "noPayNoShow");
            if (!showPrice) return;

            // stock / price
            var stocks = await db.Prod_Stocks.AsNoTracking()
                .Where(e => !e.IsDeleted && e.FK_Pid == data.Id)
                .ToListAsync();

            var stockIds = stocks.Select(s => s.Id).ToList();
            if (stockIds.Count == 0) return;

            var tokenUuid = await tokenAppService.GetUUID();
            var token = await db.Tokens.AsNoTracking().FirstOrDefaultAsync(e => e.UUID == tokenUuid);

            var prices = await productAppService.GetPriceByStock(stockIds);
            if (prices == null || prices.Count == 0) return;

            var tempPrice = prices.OrderByDescending(p => p.Price).FirstOrDefault();
            var stock = stocks.FirstOrDefault(s => tempPrice != null && s.Id == tempPrice.FK_PSId);

            if (stock == null || stock.IsTimePrice)
            {
                data.Price = L.get("MarketPrice");
                data.SuggestPrice = null;
                data.OriPrice = null;
                return;
            }

            if (token?.UserID == null && tempPrice?.FK_RId == 1)
            {
                var suggest = stock.Price;
                if (suggest > 0) data.SuggestPrice = suggest.ToString("N0");
            }

            data.Bonus = tempPrice?.Bonus.ToString("N0");
            data.Price = tempPrice?.Price?.ToString("N0") ?? "0";
            data.OriPrice = tempPrice?.OriPrice?.ToString("N0") ?? "0";
        }
        private async Task<DirectoryReleInfoGetDto> SearchProd(DirectoryReleInfoInputDto dto)
        {
            var UUID = await tokenAppService.GetUUID();
            var token = await db.Tokens.Where(e => e.UUID == UUID).FirstOrDefaultAsync();
            var output = new DirectoryReleInfoGetDto { ReleInfos = new List<DirectoryReleInfoDto>() };
            if (string.IsNullOrEmpty(dto.SearchText)) return output;
            long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
            Regex imgRegex = new Regex("(?:src=[\\S]*quot;)[\\S]*(?:quot;)", RegexOptions.IgnoreCase);
            int page = dto.Page ?? 0;
            int shownum = dto.ShowNum ?? 0;
            if (shownum <= 0 && shownum != -500) shownum = 12;
            int skip = shownum == -500 ? 0 : (page - 1) * shownum - 1;
            if (skip < 0) skip = 0;
            IQueryable<Prod>? prods = db.Prods.Include(e => e.Website)
                .Where(e => !e.IsDeleted).Where(e => !e.RemovedFromShelves)
                .Where(e => e.Visible)
                .Where(e => e.FK_WebsiteId == WebsiteID)
                .Where(e =>
                    e.Title.Contains(dto.SearchText ?? "") ||
                    e.Introduction.Contains(dto.SearchText ?? "") ||
                    e.Description.Contains(dto.SearchText ?? "") ||
                    (e.Html ?? "").Contains(dto.SearchText ?? "") ||
                    (e.ItemNo != null && e.ItemNo.Contains(dto.SearchText ?? "")) ||
                    db.Tag_Associates.Include(t => t.Tag)
                        .Where(t => t.Tag != null && t.Tag.FK_WebsiteId == WebsiteID && t.Type == TagAssociateTypeEnum.商品 && !t.Tag.IsDeleted && !t.IsDeleted)
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
                        .Where(t => t.Tag != null && t.Tag.FK_WebsiteId == WebsiteID && t.Type == TagAssociateTypeEnum.商品 && !t.Tag.IsDeleted && !t.IsDeleted)
                        .Where(d =>
                            db.Tag_Associates.Include(t => t.Tag)
                                .Where(t => t.Tag != null && t.Tag.FK_WebsiteId == WebsiteID && t.Type == TagAssociateTypeEnum.目錄 && !t.Tag.IsDeleted && !t.IsDeleted)
                                .Where(t => t.FK_AId == dto.DirectoryType)
                                .Select(t => t.FK_TId).Contains(d.FK_TId)
                        )
                        .Select(t => t.FK_AId).Contains(e.Id)
                );
            }
            List<long> Ids = await prods.Select(e => e.Id).ToListAsync();

            if (dto.Filters == null) dto.Filters = new List<DirectoryFilterDto>();
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
                                                              .Where(e => !e.IsDeleted && e.Type == TagAssociateTypeEnum.商品 && !e.Tag.IsDeleted)
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
                                                              .Where(e => !e.IsDeleted && e.Type == TagAssociateTypeEnum.商品 && !e.Tag.IsDeleted)
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
                    .Where(e => Ids.Contains(e.FK_AId) && e.Type == TagAssociateTypeEnum.商品);
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
                Name = L.get("Other"),
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
                            .Where(a => !a.IsDeleted && a.Type == TagAssociateTypeEnum.目錄 && a.FK_AId == d.Id)
                            .Where(a =>
                                db.Tag_Associates.Where(p => !p.IsDeleted && p.Type == TagAssociateTypeEnum.商品 && dirFilterIds.Contains(p.FK_AId))
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
            if (shownum == -500) shownum = prods.Count();
            output.TotalPage = (int)Math.Ceiling(output.TotalCount / (double)shownum);

            // 取得當前使用者的分群資訊
            var Groping = db.UserGroupingDetails.Include(e => e.userGrouping)
                .Where(ugd => ugd.UUID == UUID).FirstOrDefault();
            var groupTags = Groping != null ?
                    db.Tag_Associates.Where(e => e.FK_AId == Groping.FK_GropingId && e.Type == TagAssociateTypeEnum.使用者分群).Select(e => e.FK_TId).ToList() :
                    new List<long>();

            // 查詢個人標籤（若無分群）
            var personalTags = db.UserTagStatistics
                .AsNoTracking()
                .Where(ut => ut.UUID == UUID) // 根據 UUID 查詢使用者標籤
                .OrderByDescending(ut => ut.Weight) // 按權重排序
                .Take(5) // 取權重最高的前 5 個標籤
                .Select(ut => ut.FK_TagId)
                .ToList();

            var relevantTags = groupTags.Any() ? groupTags : personalTags;
            relevantTags = relevantTags ?? new List<long>();

            var tagAssociations = db.Tag_Associates
                .AsNoTracking()
                .Where(ta => ta.Type == TagAssociateTypeEnum.商品); // 篩選商品類型的標籤

            // 最終查詢
            var dataMargin = prods
                .Select(p => new
                {
                    Prod = p,
                    MatchingTags = tagAssociations
                        .Where(ta => ta.FK_AId == p.Id && relevantTags.Contains(ta.FK_TId)) // 只選擇符合的標籤
                        .Select(ta => ta.FK_TId)
                        .ToList()
                })
                .OrderBy(e => e.Prod.Status == ProdStatusEnum.售完)
                .OrderBy(e => e.Prod.Status == ProdStatusEnum.停產)
                .ThenByDescending(p => p.MatchingTags.Count) // 按符合標籤數量排序
                .ThenBy(p => p.Prod.Ser_No) // 次要排序
                .ThenBy(p => p.Prod.ItemNo)
                .ThenByDescending(p => p.Prod.Id) // 再次排序
                .Skip(skip)
                .Take(shownum)
                .Select(p => p.Prod); // 最終只返回商品

            var filtersstr = "";

            if (dto.Filters.Count > 0) filtersstr = JsonConvert.SerializeObject(dto.Filters);
            if (filtersstr.Length > 0) filtersstr = $"?filter={filtersstr}";

            var list = await (from p in dataMargin
                              select new DirectoryReleInfoDto
                              {
                                  Id = p.Id,
                                  Title = p.Title,
                                  Link = $"/product/{p.Id}{filtersstr}",
                                  OrgName = p.Website != null ? p.Website.OrgName : "",
                                  type = DirectoryTypeEnum.商品,
                                  SerNo = p.Ser_No,
                                  ItemNo = p.ItemNo,
                                  Description = p.Description,
                                  MainImage = p.Html,
                                  Status = p.Status,
                                  StatusName = p.Status.ToString(),
                                  tags = (from t in db.Tags.Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == WebsiteID)
                                          join m in db.Tag_Associates.Where(e => !e.IsDeleted) on t.Id equals m.FK_TId
                                          where m.FK_AId == p.Id && m.Type == TagAssociateTypeEnum.商品
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

                var stocks = await db.Prod_Stocks.Where(e => e.FK_Pid == data.Id).Where(e => !e.IsDeleted).ToListAsync();
                var stockids = stocks.Select(e => e.Id).ToList();
                var sotreset = await (from sd in db.StoreSetDetail
                                      join ss in db.StoreSet on sd.FK_StoreSetId equals ss.Id
                                      where sd.FK_WebsiteId == WebsiteID
                                      where ss.key == "storeBuyState"
                                      select sd.value).FirstOrDefaultAsync();

                var showprice = !(sotreset == "noPayNoShow");
                if (showprice)
                {
                    var prices = await productAppService.GetPriceByStock(stockids);

                    var temp_price = prices.Where(e => e.Price == (prices.Max(e => e.Price))).FirstOrDefault();
                    var stock = stocks.Where(e => e.Id == temp_price?.FK_PSId).FirstOrDefault();
                    if (stock == null || stock.IsTimePrice)
                    {
                        data.Price = L.get("MarketPrice");
                        data.SuggestPrice = null;
                        data.OriPrice = null;
                    }
                    else
                    {
                        if (token?.UserID == null && temp_price?.FK_RId == 1)
                        {
                            var SuggestPrice = stock?.Price ?? 0;
                            if (SuggestPrice > 0) data.SuggestPrice = (SuggestPrice).ToString("N0");
                        }
                        data.Bonus = temp_price?.Bonus.ToString("N0");
                        data.Price = temp_price?.Price?.ToString("N0") ?? "0";
                        data.OriPrice = temp_price?.OriPrice?.ToString("N0") ?? "0";
                    }
                }
            }
            output.ReleInfos = list;
            return output;
        }
        #endregion

        private static readonly Regex RxRemoveIcons = new(
            @"<\s*(?:span|i)\b[^>]*\bclass\s*=\s*""[^""]*\bmaterial-symbols-outlined\b[^""]*""[^>]*>.*?<\s*/\s*(?:span|i)\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled
        );

        private static readonly Regex RxStripTags = new(@"<[^>]+>", RegexOptions.Compiled);

        private static readonly Regex RxCollapseSpaces = new(@"\s+", RegexOptions.Compiled);
        private string getSearchDescription(string? content, string findstr)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;

            string s = stringHandler.HtmlDecode(content);

            // 1) 去掉 icon 節點
            s = RxRemoveIcons.Replace(s, string.Empty);

            // 2) 去標籤
            s = RxStripTags.Replace(s, string.Empty);

            // 3) 收斂空白
            s = RxCollapseSpaces.Replace(s, " ").Trim();

            // 4) 找出關鍵字前後的片段
            int idx = string.IsNullOrEmpty(findstr)
                ? -1
                : s.IndexOf(findstr, StringComparison.OrdinalIgnoreCase);
            int start = Math.Max(0, (idx >= 0 ? idx : 0) - 10);
            if (start > 0) s = " ... " + s.Substring(start);

            // 5) 關鍵字高亮（避免用 Regex.Replace，改用 StringBuilder）
            if (!string.IsNullOrEmpty(findstr))
            {
                var sb = new StringBuilder(s.Length + 64);
                int last = 0, flen = findstr.Length;

                while ((idx = s.IndexOf(findstr, last, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    sb.Append(s, last, idx - last);
                    sb.Append("<span class='bg-warning text-dark'>");
                    sb.Append(s, idx, flen);
                    sb.Append("</span>");
                    last = idx + flen;
                }
                sb.Append(s, last, s.Length - last);
                s = sb.ToString();
            }

            return s;
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
            corr = (from d in db_d
                    select new CorrDTAID
                    {
                        DirectoryId = d.Id,
                        DirectoryName = d.Title,
                    }).ToList();

            if (db_d != null)
            {
                var tagsData = await db.Tag_Associates
                    .Include(e => e.Tag)
                    .Where(e => dto.Ids.Contains(e.FK_AId)) // 目錄 ID
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.Type == TagAssociateTypeEnum.目錄)
                    .Where(e => siteIds.Contains(e.Tag.FK_WebsiteId))
                    .ToListAsync();  // 拉取資料到記憶體中

                // 在記憶體中進行分組
                var tags = tagsData
                    .GroupBy(e => e.Tag.FK_WebsiteId)  // 先依網站分組
                    .ToDictionary(
                        g => g.Key, // WebsiteId
                        g => g.GroupBy(e => e.FK_AId)  // 然後依目錄 ID 分組
                            .ToDictionary(
                                innerGroup => innerGroup.Key, // 目錄 ID
                                innerGroup => innerGroup.Select(e => e.FK_TId).ToHashSet() // 標籤 ID
                            )
                    );

                foreach (var site in tags) // site.Key 是 WebsiteId
                {
                    foreach (var dir in site.Value) // dir.Key 是 FK_AId
                    {
                        int index = corr.FindIndex(c => c.DirectoryId == dir.Key);
                        if (index != -1)
                        {
                            corr[index].TagId = dir.Value.First();
                        }
                    }
                }

                var notTags = await db.Tag_Associates.Include(e => e.Tag)
                    .Where(e => dto.Ids.Contains(e.FK_AId))
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.Type == TagAssociateTypeEnum.目錄拒絕)
                    .Where(e => siteIds.Contains(e.Tag.FK_WebsiteId))
                    .Select(e => e.FK_TId) // 只取標籤 ID
                    .Distinct()
                    .ToListAsync();
                var notTagIds = new HashSet<long>(notTags ?? new List<long>()); // 排除的標籤
                if (tags != null)
                {
                    List<long> allIds = new List<long>();
                    switch ((DirectoryTypeEnum)db_d[0].Type)
                    {
                        case DirectoryTypeEnum.商品:
                            var pd_notId = await (db.Tag_Associates.Where(e => notTagIds.Contains(e.FK_TId) && e.Type == TagAssociateTypeEnum.商品)).Select(e => e.FK_AId).ToListAsync();

                            foreach (var site in tags)
                            {
                                long siteId = site.Key;
                                var FKTIds = site.Value.Values.SelectMany(tags => tags).ToHashSet();
                                var allProducts = await db.Tag_Associates.Include(e => e.Tag)
                                    .Where(e => FKTIds.Contains(e.FK_TId) && !pd_notId.Contains(e.FK_AId) && e.Type == TagAssociateTypeEnum.商品)
                                    .Where(e => siteId == e.Tag.FK_WebsiteId)
                                    .Select(e => new { e.FK_AId, e.FK_TId })
                                    .ToListAsync();
                                // 按商品 ID 分群
                                var groupedProducts = allProducts
                                    .GroupBy(e => e.FK_AId)
                                    .ToDictionary(g => g.Key, g => g.Select(e => e.FK_TId).ToHashSet());
                                allIds.AddRange(
                                    groupedProducts.Where(g => site.Value.Values.Any(tagSet => tagSet.IsSubsetOf(g.Value))) // 至少符合一個目錄標籤
                                        .Select(g => g.Key)
                                        .ToList()
                                );
                            }
                            //var allProducts = await db.Tag_Associates.Where(e => FKTIds.Contains(e.FK_TId) && !pd_notId.Contains(e.FK_AId) && e.Type == TagAssociateTypeEnum.商品).Select(e => new { e.FK_AId, e.FK_TId }).ToListAsync();

                            if (allIds.Any())
                            {
                                DataIds = db.Prods
                                    .Where(e => allIds.Contains(e.Id))
                                    .Where(e => e.Visible && !e.RemovedFromShelves)
                                    .Where(e => siteIds.Contains(e.FK_WebsiteId))
                                    .Where(e => e.permanent || (DateTime.Now >= e.StartTime && DateTime.Now <= e.EndTime))
                                    .OrderBy(e => e.Ser_No)
                                    .ThenByDescending(e => e.Status == ProdStatusEnum.新品)
                                    .ThenByDescending(e => e.Status != ProdStatusEnum.售完)
                                    .ThenByDescending(e => e.Status != ProdStatusEnum.停產)
                                    .ThenBy(e => e.ItemNo)
                                    .ThenBy(e => e.Title).ThenByDescending(e => e.Id)
                                    .Select(e => e.Id).ToList();
                            }
                            break;
                        case DirectoryTypeEnum.文章:
                            var tempcorr = new List<CorrDTAID>();
                            foreach (var site in tags)
                            {
                                long siteId = site.Key;
                                var FKTIds = site.Value.Values.SelectMany(tags => tags).ToHashSet();
                                var db_as = await db.Tag_Associates
                                    .Where(e => !e.IsDeleted)
                                    .Where(e => e.Type == TagAssociateTypeEnum.文章)
                                    .Where(e => FKTIds.Contains(e.FK_TId))
                                    .ToListAsync();
                                db_as.ForEach(a =>
                                {
                                    var dindex = corr.FindIndex(c => c.TagId == a.FK_TId);
                                    if (dindex != -1)
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
                                if (db_as != null)
                                {
                                    // 按商品 ID 分群
                                    var groupedArticle = db_as
                                        .GroupBy(e => e.FK_AId)
                                        .ToDictionary(g => g.Key, g => g.Select(e => e.FK_TId).ToHashSet());
                                    allIds.AddRange(
                                        groupedArticle
                                            .Where(g => site.Value.Values.Any(tagSet => tagSet.IsSubsetOf(g.Value))) // 至少符合一個目錄標籤
                                            .Select(g => g.Key)
                                            .ToList()
                                    );
                                }
                            }
                            corr = tempcorr;
                            var articleQuery = db.Article
                                       .Where(e => allIds.Contains(e.Id) && !notTagIds.Contains(e.Id))
                                       .Where(e => !e.IsDeleted)
                                       .Where(e => e.Visible)
                                       .Where(e => siteIds.Contains(e.FK_WebsiteId))
                                       .Where(e => e.permanent || (DateTime.Now >= e.StartTime && DateTime.Now <= e.EndTime));
                            if (!string.IsNullOrEmpty(dto.Facet) && db_d != null && db_d.Any())
                            {
                                var dir = db_d.First();
                                switch (dir.FacetType) {
                                    case DirectoryFacetTypeEnum.Year:
                                        var years = dto.Facet
                                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim())
                                            .Select(s => int.TryParse(s, out var y) ?
                                                dir.CalendarType == DirectoryCalendarTypeEnum.民國年? (int?)y + 1911 : (int?)y : 
                                            null)
                                            .Where(y => y.HasValue)
                                            .Select(y => y!.Value)
                                            .ToHashSet();
                                        if (years.Count > 0)
                                        {
                                            articleQuery = articleQuery.Where(e =>
                                                e.NodeDate.HasValue && years.Contains(e.NodeDate.Value.Year));
                                        }
                                        break;
                                }
                            }
                            DataIds = articleQuery.Select(e => e.Id).ToList();
                            break;
                        default:
                            break;
                    }

                    var page = dto.Page == null || dto.Page.Value <= 0 ? 1 : dto.Page.Value;

                    var shownum = dto.ShowNum == null || dto.ShowNum.Value <= 0
                        ? DataIds.Count
                        : dto.ShowNum.Value;

                    var maxLen = dto.MaxLen ?? 0;

                    var effectiveTotalCount = maxLen > 0
                        ? Math.Min(DataIds.Count, maxLen)
                        : DataIds.Count;

                    output.TotalCount = effectiveTotalCount;
                    output.TotalPage = shownum > 0
                        ? (int)Math.Ceiling(effectiveTotalCount / (double)shownum)
                        : 0;

                    var pageDataIds = DataIds
                        .Take(effectiveTotalCount)
                        .Skip((page - 1) * shownum)
                        .Take(shownum)
                        .ToList();

                    switch ((DirectoryTypeEnum)db_d[0].Type)
                    {
                        case DirectoryTypeEnum.商品:
                            var tempproddata = await productAppService.GetDirectoryReleInfo(new DirectoryReleInfoInputDto
                            {
                                Ids = pageDataIds,
                                SiteId = WebsiteID
                            });
                            if (tempproddata != null && tempproddata.Count > 0)
                            {
                                var dirid = string.Join(",", dto.Ids);

                                foreach (var item in tempproddata)
                                {
                                    item.Link += $"?dirid={dirid}";
                                }

                                output.ReleInfos = tempproddata;
                            }
                            else
                            {
                                output.ReleInfos = new List<DirectoryReleInfoDto>();
                            }
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
                                Latitude = dto.Latitude,
                            });
                            if (temparticledata != null)
                            {
                                foreach (var item in temparticledata)
                                {
                                    var dindex = corr.FindIndex(c => c.ArticleId == item.Id);
                                    if (dindex != -1)
                                    {
                                        item.Dirname = corr[dindex].DirectoryName;
                                    }
                                }

                                if (dto.FindNearest != true)
                                {
                                    foreach (var item in temparticledata)
                                    {
                                        var dirid = string.Join(",", dto.Ids);
                                        item.Link += $"?dirid={dirid}";
                                    }
                                }
                                output.ReleInfos = temparticledata;
                            }

                            break;
                        default:
                            break;
                    }

                    var sotreset = await (from sd in db.StoreSetDetail
                                          join s in db.StoreSet on sd.FK_StoreSetId equals s.Id
                                          where sd.FK_WebsiteId == WebsiteID
                                          where s.key == "storeBuyState"
                                          select sd.value).FirstOrDefaultAsync();

                    var showprice = !(sotreset == "noPayNoShow");

                    if (!showprice)
                    {
                        for (var i = 0; i < output.ReleInfos.Count; i++)
                        {
                            output.ReleInfos[i].Price = null;
                        }
                    }
                }
            }
            return output;
        }
        public async Task<MenuItemDto> GetReleMenu(DataIdWebsiteIdDto dto)
        {
            var websiteid = dto.WebsiteId;
            if (websiteid == 0) websiteid = await loginUserData.GetWebsiteId();
            var output = await (from e in db.Directory where dto.Ids.Contains(e.Id) && !e.IsDeleted select e.FK_Mid).FirstOrDefaultAsync();
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
                                    where new List<int> { 1, 2, 3, 4 }.Contains(e.Type)
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
                                        Type = TagAssociateTypeEnum.目錄
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
        public async Task<JsonResult> GetAdvertiseList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                long UserID = await loginUserData.GetUserId();
                List<long> RoleIds = await loginUserData.GetUserRoleIds();
                bool isSuperUser = await permissionsAppService.IsPowerUserPermissions();
                IQueryable<Core.Models.Directory> result = db.Directory.Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == WebsiteID);
                if (result != null)
                {
                    var dataQuery = from e in result
                                    where e.Type == 4
                                    select new DirectoryGetListDto
                                    {
                                        Id = e.Id,
                                        Title = e.Title,
                                        Description = e.Description,
                                        Type = ((DirectoryTypeEnum)e.Type).ToString(),
                                        Visible = e.Visible,
                                        Items = "",
                                        FK_Mid = e.FK_Mid,
                                        SortBy = $"{(SortByEnum)e.SortBy}排序",
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    if (output != null)
                    {
                        foreach (var data in output.data)
                        {
                            var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                            {
                                Fk_Aid = (long)data.GetType().GetProperty("Id").GetValue(data, null),
                                Type = TagAssociateTypeEnum.目錄
                            });
                            var tag_text = "";
                            foreach (var tagData in tagDatas) tag_text += tag_text == "" ? tagData.Tag_Name : $"、{tagData.Tag_Name}";
                            data.GetType().GetProperty("Items").SetValue(data, tag_text == "" ? "無" : tag_text);
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
                    var tagids = await db.Tag_Associates.Where(e => e.FK_AId == Id && e.Type == TagAssociateTypeEnum.目錄 && !e.IsDeleted).ToListAsync();

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
                        .Where(e => e.Type == TagAssociateTypeEnum.目錄)
                        .Where(e => e.Tag.FK_WebsiteId == WebsiteID)
                        .ToListAsync();


                    if (d_tags != null)
                    {
                        var tlist = d_tags.Select(e => e.FK_TId).ToList();
                        var a_tags = await db.Tag_Associates.Include(e => e.Tag)
                            .Where(e => tlist.Contains(e.FK_TId))
                            .Where(e => !e.IsDeleted)
                            .Where(e => e.Type == TagAssociateTypeEnum.文章)
                            .Where(e => e.Tag.FK_WebsiteId == WebsiteID)
                            .ToListAsync();
                        if (!a_tags.Any()) throw new Exception("資料不存在");
                        var aids = a_tags.Select(e => e.FK_AId).ToList();
                        var dataQuery = from a in db.Article.Where(e => !e.IsDeleted)
                                        where aids.Contains(a.Id)
                                        select new DirectoryReleInfoDto
                                        {
                                            Id = a.Id,
                                            Visible = a.Visible,
                                            Available = !a.RemovedFromShelves,
                                            Title = a.Title,
                                            Description = a.Description,
                                            SerNo = a.SerNO,
                                            NodeDate = a.NodeDate,
                                            LastModificationTime = a.LastModificationTime ?? a.CreationTime
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
                        .Where(e => e.Type == TagAssociateTypeEnum.目錄)
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
        public async Task<JsonResult> GetDirectoryAdvertiseList(long id, DataSourceLoadOptions loadOptions)
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
                        .Where(e => e.Type == TagAssociateTypeEnum.目錄)
                        .Where(e => e.Tag.FK_WebsiteId == WebsiteID)
                        .ToListAsync();

                    if (d_tags != null)
                    {
                        var adids = await GetReleAdId(d_tags.Select(e => e.FK_TId).ToList());
                        var dataQuery = from a in db.Advertise.Where(e => !e.IsDeleted)
                                        where adids.Contains(a.Id)
                                        select new DirectoryReleInfoDto
                                        {
                                            Id = a.Id,
                                            Title = a.Title,
                                            Description = a.Describe,
                                            StartTime = a.StartDate,
                                            EndTime = a.EndDate,
                                            SerNo = a.SerNO,
                                            Visible = a.Visible,
                                            ClickTimes = a.Clicks,
                                            ExposureTimes = a.Exposure,
                                            LastModificationTime = a.LastModificationTime ?? a.CreationTime
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
        public async Task<List<AdvertiseDisplayDto>> GetReleAd(DataIdWebsiteIdDto dto)
        {
            var output = new List<AdvertiseDisplayDto>();
            var websiteid = dto.WebsiteId;
            if (websiteid == 0) websiteid = await loginUserData.GetWebsiteId();
            try
            {
                var db_d = db.Directory.Where(e => dto.Ids.Contains(e.Id) && e.FK_WebsiteId == websiteid && !e.IsDeleted).FirstOrDefault();
                var DataIds = new List<long>();
                if (db_d != null)
                {
                    var adids = new List<long>();
                    foreach (var id in dto.Ids)
                    {
                        var d_tags = await db.Tag_Associates.Include(e => e.Tag)
                            .Where(e => id == e.FK_AId)
                            .Where(e => !e.IsDeleted)
                            .Where(e => e.Type == TagAssociateTypeEnum.目錄)
                            .Where(e => e.Tag.FK_WebsiteId == websiteid)
                            .ToListAsync();
                        if (d_tags != null)
                        {
                            var tempadids = await GetReleAdId(d_tags.Select(e => e.FK_TId).ToList());
                            adids.AddRange(tempadids);
                        }
                    }

                    if (adids.Any())
                    {
                        adids = adids.Distinct().ToList();
                        var adresult = db.Advertise;
                        output = await (from e in adresult
                                        where adids.Contains(e.Id)
                                        where !e.IsDeleted && e.Visible
                                        where e.Permanent || ((DateTime.Compare((DateTime)e.StartDate, DateTime.Now) < 0) && (DateTime.Compare((DateTime)e.EndDate, DateTime.Now) > 0))
                                        orderby Guid.NewGuid()
                                        select new AdvertiseDisplayDto
                                        {
                                            Id = e.Id,
                                            Title = e.Title,
                                            Describe = e.Describe,
                                            Link = e.Link,
                                            Target = e.Target,
                                            Clicks = e.Clicks,
                                            Exposure = e.Exposure,
                                            SerNO = e.SerNO,
                                        }).ToListAsync();
                        switch (db_d.SortBy)
                        {
                            case 0:
                                output = output.OrderBy(o => o.SerNO).ToList();
                                break;
                            case 2:
                                output = output.OrderByDescending(o => (double)o.Clicks / (double)o.Exposure).ToList();
                                break;
                            case 3:
                                var UUID = await tokenAppService.GetUUID();

                                // 取得當前使用者的分群資訊
                                var Groping = db.UserGroupingDetails.Include(e => e.userGrouping)
                                    .Where(ugd => ugd.UUID == UUID).FirstOrDefault();
                                var groupTags = Groping != null ?
                                        db.Tag_Associates.Where(e => e.FK_AId == Groping.FK_GropingId && e.Type == TagAssociateTypeEnum.使用者分群).Select(e => e.FK_TId).ToList() :
                                        new List<long>();

                                // 查詢個人標籤（若無分群）
                                var personalTags = db.UserTagStatistics
                                    .AsNoTracking()
                                    .Where(ut => ut.UUID == UUID) // 根據 UUID 查詢使用者標籤
                                    .OrderByDescending(ut => ut.Weight) // 按權重排序
                                    .Take(5) // 取權重最高的前 5 個標籤
                                    .Select(ut => ut.FK_TagId)
                                    .ToList();

                                var relevantTags = groupTags.Any() ? groupTags : personalTags;
                                relevantTags = relevantTags ?? new List<long>();

                                var tagAssociations = db.Tag_Associates
                                    .AsNoTracking()
                                    .Where(ta => ta.Type == TagAssociateTypeEnum.商品); // 篩選商品類型的標籤

                                // 最終查詢
                                output = output
                                    .Select(p => new
                                    {
                                        Ads = p,
                                        MatchingTags = tagAssociations
                                            .Where(ta => ta.FK_AId == p.Id && relevantTags.Contains(ta.FK_TId)) // 只選擇符合的標籤
                                            .Select(ta => ta.FK_TId)
                                            .ToList()
                                    })
                                    .OrderByDescending(p => p.MatchingTags.Count) // 按符合標籤數量排序
                                    .ThenBy(p => p.Ads.SerNO) // 次要排序
                                    .ThenByDescending(p => p.Ads.Id) // 再次排序
                                    .Select(p => p.Ads).ToList(); // 最終只返回廣告
                                break;
                        }
                        for (var i = 0; i < output.Count; i++)
                        {
                            output[i].FileLink = await fileUploadAppService.getAdvertiseFiles(output[i].Id, (int)FileBindTypeEnum.自訂廣告);
                            output[i].TagDatas = await tagAppService.GetAdvertiseDataAll(output[i].Id);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return output;
        }
        public async Task<List<long>> GetReleAdId(List<long> FK_Tid_List)
        {
            var adlist = new List<long>();
            try
            {
                var a_tags = await db.Tag_Associates.Include(e => e.Tag)
                    .Where(e => FK_Tid_List.Contains(e.FK_TId))
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.Type == TagAssociateTypeEnum.廣告)
                    .ToListAsync();
                if (!a_tags.Any()) throw new Exception("資料不存在");
                var temp_aid = new Dictionary<long, long>();
                a_tags.ForEach(tag =>
                {
                    if (!temp_aid.ContainsKey(tag.FK_AId)) { temp_aid.Add(tag.FK_AId, 1); }
                    else { temp_aid[tag.FK_AId] = temp_aid[tag.FK_AId] + 1; }
                });
                foreach (var aid in temp_aid)
                {
                    if (aid.Value == FK_Tid_List.Count) { adlist.Add(aid.Key); }
                }
            }
            catch (Exception e)
            {
                return null;
            }
            return adlist;
        }
        public async Task<List<KeyValueDto>> SwitchPage(DirectorySwitchPageDto dto)
        {
            var WebsiteID = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
            List<KeyValueDto> response = new List<KeyValueDto>();
            try
            {
                if (dto.routername == "search")
                {
                    List<DirectoryFilterDto> Filters = new List<DirectoryFilterDto>();
                    Filters = dto.filters != null ? JsonConvert.DeserializeObject<List<DirectoryFilterDto>>(dto.filters) : null;
                    DirectoryReleInfoInputDto searchdto = new DirectoryReleInfoInputDto()
                    {
                        Ids = dto.type == 1 ? new List<long> { 3 } : new List<long> { 0 },
                        SearchText = dto.searchtext,
                        SiteId = WebsiteID,
                        Type = "search",
                        ShowNum = -500,
                        Filters = Filters,
                        Page = 0,
                    };
                    var releinfo = await GetReleInfo(searchdto);
                    if (dto.type == 1)
                    {
                        foreach (var info in releinfo.ReleInfos)
                        {
                            response.Add(new KeyValueDto() { Key = info.Id.ToString(), Value = info.Title.ToString() });
                        }
                    }
                    else
                    {
                        foreach (var info in releinfo.ReleInfos)
                        {
                            response.Add(new KeyValueDto() { Key = info.Link.ToString(), Value = info.Title.ToString() });
                        }
                    }
                }
                else
                {
                    if (dto.dirids == null)
                    {
                        dto.dirids = new List<long>();
                        var webmenus = await db.WebMenus.Where(e => e.FK_WebsiteId == WebsiteID && e.RouterName == dto.routername).FirstOrDefaultAsync();
                        if (webmenus != null)
                        {
                            var html = stringHandler.HtmlDecode(webmenus.Html);
                            var value = htmlProcessor.Find(htmlProcessor.LoadHtml(html), "[data-dirid]").FirstOrDefault().Attr("data-dirid");
                            if (!string.IsNullOrEmpty(value) && long.TryParse(value, out var dirId))
                            {
                                dto.dirids.Add(dirId);
                            }
                        }
                    }

                    if (dto.dirids != null)
                    {
                        string diridsStr = string.Join(",", dto.dirids);
                        var tagids = await (from tag in db.Tags
                                            join tagas in db.Tag_Associates on tag.Id equals tagas.FK_TId
                                            where tagas.Type == TagAssociateTypeEnum.目錄
                                            where dto.dirids.Contains(tagas.FK_AId)
                                            where tag.FK_WebsiteId == WebsiteID
                                            orderby tagas.Id
                                            select tag.Id).ToListAsync();
                        var not_tagids = await (from tag in db.Tags
                                                join tagas in db.Tag_Associates on tag.Id equals tagas.FK_TId
                                                where tagas.Type == TagAssociateTypeEnum.目錄拒絕
                                                where dto.dirids.Contains(tagas.FK_AId)
                                                where tag.FK_WebsiteId == WebsiteID
                                                select tag.Id).ToListAsync();
                        List<KeyValueDto> datas = new List<KeyValueDto>();

                        switch (dto.type)
                        {
                            case 1:
                                var not_tags_pid = await (from p in db.Prods
                                                          join tagas in db.Tag_Associates on p.Id equals tagas.FK_AId
                                                          where p.FK_WebsiteId == WebsiteID
                                                          where tagas.Type == TagAssociateTypeEnum.商品
                                                          where not_tagids.Contains(tagas.FK_TId)
                                                          select p.Id).ToListAsync();
                                var products_tags = await (from p in db.Prods
                                                           join tagas in db.Tag_Associates on p.Id equals tagas.FK_AId
                                                           where !not_tags_pid.Contains(p.Id)
                                                           where p.FK_WebsiteId == WebsiteID
                                                           where !p.RemovedFromShelves && p.Visible
                                                           where p.permanent || (DateTime.Now >= p.StartTime && DateTime.Now <= p.EndTime)
                                                           where tagids.Contains(tagas.FK_TId) && tagas.Type == TagAssociateTypeEnum.商品
                                                           orderby
                                                                p.Ser_No, p.Status == ProdStatusEnum.新品 descending,
                                                                p.Status != ProdStatusEnum.售完 descending, p.Status != ProdStatusEnum.停產 descending,
                                                                p.ItemNo, p.Title, p.Id descending
                                                           select new { p, tagas }).ToListAsync();

                                datas = products_tags
                                    .Select(x => new KeyValueDto()
                                    {
                                        Key = x.p.Id.ToString(),
                                        Value = x.p.Title ?? ""
                                    }).DistinctBy(x => x.Key).ToList();
                                break;
                            case 2:
                                var not_tags_aid = await (from a in db.Article
                                                          join tagas in db.Tag_Associates on a.Id equals tagas.FK_AId
                                                          where a.FK_WebsiteId == WebsiteID
                                                          where tagas.Type == TagAssociateTypeEnum.文章
                                                          where not_tagids.Contains(tagas.FK_TId)
                                                          select a.Id).ToListAsync();
                                var articles_tags = await (from a in db.Article
                                                           join tagas in db.Tag_Associates on a.Id equals tagas.FK_AId
                                                           where !not_tags_aid.Contains(a.Id)
                                                           where a.Visible
                                                           where a.FK_WebsiteId == WebsiteID
                                                           where tagas.Type == TagAssociateTypeEnum.文章 && tagids.Contains(tagas.FK_TId)
                                                           where a.permanent || (DateTime.Now >= a.StartTime && DateTime.Now <= a.EndTime)
                                                           orderby a.SerNO, a.NodeDate descending, a.Id descending
                                                           select new { a, tagas }).ToListAsync();
                                datas = articles_tags.OrderBy(x => tagids.IndexOf(x.tagas.FK_TId)).Select(x => new KeyValueDto()
                                {
                                    Key = x.a.Id.ToString(),
                                    Value = x.a.Title ?? ""
                                }).ToList();
                                break;
                        }

                        var index = datas.FindIndex(d => long.Parse(d.Key) == dto.id);
                        if (index > -1)
                        {
                            if (index == 0 && datas.Count > 1)
                            {
                                var keynext = $"{datas[index + 1].Key}?dirid={diridsStr}";
                                response.Add(new KeyValueDto());
                                response.Add(new KeyValueDto() { Key = keynext, Value = datas[index + 1].Value ?? "" });
                            }
                            else if (index == 0 && datas.Count == 1)
                            {
                                response.Add(new KeyValueDto());
                                response.Add(new KeyValueDto());
                            }
                            else if (index == datas.Count - 1)
                            {
                                var keyprev = $"{datas[index - 1].Key}?dirid={diridsStr}";
                                response.Add(new KeyValueDto() { Key = keyprev, Value = datas[index - 1].Value ?? "" });
                                response.Add(new KeyValueDto());
                            }
                            else
                            {
                                var keynext = $"{datas[index + 1].Key}?dirid={diridsStr}";
                                var keyprev = $"{datas[index - 1].Key}?dirid={diridsStr}";
                                response.Add(new KeyValueDto() { Key = keyprev, Value = datas[index - 1].Value ?? "" });
                                response.Add(new KeyValueDto() { Key = keynext, Value = datas[index + 1].Value ?? "" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"Directory=>SwitchPage回傳資料：{ex.Message}");
            }
            return response;
        }
        public async Task<ResponseMessageDto> GetDirectoryFacetConfig(long id)
        {
            var output = new ResponseMessageDto();

            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                if (websiteId <= 0)
                {
                    output.Success = false;
                    output.Message = "Website context not found.";
                    return output;
                }

                var entity = await db.Directory
                    .Include(d => d.DirectoryFacetRanges)  // QueryFilter 會自動排除 IsDeleted
                    .FirstOrDefaultAsync(d => d.Id == id && d.FK_WebsiteId == websiteId);

                if (entity == null)
                {
                    output.Success = false;
                    output.Message = "Directory not found.";
                    return output;
                }

                var dto = mapper.Map<DirectoryFacetConfigDto>(entity);

                // 確保 ranges 順序穩定（UI 依 Sort）
                dto.Ranges ??= new List<DirectoryFacetRangeDto>();
                dto.Ranges = dto.Ranges.OrderBy(r => r.Sort).ToList();

                output.Success = true;
                output.Object = dto;
                return output;
            }
            catch (Exception ex)
            {
                output.Success = false;
                output.Error = ex.Message;
                output.Message = "GetDirectoryFacetConfig failed.";
                return output;
            }
        }

        public async Task<ResponseMessageDto> SaveDirectoryFacetConfig(DirectoryFacetConfigDto dto)
        {
            var output = new ResponseMessageDto();

            try
            {
                if (dto == null)
                {
                    output.Success = false;
                    output.Message = "Invalid payload.";
                    return output;
                }

                var websiteId = await loginUserData.GetWebsiteId();
                if (websiteId <= 0)
                {
                    output.Success = false;
                    output.Message = "Website context not found.";
                    return output;
                }

                // 1) 取回 Directory（含 ranges），並確保屬於該網站
                var directory = await db.Directory
                    .Include(d => d.DirectoryFacetRanges)
                    .FirstOrDefaultAsync(d => d.Id == dto.DirectoryId && d.FK_WebsiteId == websiteId);

                if (directory == null)
                {
                    output.Success = false;
                    output.Message = "Directory not found.";
                    return output;
                }

                // 2) 正規化輸入 ranges（null -> empty）
                dto.Ranges ??= new List<DirectoryFacetRangeDto>();

                // 3) 依 UI 順序重寫 Sort（1..N）
                //    （你保留人工排序的核心：就信任前端拖曳後的順序）
                for (int i = 0; i < dto.Ranges.Count; i++)
                {
                    dto.Ranges[i].Sort = i + 1;
                }

                // 4) 驗證 ranges（僅在 FacetType != None 時需要）
                //    目前你只做 Year，但這裡不綁死 enum 值，仍以「有 ranges 就驗」的最小策略
                var validationError = ValidateRanges(dto);
                if (!string.IsNullOrWhiteSpace(validationError))
                {
                    output.Success = false;
                    output.Message = validationError;
                    return output;
                }

                // 5) 更新 Directory 基本欄位
                directory.FacetType = dto.FacetType;
                directory.CalendarType = dto.CalendarType;

                // 6) Diff 更新 ranges
                //    DB（已套 QueryFilter）中只會有未刪除的 ranges
                var existing = directory.DirectoryFacetRanges?.ToList() ?? new List<DirectoryFacetRange>();
                var existingById = existing.ToDictionary(x => x.Id, x => x);

                // dto ids (only >0)
                var incomingIds = dto.Ranges
                    .Where(x => x.DirectoryId > 0)
                    .Select(x => x.DirectoryId)
                    .ToHashSet();

                // 6-1) Update + Add
                foreach (var rDto in dto.Ranges)
                {
                    if (rDto.DirectoryId > 0 && existingById.TryGetValue(rDto.DirectoryId.Value, out var entity))
                    {
                        // Update: map 到已存在 entity（你的 mapper 已 Ignore Id/FK/Navigation）
                        mapper.Map(rDto, entity);

                        // FK 保護（理論上 mapper 忽略 FK，但這裡再保險一次）
                        entity.FK_DirectoryId = directory.Id;
                    }
                    else
                    {
                        // Add: new entity
                        var newEntity = mapper.Map<DirectoryFacetRange>(rDto);
                        newEntity.FK_DirectoryId = directory.Id;
                        db.DirectoryFacetRanges.Add(newEntity);
                    }
                }

                // 6-2) Soft Delete: DB 有、dto 沒有
                foreach (var entity in existing)
                {
                    if (!incomingIds.Contains(entity.Id))
                    {
                        // Soft delete（FullAuditedEntity）
                        db.DirectoryFacetRanges.Remove(entity);
                    }
                }

                await db.SaveChangesAsync();

                // 7) 回傳最新資料（含新增後 Id / Sort）
                //    重新查一次確保 ranges 是最新狀態（且 QueryFilter 會排除已刪除）
                var latest = await db.Directory
                    .Include(d => d.DirectoryFacetRanges)
                    .FirstOrDefaultAsync(d => d.Id == directory.Id && d.FK_WebsiteId == websiteId);

                var latestDto = mapper.Map<DirectoryFacetConfigDto>(latest);
                latestDto.Ranges ??= new List<DirectoryFacetRangeDto>();
                latestDto.Ranges = latestDto.Ranges.OrderBy(x => x.Sort).ToList();

                output.Success = true;
                output.Message = "Saved.";
                output.Object = latestDto;
                return output;
            }
            catch (Exception ex)
            {
                // 建議 log（可選）
                // _logger.LogError(ex, "SaveDirectoryFacetConfig failed. DirectoryId={DirectoryId}", dto?.DirectoryId);

                output.Success = false;
                output.Error = ex.Message;
                output.Message = "SaveDirectoryFacetConfig failed.";
                return output;
            }
        }
        private static string? ValidateRanges(DirectoryFacetConfigDto dto)
        {
            // FacetType=None 時，ranges 可以視為不生效；你也可以選擇直接清空
            // 這裡採最小策略：如果 ranges 有資料就驗，避免存入爛資料
            if (dto.Ranges == null || dto.Ranges.Count == 0) return null;

            // 必填與 Start<=End
            foreach (var r in dto.Ranges)
            {
                if (r.Start <= 0 || r.End <= 0)
                    return "起始/結束 必須為有效年份。";

                if (r.Start > r.End)
                    return "起始年份不可大於結束年份。";
            }

            // 不允許重疊（允許缺口）
            var ordered = dto.Ranges
                .OrderBy(x => x.Start)
                .ThenBy(x => x.End)
                .ToList();

            for (int i = 1; i < ordered.Count; i++)
            {
                var prev = ordered[i - 1];
                var cur = ordered[i];

                // overlap if cur.Start <= prev.End
                if (cur.Start <= prev.End)
                    return "分類區間不可重疊（含相交）。";
            }

            return null;
        }
        public async Task<ResponseMessageDto> GetFacetAsync(long directoryId)
        {
            var res = new ResponseMessageDto { Success = false };

            try
            {
                var websiteId = loginUserData.GetFrontWebsiteId();

                var dir = await db.Directory
                    .Include(d => d.DirectoryFacetRanges)
                    .FirstOrDefaultAsync(d => d.Id == directoryId && d.FK_WebsiteId == websiteId);

                if (dir == null)
                {
                    res.Error = "NOT_FOUND";
                    res.Message = "Directory not found.";
                    return res;
                }

                var dirType = (DirectoryTypeEnum)dir.Type;

                if (dirType != DirectoryTypeEnum.商品 && dirType != DirectoryTypeEnum.文章)
                {
                    res.Success = true;
                    res.Object = new DirectoryGetFacetResponseDto
                    {
                        DirectoryId = directoryId,
                        Items = new List<DirectoryFacetItemDto>() // ✅ 永遠回空陣列，不回 null
                    };
                    return res;
                }

                if (dirType == DirectoryTypeEnum.商品 &&
                    (dir.FacetType == DirectoryFacetTypeEnum.Year ||
                     dir.FacetType == DirectoryFacetTypeEnum.Month ||
                     dir.FacetType == DirectoryFacetTypeEnum.YearMonth))
                {
                    res.Success = true;
                    res.Object = new DirectoryGetFacetResponseDto
                    {
                        DirectoryId = directoryId,
                        Items = new List<DirectoryFacetItemDto>() // ✅ 永遠回空陣列，不回 null
                    };
                    return res;
                }

                // ✅ 先給預設空陣列，避免任何分支漏 assign 變成 null
                var items = new List<DirectoryFacetItemDto>();

                switch (dir.FacetType)
                {
                    case DirectoryFacetTypeEnum.Year:
                    case DirectoryFacetTypeEnum.Month:
                    case DirectoryFacetTypeEnum.YearMonth:
                        {
                            if (dirType != DirectoryTypeEnum.文章)
                                break;

                            var enabledRanges = (dir.DirectoryFacetRanges ?? new List<DirectoryFacetRange>())
                                .Where(r => r.Enabled)
                                .OrderBy(r => r.Sort)
                                .ThenBy(r => r.Id)
                                .Select(r => (Start: r.Start, End: r.End))
                                .ToList();

                            enabledRanges = NormalizeFacetRanges(enabledRanges, dir.FacetType, dir.CalendarType);

                            // 1️、 先取出「此目錄所要求的 Tag 清單」
                            var dirTagIds = await db.Tag_Associates
                                .AsNoTracking()
                                .Where(x => x.Type == TagAssociateTypeEnum.目錄
                                         && x.FK_AId == directoryId)
                                .Select(x => x.FK_TId)
                                .Distinct()
                                .ToListAsync();

                            if (dirTagIds.Count == 0)
                                break;

                            // 2️、 找出「同時擁有全部這些 Tag」的文章
                            var nodeDates = await (
                                from ta in db.Tag_Associates.AsNoTracking()
                                join a in db.Article.AsNoTracking()
                                    on ta.FK_AId equals a.Id
                                where ta.Type == TagAssociateTypeEnum.文章
                                      && dirTagIds.Contains(ta.FK_TId)
                                      && a.FK_WebsiteId == websiteId
                                      && !a.IsDeleted
                                      && a.Visible
                                      && !a.RemovedFromShelves
                                      && a.NodeDate != null
                                group ta by new { a.Id, a.NodeDate } into g
                                // ⭐ 關鍵：文章擁有的目錄 Tag 數量 == 目錄所需 Tag 數量
                                where g.Select(x => x.FK_TId).Distinct().Count() == dirTagIds.Count
                                select g.Key.NodeDate!.Value
                            ).ToListAsync();

                            if (nodeDates.Count == 0)
                                break;


                            items = dir.FacetType switch
                            {
                                DirectoryFacetTypeEnum.Year =>
                                    BuildYearItems(nodeDates, dir.CalendarType, enabledRanges),

                                DirectoryFacetTypeEnum.Month =>
                                    BuildMonthItems(nodeDates, enabledRanges),

                                DirectoryFacetTypeEnum.YearMonth =>
                                    BuildYearMonthItems(nodeDates, dir.CalendarType, enabledRanges),

                                _ => new List<DirectoryFacetItemDto>()
                            };

                            items = items
                                .OrderByDescending(x => x.End)
                                .ThenByDescending(x => x.Start)
                                .ToList();

                            break;
                        }

                    case DirectoryFacetTypeEnum.Tag:
                        {
                            items = await BuildDirectoryTagItemsAsync(websiteId, directoryId);
                            break;
                        }

                    case DirectoryFacetTypeEnum.DocumentType:
                    default:
                        break;
                }

                res.Success = true;
                res.Object = new DirectoryGetFacetResponseDto
                {
                    DirectoryId = directoryId,
                    Items = items ?? new List<DirectoryFacetItemDto>() // ✅ 雙保險
                };
                return res;
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.Error = ex.Message;
                res.Message = "GetFacet failed.";
                return res;
            }
        }

        #region ===== Tag facet =====

        private async Task<List<DirectoryFacetItemDto>> BuildDirectoryTagItemsAsync(long websiteId, long directoryId)
        {
            // 子站（你系統有父子站）
            var siteIds = await db.MappingWebsiteRelationship
                .Where(x => x.FatherId == websiteId && !x.IsDeleted)
                .Select(x => x.Id)
                .ToListAsync();
            siteIds.Add(websiteId);

            // 目錄包含標籤（Type==目錄）
            var tags = await db.Tag_Associates
                .Include(x => x.Tag)
                .Where(x => !x.IsDeleted)
                .Where(x => x.Type == TagAssociateTypeEnum.目錄)
                .Where(x => x.FK_AId == directoryId)
                .Where(x => x.Tag != null)
                .Where(x => siteIds.Contains(x.Tag!.FK_WebsiteId))
                .Select(x => new { TagId = x.FK_TId, TagName = x.Tag!.Title })
                .Distinct()
                .OrderBy(x => x.TagName)
                .ToListAsync();

            return tags.Select(t => new DirectoryFacetItemDto
            {
                Start = (int)t.TagId,
                End = (int)t.TagId,
                Label = string.IsNullOrWhiteSpace(t.TagName) ? t.TagId.ToString() : t.TagName!
            }).ToList();
        }

        #endregion

        #region ===== Date facet builders (fill holes) =====

        // 規則：
        // 1) configuredRanges 保留成段（range 內若完全沒資料 → 不回傳該段）
        // 2) 其他有資料但不在任何 configured range 的 key → 單點補洞（Start=End）
        private static List<DirectoryFacetItemDto> BuildYearItems(
            List<DateTime> nodeDates,
            DirectoryCalendarTypeEnum calendarType,
            List<(int Start, int End)> configuredRanges)
        {
            var years = nodeDates
                .Select(d => calendarType == DirectoryCalendarTypeEnum.民國年 ? (d.Year - 1911) : d.Year)
                .Where(y => y > 0)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            return BuildFilledItems(
                years,
                configuredRanges,
                single => $"{single}",
                (s, e) => $"{s}-{e}"
            );
        }

        private static List<DirectoryFacetItemDto> BuildMonthItems(
            List<DateTime> nodeDates,
            List<(int Start, int End)> configuredRanges)
        {
            var months = nodeDates
                .Select(d => d.Month)
                .Where(m => m >= 1 && m <= 12)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            return BuildFilledItems(
                months,
                configuredRanges,
                single => $"{single:00}",
                (s, e) => $"{s:00}-{e:00}"
            );
        }

        private static List<DirectoryFacetItemDto> BuildYearMonthItems(
            List<DateTime> nodeDates,
            DirectoryCalendarTypeEnum calendarType,
            List<(int Start, int End)> configuredRanges)
        {
            int EncodeYm(DateTime d)
            {
                var y = calendarType == DirectoryCalendarTypeEnum.民國年 ? (d.Year - 1911) : d.Year;
                return (y * 100) + d.Month; // 11201 / 202501
            }

            string FormatYm(int ym)
            {
                var y = ym / 100;
                var m = ym % 100;
                return $"{y}-{m:00}";
            }

            var yms = nodeDates
                .Select(EncodeYm)
                .Where(ym =>
                {
                    var m = ym % 100;
                    return ym > 0 && m >= 1 && m <= 12;
                })
                .Distinct()
                .OrderBy(ym => ym)
                .ToList();

            return BuildFilledItems(
                yms,
                configuredRanges,
                single => FormatYm(single),
                (s, e) => $"{FormatYm(s)}~{FormatYm(e)}"
            );
        }

        private static List<DirectoryFacetItemDto> BuildFilledItems(
            List<int> availableKeys,
            List<(int Start, int End)> configuredRanges,
            Func<int, string> labelSingle,
            Func<int, int, string> labelRange)
        {
            var items = new List<DirectoryFacetItemDto>();
            if (availableKeys == null || availableKeys.Count == 0) return items;

            availableKeys = availableKeys
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // normalize ranges
            var ranges = (configuredRanges ?? new List<(int Start, int End)>())
                .Select(r => (Start: Math.Min(r.Start, r.End), End: Math.Max(r.Start, r.End)))
                .Where(r => r.Start > 0 && r.End > 0)
                .OrderBy(r => r.Start)
                .ThenBy(r => r.End)
                .ToList();

            // 先依「設定的 ranges」組段：只要該段內有任何 key，就輸出整段
            var covered = new HashSet<int>();

            foreach (var r in ranges)
            {
                bool hasAny = false;

                // availableKeys 已排序，用線性掃描即可（這裡用簡單寫法，效能也足夠）
                foreach (var k in availableKeys)
                {
                    if (k < r.Start) continue;
                    if (k > r.End) break;

                    hasAny = true;
                    covered.Add(k);
                }

                if (hasAny)
                {
                    items.Add(new DirectoryFacetItemDto
                    {
                        Start = r.Start,
                        End = r.End,
                        Label = labelRange(r.Start, r.End)
                    });
                }
            }

            // 再補洞：所有不在任何 range 內的 key，輸出單點
            foreach (var k in availableKeys)
            {
                if (covered.Contains(k)) continue;

                items.Add(new DirectoryFacetItemDto
                {
                    Start = k,
                    End = k,
                    Label = labelSingle(k)
                });
            }

            return items;
        }

        private static List<(int Start, int End)> NormalizeFacetRanges(
    List<(int Start, int End)> ranges,
    DirectoryFacetTypeEnum facetType,
    DirectoryCalendarTypeEnum calendarType)
        {
            if (ranges == null || ranges.Count == 0) return new List<(int Start, int End)>();

            // 只在「民國年」模式下需要把 AD -> ROC
            if (calendarType != DirectoryCalendarTypeEnum.民國年)
                return ranges;

            // 小工具：如果已經是 ROC（通常 < 1911），就不要再轉一次
            static int ToRocYearIfAd(int y) => y >= 1912 ? (y - 1911) : y;

            // YearMonth：判斷像 202501 這種 ADYYYYMM 才轉；11201 這種 ROCYYYYMM 不轉
            static int ToRocYmIfAd(int ym)
            {
                if (ym <= 0) return ym;
                var y = ym / 100;
                var m = ym % 100;

                // 粗判斷：AD 年通常 >= 1912，且月份 1..12
                if (y >= 1912 && m >= 1 && m <= 12)
                {
                    var rocY = y - 1911;
                    return (rocY * 100) + m;
                }

                return ym; // 看起來已經是 ROCYYYYMM
            }

            return facetType switch
            {
                DirectoryFacetTypeEnum.Year => ranges
                    .Select(r => (Start: ToRocYearIfAd(r.Start), End: ToRocYearIfAd(r.End)))
                    .ToList(),

                DirectoryFacetTypeEnum.YearMonth => ranges
                    .Select(r => (Start: ToRocYmIfAd(r.Start), End: ToRocYmIfAd(r.End)))
                    .ToList(),

                _ => ranges // Month / 其他：不轉
            };
        }

        #endregion

    }
}