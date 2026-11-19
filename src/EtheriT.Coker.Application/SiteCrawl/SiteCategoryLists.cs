using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.SiteCrawl;

// 舊站分類頁的「子分類列表」（2026-11-17 superiorlink 實測）：產品資訊頁用 JS（getImgList type=folder）列出子分類卡片，
// 每張連到只有 sub_id 的分類頁（例 index.asp?au_id=28&sub_id=214），子分類頁自己可能再列一層子分類。
// 這些分類頁都會建成選單，所以分類頁在新站改用選單目錄列出子選單，不建空的文章目錄。
internal static class SiteCategoryLists
{
    private const int MaxDirectoryTitleLength = 60;

    internal sealed record ListedCategory(string ParentKey, int Index);

    // 每個子分類頁 → 列出它的分類頁與在列表上的順序。子分類列表只列直屬的下一層，同一頁出現在兩個列表時取第一個。
    // 子分類在掃描結束時已從 List 拿掉（SiteLayoutScanner.ResolveSubPages），要看 SubCategories。
    internal static Dictionary<string, ListedCategory> ListedCategories(IEnumerable<SiteLayoutScanner.LayoutPage> pages)
    {
        var listed = new Dictionary<string, ListedCategory>(StringComparer.Ordinal);
        foreach (var page in pages)
        {
            var parentKey = SiteLayoutScanner.PageKey(page.Url);
            var index = 0;
            foreach (var url in page.SubCategories ?? [])
            {
                var key = SiteLayoutScanner.PageKey(url);
                if (key != parentKey) listed.TryAdd(key, new ListedCategory(parentKey, index++));
            }
        }
        return listed;
    }

    // 分類頁：列表上有子分類，剩下的項目（頁數上限沒掃到的）也都只有 sub_id、不是文章或商品。
    // 剩下的有真正的文章或商品時照舊當列表，子分類照樣是選單。
    internal static bool IsCategoryPage(SiteLayoutScanner.LayoutPage page) =>
        page.SubCategories is { Count: > 0 }
        && (page.List == null || page.List.Items.All(item => SiteArticleDetector.IsSubPageKey(item.Key)));

    // 選單類型目錄（FK_Mid＝分類選單）由前台 .menu_directory 元件列出它底下的子選單；同一個選單重新匯入時沿用。
    internal static async Task<Core.Models.Directory> EnsureMenuDirectoryAsync(CokerDbContext db, LoginUserData user,
        long websiteId, long menuId, string title, SiteLayoutResult result, CancellationToken token)
    {
        var directory = await db.Directory.FirstOrDefaultAsync(x => x.FK_WebsiteId == websiteId && !x.IsDeleted
            && x.Type == (int)DirectoryTypeEnum.選單 && x.FK_Mid == menuId, token);
        if (directory != null) return directory;
        directory = new Core.Models.Directory
        {
            FK_WebsiteId = websiteId,
            Title = title.Length <= MaxDirectoryTitleLength ? title : title[..MaxDirectoryTitleLength],
            Type = (int)DirectoryTypeEnum.選單,
            FK_Mid = menuId,
            Visible = true
        };
        db.Directory.Add(directory);
        await user.SaveChanges(directory);
        result.DirectoriesCreated++;
        return directory;
    }
}
