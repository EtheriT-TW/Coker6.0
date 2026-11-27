using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Core.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtheriT.Coker.Application.SiteCrawl;

// 商品匯入（使用者 2026-11-17 決定）：順序和文章相同，商品建在商品管理的商品列表。
// 舊站型錄商品沒有價格也沒有規格：每個商品建一筆無規格、時價、不管庫存的規格資料（前台顯示「時價」、不能加入購物車）。
// 圖片：列表主圖與詳細頁相簿圖存成商品圖片（前台商品頁圖片區），其餘內文存成商品內容。
// 簡述（使用者 2026-11-17 指定）：新站商品簡述只能放文字（前台在商品圖片旁逐行顯示）。
// 舊站相簿右側（標題後到 clear／下一個版面列前）只有文字 → 填入簡述；有文字也有圖片 → 整段留在產品說明（商品內容）。
// 相簿右側沒有內容時才用舊站列表說明，且同一段文字只存一份（ChooseIntroduction）。
public sealed partial class SiteArticleImporter
{
    internal const string ProductPurposeCode = "product-import-directory";
    private const int MaxProductTitleLength = 150;
    private const int MaxIntroductionLength = 3000;
    private const int DefaultProductSerNo = 500;

    private sealed record ProductContent(string Html, string Css);

    private static readonly ImportKind ProductKind = new("商品", DirectoryTypeEnum.商品, ProductPurposeCode, "商品匯入目錄",
        "//*[@data-import-role='product-directory']", CatalogFrameMarker);

    // 第一輪只建商品（取得新站 Id）並記下新站網址，第二輪才寫內文，連到其他商品／文章的連結才改寫得到。
    // 簡述要看內文才知道，第一輪先不動（新商品是空白、既有商品保留原本的），第二輪再寫。
    private async Task<Dictionary<string, DetailTarget>> CreateProductsAsync(Dictionary<string, SiteLayoutScanner.LayoutPage> pages,
        Dictionary<string, Membership> membership, long websiteId, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var existing = await db.Prods.AsNoTracking().Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted)
            .Select(x => new { x.Id, x.Title }).OrderBy(x => x.Id).ToListAsync(token);
        var existingIds = existing.Select(x => x.Id).ToHashSet();
        var byTitle = existing.ToLookup(x => x.Title, x => x.Id, StringComparer.Ordinal);
        var claimed = ReservedDetailIds(result, pages.Values, existingIds, isProduct: true);
        var ids = new Dictionary<string, DetailTarget>(StringComparer.Ordinal);
        foreach (var (key, page) in pages)
        {
            await checkpoint();
            try
            {
                var member = membership.GetValueOrDefault(key);
                var target = await ResolveProductAsync(page, member, byTitle, existingIds, claimed, websiteId, result);
                ids[key] = target;
                SiteCrawlHtml.RegisterDetail(result, page.Url, member?.ListUrl, isProduct: true, target.Id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException && !token.IsCancellationRequested)
            {
                logger.LogError(ex, "Site crawl product save failed for {Url}", page.Url);
                result.AddError($"商品「{page.Article!.Title}」匯入失敗，請查看 MVC 日誌。");
            }
        }
        return ids;
    }

    private async Task FillProductsAsync(Dictionary<string, SiteLayoutScanner.LayoutPage> pages, Dictionary<string, DetailTarget> ids,
        Dictionary<string, Membership> membership, long websiteId, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var tagIds = membership.Values.SelectMany(x => x.TagIds).Distinct().ToList();
        var bound = (await db.Tag_Associates.Where(x => !x.IsDeleted && x.Type == TagAssociateTypeEnum.商品 && tagIds.Contains(x.FK_TId))
                .Select(x => new { x.FK_AId, x.FK_TId }).ToListAsync(token))
            .Select(x => (x.FK_AId, x.FK_TId)).ToHashSet();

        foreach (var (key, page) in pages)
        {
            if (!ids.TryGetValue(key, out var target)) continue;
            var id = target.Id;
            await checkpoint();
            try
            {
                var member = membership.GetValueOrDefault(key);
                var query = db.Prods.Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted && x.Id == id);
                var contentQuery = query.AsNoTracking().Select(x => new SiteCrawlContentSnapshot(x.Html, x.Css, x.SaveHtml, x.SaveCss));
                var current = await contentQuery.SingleOrDefaultAsync(token);
                if (current == null) continue;
                var title = page.Article!.Title;
                var decision = result.DecideImport(target.Previous, target.SourceHash, current, $"商品「{title}」");
                if (decision == SiteCrawlImportDecision.Unchanged) continue;
                var (content, galleryImages, sideText) = await BuildProductContentAsync(page, websiteId, result, checkpoint, token);
                var (introduction, keepContent) = sideText != null ? (sideText, true)
                    : ChooseIntroduction(member?.Description, content?.Html);
                var markup = keepContent ? content : null;
                var latest = await contentQuery.SingleOrDefaultAsync(token);
                if (decision == SiteCrawlImportDecision.Conflict || !result.CheckContentUnchanged(current, latest, $"商品「{title}」"))
                {
                    if (latest != null && target.StateKey is { } stateKey)
                    {
                        await CompleteProductAsync(page, target, query, introduction, member?.ListUrl, result, contentWritten: false);
                        await result.StageConflictAsync(SiteCrawlConflictKind.Product, id, stateKey, page.Url, target.SourceHash,
                            title, latest, target.Previous, markup?.Html ?? "", markup?.Css ?? "", Truncate(title, MaxProductTitleLength));
                    }
                    continue;
                }
                // 商品已取得 Id 但匯入中斷時，可能還沒建立預設規格，沿用同一個商品補齊。
                if (target.Previous is { SourceHash: null, WrittenFields: not null }
                    && !await db.Prod_Stocks.AnyAsync(x => !x.IsDeleted && x.FK_Pid == id
                        && x.Prod!.FK_WebsiteId == websiteId && !x.Prod.IsDeleted, token))
                    await CreateDefaultStockAsync(id);
                foreach (var tagId in member?.TagIds ?? [])
                    if (bound.Add((id, tagId)))
                    {
                        var binding = new Tag_Associate { FK_TId = tagId, FK_AId = id, Type = TagAssociateTypeEnum.商品 };
                        db.Tag_Associates.Add(binding);
                        await user.SaveChanges(binding);
                        result.DirectoryBindingsChanged = true;
                    }
                // 列表主圖排第一張，目錄卡片用的就是第一張商品圖片。
                if (member?.ImageUrl != null)
                    await html.BindImageAsync(member.ImageUrl, member.ListUrl, id, FileBindTypeEnum.產品, result, checkpoint, token);
                foreach (var image in galleryImages)
                    await html.BindImageAsync(image.AbsoluteUri, page.Url, id, FileBindTypeEnum.產品, result, checkpoint, token);
                latest = await contentQuery.SingleOrDefaultAsync(token);
                if (!result.CheckContentUnchanged(current, latest, $"商品「{title}」"))
                {
                    if (latest != null && target.StateKey is { } stateKey)
                    {
                        await CompleteProductAsync(page, target, query, introduction, member?.ListUrl, result, contentWritten: false);
                        await result.StageConflictAsync(SiteCrawlConflictKind.Product, id, stateKey, page.Url, target.SourceHash,
                            title, latest, target.Previous, markup?.Html ?? "", markup?.Css ?? "", Truncate(title, MaxProductTitleLength));
                    }
                    continue;
                }
                // 內文只是簡述那段文字時存空內容：重新匯入要清掉之前重複存的內容。
                if (!await SaveProductContentAsync(id, page, markup, result)) continue;
                await CompleteProductAsync(page, target, query, introduction, member?.ListUrl, result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException && !token.IsCancellationRequested)
            {
                logger.LogError(ex, "Site crawl product import failed for {Url}", page.Url);
                result.AddError($"商品「{page.Article!.Title}」匯入失敗，請查看 MVC 日誌。");
            }
        }
    }

    // 舊站列表說明常和詳細頁內文一字不差（例 sf2 紅藜棒），兩邊都存的話前台商品頁會出現兩次：
    // 內文就只有這段說明 → 存成簡述（前台放在商品圖片旁，和舊站詳細頁圖文並排一致），內容不存；
    // 內文包含這段說明又有其他內容（圖片、更多段落）→ 只存內容，簡述留空。
    // 回傳 null 簡述代表列表沒有說明，重新匯入時不動原本的簡述。
    private static (string? Introduction, bool KeepContent) ChooseIntroduction(string? description, string? contentHtml)
    {
        if (string.IsNullOrWhiteSpace(description)) return (null, true);
        var introduction = Truncate(description.Trim(), MaxIntroductionLength);
        if (contentHtml == null) return (introduction, true);
        var document = new HtmlDocument();
        document.LoadHtml(contentHtml);
        var text = Compact(HtmlEntity.DeEntitize(document.DocumentNode.InnerText));
        var summary = Compact(description).TrimEnd('.', '…');
        if (summary.Length == 0 || !text.Contains(summary, StringComparison.Ordinal)) return (introduction, true);
        var onlySummary = text == summary && document.DocumentNode.SelectSingleNode(VisibleMedia) == null;
        return onlySummary ? (introduction, false) : ("", true);
    }

    // 只有隱藏欄位的表單（例 舊站相簿用的 #myMedia）看不到，不算內容。
    private const string VisibleMedia =
        "//img|//iframe|//video|//table|//form[.//textarea or .//select or .//input[not(@type='hidden')]]";

    private static string Compact(string value) => string.Concat(value.Where(c => !char.IsWhiteSpace(c)));

    private async Task<DetailTarget> ResolveProductAsync(SiteLayoutScanner.LayoutPage page, Membership? member,
        ILookup<string, long> byTitle, HashSet<long> existingIds, HashSet<long> claimed, long websiteId, SiteLayoutResult result)
    {
        var title = Truncate(page.Article!.Title, MaxProductTitleLength);
        var target = ResolveDetail(page, member, title, byTitle, existingIds, claimed, result, isProduct: true);
        if (target.Id > 0) return target;
        var product = new Prod
        {
            FK_WebsiteId = websiteId,
            Title = title,
            Introduction = "",
            Description = "",
            Ser_No = DefaultProductSerNo,
            permanent = true,
            Visible = true,
            RemovedFromShelves = false,
            NoStockManagement = true,
            Status = ProdStatusEnum.一般
        };
        db.Prods.Add(product);
        await user.SaveChanges(product);
        if (target.StateKey is { } key)
            await result.RecordImportTargetAsync(key, product.Id, page.Url, page.Article.Title, true, product.Title,
                new(Introduction: product.Introduction));
        await CreateDefaultStockAsync(product.Id);
        result.ProductsCreated++;
        return target with { Id = product.Id, Created = true };
    }

    private async Task CreateDefaultStockAsync(long productId)
    {
        // 無規格（規格 id 0）、時價：和後台新增商品不選規格、勾「時價」存出來的資料相同。
        var stock = new Prod_Stock
        {
            FK_Pid = productId,
            FK_S1id = 0,
            FK_S2id = 0,
            Price = 0,
            IsTimePrice = true,
            Stock = null,
            Min_Qty = 1,
            Alert_Qty = 0,
            PackingPoint = 1,
            SubItemNo = "",
            Visible = true
        };
        db.Prod_Stocks.Add(stock);
        await user.SaveChanges(stock);
    }

    // 相簿圖（a[href=#modalOfSwiper] 與縮圖）改存商品圖片，從內文拿掉；內文其他圖片照舊留在內容裡。
    // 已沒有相簿，BuildAsync 只會清理內文並拿掉舊站標題（商品名稱已存在商品名稱欄位）。
    // sideText：相簿右側只有文字時的簡述（已從內文拿掉）；右側有圖片、太長或沒有相簿時為 null，內容照舊留在產品說明。
    // 相簿右側有圖片時回傳空字串，重新匯入會清掉之前的簡述。
    private async Task<(ProductContent? Content, List<Uri> GalleryImages, string? SideText)> BuildProductContentAsync(
        SiteLayoutScanner.LayoutPage page, long websiteId, SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var content = SiteContentImporter.PrepareContent(page.Content!);
        result.ContentSource = SiteLayoutResult.SourceName(page.Article?.Title ?? page.Title, page.Url, "商品");
        var hasGallery = SiteImportTemplates.HasGallery(content);
        var images = new List<Uri>();
        foreach (var image in SiteImportTemplates.TakeGalleryImages(content))
        {
            var source = SiteCrawlHtml.ImageSource(image);
            if (!string.IsNullOrWhiteSpace(source) && Uri.TryCreate(page.Url, source, out var url)
                && url.Scheme is "http" or "https")
                images.Add(url);
        }
        var sideText = hasGallery ? TakeSideText(content) : null;
        var built = await gallery.BuildAsync(content, page.Url, websiteId, isArticle: true, page.SubTitle, result, checkpoint, token);
        return (built == null ? null : new ProductContent(built.Html, built.Css), images, sideText);
    }

    // 先在複本上切出相簿右側，確定只有文字才真的從內文拿走；有圖片就不動內文。
    private static string? TakeSideText(HtmlNode content)
    {
        var probe = SiteImportTemplates.TakeIntroduction(content.CloneNode(true), takeTitle: true, takeText: true).Introduction;
        if (probe == null) return null;
        if (probe.SelectSingleNode(SideMedia) != null) return "";
        var text = PlainLines(probe);
        if (text.Length == 0 || text.Length > MaxIntroductionLength) return null;
        SiteImportTemplates.TakeIntroduction(content, takeTitle: true, takeText: true);
        return text;
    }

    private const string SideMedia = ".//img|.//iframe|.//video|.//audio|.//object|.//embed|.//svg|.//table";

    // 前台商品頁把簡述依換行拆成一行一項，所以保留舊站的換行（<br>、段落、清單項目），其他空白合併。
    private static string PlainLines(HtmlNode node)
    {
        var text = new System.Text.StringBuilder();
        foreach (var current in node.Descendants())
        {
            if (current.NodeType == HtmlNodeType.Text)
                text.Append(HtmlEntity.DeEntitize(current.InnerText));
            else if (current.Name is "br" or "p" or "div" or "li" or "ul" or "ol" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "tr")
                text.Append('\n');
        }
        return string.Join('\n', text.ToString().Split('\n')
            .Select(line => System.Text.RegularExpressions.Regex.Replace(line, "\\s+", " ").Trim())
            .Where(line => line.Length > 0));
    }

    private async Task<bool> SaveProductContentAsync(long id, SiteLayoutScanner.LayoutPage page, ProductContent? content,
        SiteLayoutResult result)
    {
        var response = await products.ImportConten(new ProdSaveContenDto
        {
            Id = checked((int)id),
            SaveHtml = content?.Html ?? "",
            SaveCss = content?.Css ?? ""
        });
        if (!response.Success)
        {
            logger.LogWarning("Site crawl product content save failed for {ProductId}: {Reason}", id, response.Error);
            result.AddError($"商品「{page.Article!.Title}」的內容儲存失敗，請查看 MVC 日誌。");
        }
        return response.Success;
    }

    private async Task CompleteProductAsync(SiteLayoutScanner.LayoutPage page, DetailTarget target, IQueryable<Prod> query,
        string? introduction, Uri? sourceList, SiteLayoutResult result, bool contentWritten = true)
    {
        if (!contentWritten && target.Previous == null) return;
        var saved = await query.AsNoTracking()
            .Select(x => new { x.Html, x.Css, x.SaveHtml, x.SaveCss, x.Title, x.Introduction }).SingleOrDefaultAsync();
        if (saved == null) return;
        var previous = target.Previous;
        var sourceTitle = page.Article!.Title;
        var title = contentWritten && previous != null && SiteCrawlReimport.ShouldUpdate(sourceTitle, previous.SourceTitle, saved.Title, previous.WrittenTitle)
            ? Truncate(sourceTitle, MaxProductTitleLength) : saved.Title;
        var newIntroduction = introduction ?? "";
        var updateIntroduction = (introduction != null || previous?.SourceFields?.Introduction != null)
            && ShouldWriteField(newIntroduction, saved.Introduction, previous, x => x.Introduction);
        if (title != saved.Title || updateIntroduction)
        {
            var entity = await query.SingleAsync();
            if (title != saved.Title) entity.Title = title;
            if (updateIntroduction) entity.Introduction = newIntroduction;
            await user.SaveChanges(entity);
        }
        if (!target.Created && (contentWritten || updateIntroduction)) result.ProductsUpdated++;
        if (target.StateKey is not { } key) return;
        var writtenTitle = previous == null || title != saved.Title ? title : previous.WrittenTitle;
        var writtenIntroduction = updateIntroduction ? newIntroduction
            : previous == null ? saved.Introduction : previous.WrittenFields?.Introduction;
        var writtenFields = new SiteCrawlFields(Introduction: writtenIntroduction);
        var sourceFields = new SiteCrawlFields(Introduction: introduction);
        if (!contentWritten)
        {
            if (previous?.WrittenFields != null)
                result.ImportState?.RecordFields(key, target.Id, writtenFields, sourceFields, sourceList);
            return;
        }
        result.ImportState?.RecordWritten(key, target.Id, page.Url, sourceTitle, target.SourceHash,
            new(SiteCrawlFingerprint.ComputeWritten(saved.Html, saved.Css), writtenTitle, writtenFields,
                SiteCrawlFingerprint.ComputeWritten(saved.SaveHtml, saved.SaveCss)),
            sourceFields, sourceListUrl: sourceList?.AbsoluteUri);
    }
}
