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

    private async Task ImportProductsAsync(Dictionary<string, SiteLayoutScanner.LayoutPage> pages,
        Dictionary<string, Membership> membership, long websiteId, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        // 重新匯入時用名稱找回原本的商品（比照文章）；同一次匯入內同名的不同商品不會互相覆蓋。
        var byTitle = (await db.Prods.Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted)
                .Select(x => new { x.Id, x.Title }).ToListAsync(token))
            .GroupBy(x => x.Title, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(x => x.Id).ToList(), StringComparer.Ordinal);
        var tagIds = membership.Values.SelectMany(x => x.TagIds).Distinct().ToList();
        var bound = (await db.Tag_Associates.Where(x => !x.IsDeleted && x.Type == TagAssociateTypeEnum.商品 && tagIds.Contains(x.FK_TId))
                .Select(x => new { x.FK_AId, x.FK_TId }).ToListAsync(token))
            .Select(x => (x.FK_AId, x.FK_TId)).ToHashSet();
        var claimed = new HashSet<long>();

        foreach (var (key, page) in pages)
        {
            await checkpoint();
            try
            {
                var member = membership.GetValueOrDefault(key);
                var (content, galleryImages, sideText) = await BuildProductContentAsync(page, websiteId, result, checkpoint, token);
                var (introduction, keepContent) = sideText != null ? (sideText, true)
                    : ChooseIntroduction(member?.Description, content?.Html);
                var id = await SaveProductAsync(page.Article!, introduction, byTitle, claimed, websiteId, result, token);
                foreach (var tagId in member?.TagIds ?? [])
                    if (bound.Add((id, tagId)))
                    {
                        var binding = new Tag_Associate { FK_TId = tagId, FK_AId = id, Type = TagAssociateTypeEnum.商品 };
                        db.Tag_Associates.Add(binding);
                        await user.SaveChanges(binding);
                    }
                // 列表主圖排第一張，目錄卡片用的就是第一張商品圖片。
                if (member?.ImageUrl != null)
                    await html.BindImageAsync(member.ImageUrl, member.ListUrl, id, FileBindTypeEnum.產品, result, checkpoint, token);
                foreach (var image in galleryImages)
                    await html.BindImageAsync(image.AbsoluteUri, page.Url, id, FileBindTypeEnum.產品, result, checkpoint, token);
                // 內文只是簡述那段文字時存空內容：重新匯入要清掉之前重複存的內容。
                await SaveProductContentAsync(id, page, keepContent ? content : null, result);
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

    private async Task<long> SaveProductAsync(SiteArticleInfo info, string? introduction,
        Dictionary<string, List<long>> byTitle, HashSet<long> claimed, long websiteId,
        SiteLayoutResult result, CancellationToken token)
    {
        var title = Truncate(info.Title, MaxProductTitleLength);
        var existingId = byTitle.GetValueOrDefault(title)?.FirstOrDefault(id => !claimed.Contains(id));
        if (existingId is > 0)
        {
            claimed.Add(existingId.Value);
            var existing = await db.Prods.FirstAsync(x => x.Id == existingId.Value, token);
            if (introduction != null && existing.Introduction != introduction)
            {
                existing.Introduction = introduction;
                await user.SaveChanges(existing);
            }
            result.ProductsUpdated++;
            return existing.Id;
        }

        var product = new Prod
        {
            FK_WebsiteId = websiteId,
            Title = title,
            Introduction = introduction ?? "",
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
        // 無規格（規格 id 0）、時價：和後台新增商品不選規格、勾「時價」存出來的資料相同。
        var stock = new Prod_Stock
        {
            FK_Pid = product.Id,
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
        claimed.Add(product.Id);
        result.ProductsCreated++;
        return product.Id;
    }

    // 相簿圖（a[href=#modalOfSwiper] 與縮圖）改存商品圖片，從內文拿掉；內文其他圖片照舊留在內容裡。
    // 已沒有相簿，BuildAsync 只會清理內文並拿掉舊站標題（商品名稱已存在商品名稱欄位）。
    // sideText：相簿右側只有文字時的簡述（已從內文拿掉）；右側有圖片、太長或沒有相簿時為 null，內容照舊留在產品說明。
    // 相簿右側有圖片時回傳空字串，重新匯入會清掉之前的簡述。
    private async Task<(ProductContent? Content, List<Uri> GalleryImages, string? SideText)> BuildProductContentAsync(
        SiteLayoutScanner.LayoutPage page, long websiteId, SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var content = SiteContentImporter.PrepareContent(page.Content!);
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

    private async Task SaveProductContentAsync(long id, SiteLayoutScanner.LayoutPage page, ProductContent? content,
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
    }
}
