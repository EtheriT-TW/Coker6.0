using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Application.Shared.Dto.Product;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.Product
{
    public partial class ProductAppService
    {
        public async Task<ProductRecommendationsDto?> GetRecommendations(long id, int count = 8)
        {
            count = Math.Clamp(count, 1, 50);
            // 由商品所屬網站決定範圍，但必須先通過本站／允許子站的檢查。
            // 外部只提供商品 ID，不能自行指定網站。
            var rootSiteId = loginUserData.GetFrontWebsiteId();
            if (rootSiteId <= 0) return null;
            var websites = db.Websites.AsNoTracking().Where(w => !w.IsDeleted);
            if (!await websites.AnyAsync(w => w.Id == rootSiteId)) return null;
            var childOrgNames = loginUserData.GetFrontChildOrgName();
            var site = await (from p in db.Prods.AsNoTracking()
                              join w in websites on p.FK_WebsiteId equals w.Id
                              where p.Id == id && !p.IsDeleted
                                  && (w.Id == rootSiteId
                                      || (childOrgNames.Contains(w.OrgName)
                                          && db.MappingWebsiteRelationship.Any(r => !r.IsDeleted
                                              && r.FatherId == rootSiteId && r.WebsiteId == w.Id)))
                              select w).FirstOrDefaultAsync();
            if (site == null) return null;
            var siteId = site.Id;
            var now = DateTime.Now;
            var displayProducts = db.Prods.AsNoTracking().Where(p => p.FK_WebsiteId == siteId
                && !p.IsDeleted && p.Visible && !p.RemovedFromShelves
                && (p.permanent || (p.StartTime <= now && p.EndTime > now)));
            if (!await displayProducts.AnyAsync(p => p.Id == id)) return null;

            var tags = db.Tags.AsNoTracking().Where(t => !t.IsDeleted && !t.IsTemporary && t.FK_WebsiteId == siteId);
            var bindings = db.Tag_Associates.AsNoTracking()
                .Where(b => !b.IsDeleted && b.Type == TagAssociateTypeEnum.商品)
                .Select(b => new { ProductId = b.FK_AId, TagId = b.FK_TId }).Distinct();
            var sourceTags = await (from b in bindings
                                    join t in tags on b.TagId equals t.Id
                                    where b.ProductId == id
                                    select new { t.Id, t.ProductIdfWeight }).ToListAsync();
            var result = new ProductRecommendationsDto { ProductId = id, SimilarityWeight = 1d };
            if (sourceTags.Count == 0) return result;

            var sourceTagIds = sourceTags.Select(t => t.Id).ToArray();
            var sourceWeight = sourceTags.Sum(t => t.ProductIdfWeight);

            // 由目前請求的 Token 識別會員或訪客，不接受外部指定 UUID。
            // 同時讀取已對應的訪客 UUID，涵蓋登入前已彙整而尚未合併的統計。
            var uuid = await tokenAppService.GetUUID();
            var userUuids = new List<Guid>();
            if (uuid != Guid.Empty)
            {
                var memberUuid = await db.MappingOldNewUUID.AsNoTracking()
                    .Where(m => !m.IsDeleted && m.TempUUID == uuid)
                    .Select(m => m.UserUUID).FirstOrDefaultAsync();
                if (memberUuid == Guid.Empty) memberUuid = uuid;
                userUuids = await db.MappingOldNewUUID.AsNoTracking()
                    .Where(m => !m.IsDeleted && m.UserUUID == memberUuid && m.TempUUID != Guid.Empty)
                    .Select(m => m.TempUUID).Distinct().ToListAsync();
                userUuids.Add(memberUuid);
                userUuids.Add(uuid);
            }

            var interests = from stat in db.UserTagStatistics.AsNoTracking()
                            join t in tags on stat.FK_TagId equals t.Id
                            where userUuids.Contains(stat.UUID) && stat.Weight > 0
                            group stat by stat.FK_TagId into g
                            select new { TagId = g.Key, Weight = g.Sum(s => s.Weight) };
            var totalInterest = await interests.SumAsync(s => (double?)s.Weight) ?? 0d;
            result.IsPersonalized = totalInterest > 0;
            result.SimilarityWeight = result.IsPersonalized ? 0.6 : 1d;
            result.InterestWeight = result.IsPersonalized ? 0.4 : 0d;
            var interestDenominator = totalInterest > 0 ? totalInterest : 1d;

            var candidates = displayProducts.Where(p => p.Id != id
                && p.Status != ProdStatusEnum.售完 && p.Status != ProdStatusEnum.停產
                && db.Prod_Stocks.Any(s => s.FK_Pid == p.Id && !s.IsDeleted && s.Visible
                    && !s.IsTimePrice && (p.NoStockManagement || s.Stock > 0))
                && bindings.Any(b => b.ProductId == p.Id && sourceTagIds.Contains(b.TagId)));

            // 彙總、計分、排序與 Take 均留在資料庫，不逐筆查詢或截斷候選池。
            var aggregates = from b in bindings
                             join p in candidates on b.ProductId equals p.Id
                             join t in tags on b.TagId equals t.Id
                             join interest in interests on t.Id equals interest.TagId into userInterests
                             from interest in userInterests.DefaultIfEmpty()
                             group new { t, Interest = (double?)interest.Weight ?? 0d } by new { p.Id, p.Title } into g
                             select new
                             {
                                 g.Key.Id,
                                 g.Key.Title,
                                 TotalWeight = g.Sum(x => x.t.ProductIdfWeight),
                                 CommonWeight = g.Sum(x => sourceTagIds.Contains(x.t.Id) ? x.t.ProductIdfWeight : 0d),
                                 CommonTagCount = g.Count(x => sourceTagIds.Contains(x.t.Id)),
                                 Interest = g.Sum(x => x.Interest)
                             };
            var scores = aggregates.Select(a => new
            {
                a.Id,
                a.Title,
                a.CommonTagCount,
                Similarity = a.CommonWeight / (sourceWeight + a.TotalWeight - a.CommonWeight),
                Interest = a.Interest / interestDenominator
            });
            var similarityWeight = result.SimilarityWeight;
            var interestWeight = result.InterestWeight;
            result.Items = await scores.Select(s => new ProductRecommendationItemDto
            {
                Id = s.Id,
                Title = s.Title,
                CommonTagCount = s.CommonTagCount,
                SimilarityScore = s.Similarity,
                InterestScore = s.Interest,
                Score = s.Similarity * similarityWeight + s.Interest * interestWeight
            }).OrderByDescending(s => s.Score).ThenByDescending(s => s.SimilarityScore)
                .ThenBy(s => s.Id).Take(count).ToListAsync();

            foreach (var item in result.Items)
                item.Link = $"/{Uri.EscapeDataString(string.IsNullOrWhiteSpace(site.OrgName) ? "Page" : site.OrgName)}/search/product/{item.Id}";
            return result;
        }
    }
}
