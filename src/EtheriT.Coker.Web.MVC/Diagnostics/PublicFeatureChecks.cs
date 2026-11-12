using System.Text.Json;
using Microsoft.Playwright;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public sealed record PublicFeatureEvidence(string Title, int Checked, int Failed, int Pending)
{
    public string? Reason { get; init; }
}
public sealed record PublicFeatureDefinition(string Id, string Title, string Selector, string? RequiredFeature = null, bool Isolated = false);
public sealed record PublicFeatureActivation(string Id, string Signature);

public static class PublicFeatureChecks
{
    internal static readonly PublicFeatureDefinition[] Definitions =
    {
        new("directoryGetData", "目錄資料、排序、過濾與分頁", ".catalog_frame,.menu_directory,.advertise_directory", "CustomPage"),
        new("swiper", "輪播切換", ".swiper:not(.marqueeSwiper)"),
        new("marqueeSwiper", "跑馬燈資料與切換", ".marqueeSwiper"),
        new("frameInit", "圖片排版與影片開關", ".masonry,.YTmodal_frame,.imageList.layout-masonry"),
        new("viewTypeChange", "列表版型切換", ".type_change_frame"),
        new("categoryFilter", "分類篩選", ".filter-btn"),
        new("hoverMask", "滑入與離開效果", ".hover_mask"),
        new("sitemap", "網站地圖階層與連結", ".sitemap_hierarchical_frame", "CustomPage"),
        new("linkWithIcon", "檔案圖示與下載設定", ".link_with_icon"),
        new("anchorPoint", "頁內導覽與定位", ".anchor_directory,.anchor_title"),
        new("shareBlock", "分享操作與網址", ".shareBlock,.btn_share"),
        new("flipTimer", "倒數更新與到期處理", ".flipdown"),
        new("articleTags", "文章標籤與搜尋連結", ".article-tags", "Article"),
        new("contactForm", "表單欄位與驗證", ".ContactForm,.DynamicForm", "CustomPage"),
        new("remoteTracking", "追蹤資料建構（攔截不送出）", "body"),
        new("bgCanvas", "背景動畫與尺寸變更", ".BGCanvas"),
        new("flipBook", "電子書開啟、翻頁與縮放", ".FlipBookItem"),
        new("mapMessage", "互動圖區與訊息切換", ".MapMessage"),
        new("geolocation", "定位允許與拒絕處理（模擬回呼）", ".getlatlng"),
        new("mobileNavbarMove", "行動導覽與展開收合", "#layout4 #NavbarContent,#lanBar"),
        new("sideFloating", "側邊工具展開與關閉", "#Floating_Center,.side-floating-item"),
        new("search", "搜尋輸入、分類與結果", ".input_sear,.search-input,#SearchInput,.searchblock input", "CustomPage"),
        new("quickCartUi", "快速購物車規格與數量控制（隔離商品）", "[data-quick-cart-product-id],.btn_cart,[data-bs-target='#ShoppingCarModal']", "ProductList"),
        new("quickCart", "購物車新增、數量與刪除（隔離資料）", ".btn_cart,.btn_addToCar,[data-bs-target='#ShoppingCarModal']", "ProductList", true),
        new("addOn", "加價購資格與金額（隔離資料）", "[data-product-addon],.cart-additional-item", "ProductList", true),
        new("cart", "結帳優惠與總額（隔離資料）", "#BuyStepSwiper", "OrderList", true),
        new("bonus", "紅利餘額與折抵（隔離資料）", ".bonus-part,.priceline .bonus,[data-subtotal_bonus],#MemberBonus", "MemberList", true),
        new("payment", "付款種類與金額條件（隔離資料）", "#BuyStepSwiper,[name='paymentType'],[name='PaymentType']", "OrderList", true),
        new("noLink", "無連結選單與事件傳遞", "a.is-no-link")
    };

    internal static string ReadScript(string name)
    {
        using var stream = typeof(PublicFeatureChecks).Assembly.GetManifestResourceStream("Coker.Diagnostics." + name)
            ?? throw new InvalidOperationException("Missing public feature script.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    internal static string ObservationScript => ReadScript("PublicFeatureObserver.js");

    internal static PublicFinding Outcome(PublicFeatureEvidence evidence, string source)
    {
        var state = evidence.Failed > 0 ? PublicCheckStatus.Failed : evidence.Checked == 0 || evidence.Pending > 0
            ? PublicCheckStatus.NotRun : PublicCheckStatus.Passed;
        var message = state == PublicCheckStatus.Failed ? "操作檢測未通過。" : state == PublicCheckStatus.NotRun ? "尚未完成檢測。" : "操作驗證通過。";
        message += evidence.Reason ?? (state == PublicCheckStatus.Passed ? $"已核對 {evidence.Checked} 項適用操作。" : "元件未就緒、樣本不足或操作結果不符。");
        return new(evidence.Title, source, state, message);
    }

    public static async Task<List<PublicFinding>> RunAsync(IPage page, PublicTarget target, CancellationToken token,
        IReadOnlySet<string>? enabledFeatures = null,
        Func<string, CancellationToken, Task<PublicFinding>>? isolated = null,
        Func<IReadOnlyList<PublicFinding>, Task>? publish = null, ILogger? logger = null)
    {
        var source = PublicPageChecks.Display(new Uri(target.Url));
        var idPrefix = "feature/" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(target.Url)))[..16] + "/";
        var eligible = Definitions.Where(item => (item.RequiredFeature == null || enabledFeatures?.Contains(item.RequiredFeature) == true)
            && (!(item.Id is "cart" or "bonus" or "addOn" or "quickCart") || enabledFeatures?.Contains("ProductList") == true)).ToArray();
        var script = ReadScript("PublicFeatureWorkflows.js");
        var rows = new Dictionary<string, PublicFinding>();
        var completed = new Dictionary<string, string>();
        List<PublicFeatureEvidence>? core = null;
        // Directory rendering can introduce more components after the initial HTML.
        for (var round = 0; round < 3; round++)
        {
            token.ThrowIfCancellationRequested();
            var detected = (await page.EvaluateAsync<PublicFeatureActivation[]>(script, new { Phase = "detect", Definitions = eligible }))
                .ToDictionary(item => item.Id, item => item.Signature);
            var selected = eligible.Where(item => detected.ContainsKey(item.Id)).ToArray();
            foreach (var feature in selected)
                rows.TryAdd(feature.Id, new(feature.Title, source, PublicCheckStatus.NotRun, "等待此功能的操作檢測。") { Id = idPrefix + feature.Id });
            if (publish != null) await publish(rows.Values.ToList());
            var work = selected.Where(item => !completed.TryGetValue(item.Id, out var signature) || signature != detected[item.Id]).ToArray();
            if (work.Length == 0) break;
            foreach (var feature in work)
            {
                token.ThrowIfCancellationRequested();
                var previousFinding = rows.GetValueOrDefault(feature.Id);
                try
                {
                    if (feature.Isolated)
                        rows[feature.Id] = isolated == null ? new(feature.Title, source, PublicCheckStatus.NotRun, "隔離執行環境未就緒。")
                            : await isolated(feature.Id, token);
                    else if (feature.Id == "search")
                        rows[feature.Id] = Outcome(await SearchWorkflowAsync(page, target, feature, script, token), source);
                    else if (feature.Id is "swiper" or "viewTypeChange" or "categoryFilter")
                    {
                        if (core == null)
                        {
                            var snapshot = await page.EvaluateAsync<JsonElement>(ReadScript("PublicFeatureLegacy.js"));
                            core = JsonSerializer.Deserialize<List<PublicFeatureEvidence>>(snapshot.GetProperty("Evidence").GetRawText())!;
                        }
                        rows[feature.Id] = Outcome(core.FirstOrDefault(item => item.Title == feature.Title)
                            ?? new(feature.Title, 0, 0, 1) { Reason = "元件尚未建立或沒有可操作資料。" }, source);
                    }
                    else
                    {
                        var viewport = page.ViewportSize;
                        try
                        {
                            if (feature.Id == "mobileNavbarMove")
                                await page.SetViewportSizeAsync(390, 844);
                            var evidence = await page.EvaluateAsync<PublicFeatureEvidence>(script,
                                new { Phase = "run", feature.Id, feature.Title, feature.Selector });
                            rows[feature.Id] = Outcome(evidence, source);
                        }
                        finally
                        {
                            if (feature.Id == "mobileNavbarMove" && viewport != null)
                                await page.SetViewportSizeAsync(viewport.Width, viewport.Height);
                        }
                    }
                }
                catch (PlaywrightException) when (!token.IsCancellationRequested)
                { rows[feature.Id] = new(feature.Title, source, PublicCheckStatus.Failed, "執行時頁面、元件或操作發生錯誤，請檢查此功能。"); }
                catch (Exception error) when (error is not OperationCanceledException && !token.IsCancellationRequested)
                {
                    logger?.LogWarning(error, "Public feature failed for {Feature}", feature.Id);
                    rows[feature.Id] = new(feature.Title, source, PublicCheckStatus.Failed, "此項功能操作或隔離環境發生錯誤，請聯絡系統維護人員。");
                }
                if (previousFinding?.Status == PublicCheckStatus.Failed && rows[feature.Id].Status != PublicCheckStatus.Failed)
                    rows[feature.Id] = previousFinding; // A replaced component must not hide an earlier failed instance.
                completed[feature.Id] = detected[feature.Id];
                rows[feature.Id] = rows[feature.Id] with { Id = idPrefix + feature.Id };
                if (publish != null) await publish(rows.Values.ToList());
            }
            core = null;
        }
        var finalActivation = await page.EvaluateAsync<PublicFeatureActivation[]>(script, new { Phase = "detect", Definitions = eligible });
        foreach (var activation in finalActivation.Where(item => !completed.TryGetValue(item.Id, out var signature) || signature != item.Signature))
        {
            var definition = eligible.Single(item => item.Id == activation.Id);
            if (rows.GetValueOrDefault(activation.Id)?.Status == PublicCheckStatus.Failed) continue;
            rows[activation.Id] = new(definition.Title, source, PublicCheckStatus.NotRun, "頁面持續新增元件，已達本頁觀察上限，新增實例尚未驗證。")
                { Id = idPrefix + definition.Id };
        }
        if (rows.Count == 0) rows["none"] = new("功能檢測範圍", source, PublicCheckStatus.NotRun,
            "本次未觸發適用的功能；不代表本頁已全部通過。");
        return rows.Values.ToList();
    }

    private static async Task<PublicFeatureEvidence> SearchWorkflowAsync(IPage original, PublicTarget target,
        PublicFeatureDefinition feature, string script, CancellationToken token)
    {
        // A separate page exercises the real navigation without destroying the
        // original page's other feature instances or sharing a customer's login.
        var page = await original.Context.NewPageAsync();
        try
        {
            var response = await page.GotoAsync(target.Url, new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15000 });
            if (response == null || response.Status >= 400)
                return new(feature.Title, 0, 1, 0) { Reason = "搜尋起始頁無法載入。" };
            var input = page.Locator(string.Join(",", feature.Selector.Split(',').Select(selector => selector + ":visible"))).First;
            var keyword = "CokerDiagnosticsNoMatch" + Guid.NewGuid().ToString("N");
            await input.FillAsync(keyword, new() { Timeout = 5000 });
            var selectedCategory = await input.EvaluateAsync<string?>(@"element => {
                let root=element.parentElement;
                while(root && root!==document.body && !root.querySelector('.dropdown-menu button[data-id]'))root=root.parentElement;
                if(!root || root===document.body)return null;
                const buttons=[...root.querySelectorAll('.dropdown-menu button[data-id]')];
                const button=buttons.find(item=>Number(item.dataset.id)>0)||buttons[0];
                if(!button)return null;button.click();return button.dataset.id;
            }");
            var button = page.Locator(".btn_sear:visible").First;
            if (selectedCategory != null) { /* Category selection performs the real search navigation. */ }
            else if (await button.CountAsync() > 0) await button.ClickAsync(new() { Timeout = 5000 });
            else
            {
                await input.PressAsync("Enter");
                await input.PressAsync("Tab"); // Change-driven search inputs.
            }
            await page.WaitForURLAsync(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && PublicPageChecks.SameOrigin(new Uri(target.Url), uri)
                && Uri.UnescapeDataString(uri.AbsolutePath).Contains(keyword, StringComparison.Ordinal),
                new() { Timeout = 8000, WaitUntil = WaitUntilState.DOMContentLoaded });
            token.ThrowIfCancellationRequested();
            if (selectedCategory != null && !new Uri(page.Url).AbsolutePath.Contains("/Search/Get/" + selectedCategory + "/", StringComparison.OrdinalIgnoreCase))
                return new(feature.Title, 0, 1, 0) { Reason = "搜尋分類與實際目的地不一致。" };
            var evidence = await page.EvaluateAsync<PublicFeatureEvidence>(script,
                new { Phase = "run", feature.Id, feature.Title, feature.Selector, Keyword = keyword });
            return evidence with { Checked = evidence.Checked + 1 };
        }
        finally { await page.CloseAsync(); }
    }
}
