using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Marketing;
using EtheriT.Coker.Application.Payment;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Bonus;
using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.Application.Shared.Dto.enumType.OAuth;
using EtheriT.Coker.Application.Shared.Dto.Marketing;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.Dto.Token;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private const long PublicFixtureSite = 999000001;
    private static readonly Guid PublicFixtureMember = Guid.Parse("44110000-0000-4000-8000-000000000001");
    private static readonly Guid PublicFixtureToken = Guid.Parse("44110000-0000-4000-8000-000000000002");

    public static async Task<string> CreatePublicProductFixtureAsync(BackofficeSandboxFactory factory, IMapper mapper,
        WebsiteLevelEnum level, CancellationToken token)
    {
        using var effects = new DiagnosticsSandboxScope();
        await using var box = await factory.CreateAsync(mapper, level, token, services =>
        {
            services.RemoveAll<ITokenAppService>();
            services.AddSingleton<ITokenAppService>(new PublicFixtureTokens());
        });
        box.Services.GetRequiredService<IConfiguration>()["WebConfig:SiteId"] = PublicFixtureSite.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var fixture = await PublicCartFixture(box);
        var type = new Prod_Spec_Type { FK_WebsiteId = PublicFixtureSite, Type = "隔離尺寸" };
        box.Db.Prod_Spec_Types.Add(type); await box.Db.SaveChangesAsync(token);
        var small = new Prod_Spec { FK_Tid = type.Id, Title = "小" };
        var large = new Prod_Spec { FK_Tid = type.Id, Title = "大" };
        box.Db.Prod_Specs.AddRange(small, large); await box.Db.SaveChangesAsync(token);
        var initial = await box.Db.Prod_Stocks.SingleAsync(row => row.Id == fixture.Stock, token);
        initial.FK_S1id = small.Id;
        var alternate = new Prod_Stock { FK_Pid = fixture.Product, FK_S1id = large.Id, Visible = true, Price = 125,
            Min_Qty = 2, Stock = 12, SubItemNo = "DIAGNOSTICS-LARGE", PackingPoint = 1 };
        box.Db.Prod_Stocks.Add(alternate); await box.Db.SaveChangesAsync(token);
        box.Db.Prod_Prices.Add(new Prod_Price { FK_PSId = alternate.Id, FK_RId = 1, Price = 125, Bonus = 0 });
        await box.Db.SaveChangesAsync(token); box.Db.ChangeTracker.Clear();
        var data = await Products(box).GetMainDisplayOne(fixture.Product);
        token.ThrowIfCancellationRequested();
        return System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
    }

    public static async Task<PublicFinding> ExecutePublicFeatureAsync(string feature, BackofficeSandboxFactory factory,
        IMapper mapper, WebsiteLevelEnum level, CancellationToken token)
    {
        var definition = PublicFeatureChecks.Definitions.Single(item => item.Id == feature && item.Isolated);
        using var effects = new DiagnosticsSandboxScope();
        await using var box = await factory.CreateAsync(mapper, level, token, services =>
        {
            services.RemoveAll<ITokenAppService>();
            services.AddSingleton<ITokenAppService>(new PublicFixtureTokens());
        }, allowMemoryTransactions: true);
        box.Services.GetRequiredService<IConfiguration>()["WebConfig:SiteId"] = PublicFixtureSite.ToString(System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            token.ThrowIfCancellationRequested();
            switch (feature)
            {
                case "quickCart": await PublicCartLifecycle(box); break;
                case "addOn": await PublicAddOnRules(box); break;
                case "cart": await PublicCheckoutDiscounts(box); break;
                case "bonus": await PublicBonusBalance(box); break;
                case "payment": await PublicPaymentAvailability(box); break;
                default: throw new InvalidOperationException("Unknown public isolated executor.");
            }
            token.ThrowIfCancellationRequested();
            return new(definition.Title, "獨立記憶體測試資料", PublicCheckStatus.Passed,
                "實際業務服務的隔離資料操作與斷言通過；未驗證客戶資料、SQL 交易或真實瀏覽器交易。");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception error)
        {
            box.Services.GetRequiredService<ILoggerFactory>().CreateLogger("PublicCommerceDiagnostics")
                .LogWarning(error, "Public isolated feature failed for {Feature}", feature);
            return new(definition.Title, "獨立記憶體測試資料", PublicCheckStatus.Failed,
                "隔離資料中的業務操作或斷言未通過；未對客戶資料建立訂單、寄信或付款。");
        }
    }

    private static async Task PublicSetting(BackofficeSandbox box, string key, string json, long group = 1)
    {
        var field = new StoreSet { key = key, name = key, jobID = "diagnostics", FK_StoreSetGroupId = group };
        box.Db.StoreSet.Add(field);
        await box.Db.SaveChangesAsync();
        box.Db.StoreSetDetail.Add(new StoreSetDetail { FK_WebsiteId = PublicFixtureSite, FK_StoreSetId = field.Id, value = json, enable = true });
        await box.Db.SaveChangesAsync();
    }

    private static async Task<(long Product, long Stock, long Price)> PublicCartFixture(BackofficeSandbox box)
    {
        await PublicSetting(box, "storeBuyState", "[\"yesPay\"]");
        box.Db.Roles.Add(new Role { Id = 1, Name = "隔離訪客角色", Type = RoleTypeEnum.前台, FK_WebsiteId = PublicFixtureSite });
        box.Db.Tokens.Add(new Token { id = PublicFixtureToken, UUID = PublicFixtureMember, UserID = PublicFixtureSite,
            websiteId = PublicFixtureSite, EndTime = DateTime.Now.AddDays(1), ip = "127.0.0.1", PrivacyAgreeTime = DateTime.Now });
        box.Db.FrontUsers.Add(new FrontUser { UUID = PublicFixtureMember, FK_User = PublicFixtureSite,
            Account = "isolated-member", Password = "unusable", Name = "隔離會員" });
        await box.Db.SaveChangesAsync();
        var product = await CreateProduct(box);
        var stock = await box.Db.Prod_Stocks.SingleAsync(row => row.Id == product.Stocks[0].Id);
        stock.Visible = true;
        var price = await box.Db.Prod_Prices.FirstOrDefaultAsync(row => row.FK_PSId == stock.Id && row.FK_RId == 1);
        if (price == null)
        {
            price = new Prod_Price { FK_PSId = stock.Id, FK_RId = 1 };
            box.Db.Prod_Prices.Add(price);
        }
        price.Price = 100; price.Bonus = 0;
        await box.Db.SaveChangesAsync();
        box.Db.ChangeTracker.Clear();
        return (product.Id, stock.Id, price.Id);
    }

    private static async Task PublicCartLifecycle(BackofficeSandbox box)
    {
        var fixture = await PublicCartFixture(box);
        var service = box.Services.GetRequiredService<IShoppingCartAppService>();
        Success(await service.AddUp(new ShoppingCartAddUpDto { FK_PSid = fixture.Stock, FK_PriceId = fixture.Price, Quantity = 2 }));
        box.Db.ChangeTracker.Clear();
        var cart = await box.Db.ShoppingCarts.SingleAsync(row => row.UUID == PublicFixtureMember && !row.IsDeleted);
        Require(cart.Quantity == 2 && cart.Price == 100 && cart.FK_PSid == fixture.Stock, "Cart add did not persist the expected SKU, quantity and price.");
        var display = (await service.GetAll()).Single(row => row.SCId == cart.Id);
        Require(display.PSId == fixture.Stock && display.Quantity == 2 && display.Subtotal == 200,
            "Cart display did not return the selected SKU and calculated subtotal.");
        Success(await service.QuantityUpdate(new() { new ShoppingQuantityUpdateDto { Id = cart.Id, Quantity = 3 } }));
        box.Db.ChangeTracker.Clear();
        Require((await box.Db.ShoppingCarts.SingleAsync(row => row.Id == cart.Id)).Quantity == 3, "Quantity update was not persisted.");
        var before = await box.Db.ShoppingCarts.CountAsync(row => !row.IsDeleted);
        Require(!(await service.AddUp(new ShoppingCartAddUpDto { FK_PSid = fixture.Stock, FK_PriceId = fixture.Price, Quantity = 999 })).Success,
            "Cart accepted a quantity above stock.");
        box.Db.ChangeTracker.Clear();
        Require(await box.Db.ShoppingCarts.CountAsync(row => !row.IsDeleted) == before, "Rejected purchase inserted another cart row.");
        var foreign = new ShoppingCart { UUID = Guid.NewGuid(), FK_PSid = fixture.Stock, FK_PriceId = fixture.Price, Quantity = 1 };
        box.Db.ShoppingCarts.Add(foreign); await box.Db.SaveChangesAsync();
        Require(!(await service.DeleteDrop(foreign.Id)).Success, "Cart deletion crossed member ownership.");
        Success(await service.DeleteDrop(cart.Id));
        box.Db.ChangeTracker.Clear();
        Require((await box.Db.ShoppingCarts.IgnoreQueryFilters().SingleAsync(row => row.Id == cart.Id)).IsDeleted,
            "Cart delete did not remove the current member's row.");
    }

    private static MarketingCampaign PublicCampaign(MarketingRuleTypeEnum type, decimal threshold, decimal discount)
    {
        var campaign = new MarketingCampaign { FK_WebsiteId = PublicFixtureSite, Name = "隔離優惠", StartTime = DateTime.Now.AddDays(-1),
            NeverEnd = true, Status = MarketingDisplayStatusEnum.活動中, CanStack = false };
        campaign.Rules.Add(new MarketingRule { Enabled = true, RuleType = type, ScopeType = MarketingScopeTypeEnum.AllOrder,
            Condition = new MarketingCondition { ConditionType = MarketingConditionTypeEnum.OrderAmount, MinAmount = threshold },
            Reward = new MarketingReward { DiscountAmount = discount, DiscountPercent = 90 } });
        return campaign;
    }

    private static async Task PublicCheckoutDiscounts(BackofficeSandbox box)
    {
        var campaign = PublicCampaign(MarketingRuleTypeEnum.AmountDiscount, 200, 30);
        box.Db.MarketingCampaigns.Add(campaign); await box.Db.SaveChangesAsync();
        var service = ActivatorUtilities.CreateInstance<CheckoutDiscountService>(box.Services);
        var input = new CheckoutDiscountInputDto { PreviewOnly = true, Items = new() { new CheckoutDiscountItemDto { ProductId = 1, ProductStockId = 1, Quantity = 1, UnitPrice = 100 } } };
        Require((await service.CalculateAsync(input)).TotalDiscountAmount == 0, "Discount applied below threshold.");
        input.Items[0].Quantity = 2;
        var output = await service.CalculateAsync(input);
        Require(output.EligibleProductAmount == 200 && output.TotalDiscountAmount == 30, "Checkout total or threshold discount was wrong.");
        campaign.Status = MarketingDisplayStatusEnum.已關閉; await box.Db.SaveChangesAsync();
        Require((await service.CalculateAsync(input)).TotalDiscountAmount == 0, "Closed campaign still discounted checkout.");
        Require(!await box.Db.Order_Headers.AnyAsync(), "Discount preview unexpectedly created an order.");
    }

    private static async Task PublicAddOnRules(BackofficeSandbox box)
    {
        var fixture = await PublicCartFixture(box);
        var rewardInput = ProductFixture();
        rewardInput.Title = "隔離加價購商品"; rewardInput.ItemNo = "DIAGNOSTICS-REWARD";
        rewardInput.Stocks[0].SubItemNo = "DIAGNOSTICS-REWARD-SKU";
        var rewardProduct = await CreateProduct(box, rewardInput);
        var rewardStock = await box.Db.Prod_Stocks.SingleAsync(row => row.Id == rewardProduct.Stocks[0].Id);
        rewardStock.Visible = true;
        var campaign = PublicCampaign(MarketingRuleTypeEnum.AddOnPurchase, 0, 0);
        campaign.CampaignType = MarketingCampaignTypeEnum.加價購;
        var rule = campaign.Rules.Single();
        rule.ScopeType = MarketingScopeTypeEnum.SpecificProducts;
        rule.Condition.ConditionType = MarketingConditionTypeEnum.BuySpecificProduct;
        rule.Condition.MinQuantity = 2;
        rule.ScopeItems.Add(new MarketingScopeItem { TargetType = MarketingScopeTargetTypeEnum.Product, TargetId = fixture.Product, RequiredQuantityPerQualification = 2 });
        rule.Reward.SelectionQuantityPerQualification = 1;
        var reward = new MarketingRewardItem { FK_ProdStockId = rewardStock.Id, OfferPrice = 25, MaxQuantityPerOrder = 1, Enabled = true };
        rule.Reward.Items.Add(reward);
        box.Db.MarketingCampaigns.Add(campaign); await box.Db.SaveChangesAsync();
        var service = ActivatorUtilities.CreateInstance<MarketingAppService>(box.Services);
        var result = await service.GetProductAddOnCampaigns(fixture.Product);
        Success(result);
        var data = ((IEnumerable<ProductAddOnCampaignDto>)result.Object!).ToArray();
        Require(data.Length == 1 && data[0].RequiredQuantity == 2 && data[0].RewardItems.Single().OfferPrice == 25,
            "Add-on campaign qualification, price or selection pool was incorrect.");
        var cart = box.Services.GetRequiredService<IShoppingCartAppService>();
        var selection = new ShoppingCartRewardSelectionDto { CampaignId = campaign.Id, RuleId = rule.Id, RewardItemId = reward.Id, Quantity = 1 };
        Require(!(await cart.AddUp(new ShoppingCartAddUpDto { FK_PSid = fixture.Stock, FK_PriceId = fixture.Price, Quantity = 1, RewardSelections = new() { selection } })).Success,
            "Add-on selection was accepted below its purchase qualification.");
        Success(await cart.AddUp(new ShoppingCartAddUpDto { FK_PSid = fixture.Stock, FK_PriceId = fixture.Price, Quantity = 2, RewardSelections = new() { selection } }));
        box.Db.ChangeTracker.Clear();
        Require(await box.Db.ShoppingCarts.AnyAsync(row => row.UUID == PublicFixtureMember && row.IsAdditional && row.Price == 25 && row.Quantity == 1),
            "Qualified add-on purchase did not use the campaign offer price.");
    }

    private static async Task PublicBonusBalance(BackofficeSandbox box)
    {
        var fixture = await PublicCartFixture(box);
        await PublicSetting(box, "BonusEnabled", "[\"true\"]", 6);
        var price = await box.Db.Prod_Prices.SingleAsync(row => row.Id == fixture.Price);
        price.Bonus = 10;
        var active = new Bonus { UUID = PublicFixtureMember, Amount = 30, Balance = 30, StartDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now.AddDays(1), Status = BonusStatusEnum.Active, Note = "隔離紅利" };
        box.Db.Bonus.Add(active);
        box.Db.Bonus.Add(new Bonus { UUID = PublicFixtureMember, Amount = 500, Balance = 500, StartDate = DateTime.Now.AddDays(-10),
            EndDate = DateTime.Now.AddDays(-1), Status = BonusStatusEnum.Active, Note = "已過期" });
        await box.Db.SaveChangesAsync();
        var service = box.Services.GetRequiredService<IBonusManagementAppService>();
        var balances = await service.GetQueryFrontUsersTotalAvaliableBonus(new() { PublicFixtureMember, Guid.NewGuid() });
        Require(balances.Single(row => row.UserUUID == PublicFixtureMember).TotalAvaliableBonus == 30, "Expired points were included in the balance.");
        var cart = box.Services.GetRequiredService<IShoppingCartAppService>();
        Success(await cart.AddUp(new ShoppingCartAddUpDto { FK_PSid = fixture.Stock, FK_PriceId = fixture.Price, Quantity = 2 }));
        Require(!(await cart.AddUp(new ShoppingCartAddUpDto { FK_PSid = fixture.Stock, FK_PriceId = fixture.Price, Quantity = 2 })).Success,
            "Cart accepted more bonus points than the member owns.");
        box.Db.ChangeTracker.Clear();
        Require((await box.Db.Bonus.SingleAsync(row => row.Id == active.Id)).Balance == 30, "Cart preview consumed real bonus points.");
    }

    private static async Task PublicPaymentAvailability(BackofficeSandbox box)
    {
        var logistics = new LogisticsSetting { FK_WebsiteId = PublicFixtureSite, Title = "隔離物流" };
        var payment = new PaymentType { Title = "隔離付款", Code = "manual", Icons = "", MinAmount = 100, MaxAmount = 500, FK_ThirdPartyId = 0, Used = true };
        box.Db.LogisticsSettings.Add(logistics); box.Db.PaymentTypes.Add(payment); await box.Db.SaveChangesAsync();
        box.Db.PaymentTypesValues.Add(new PaymentTypesValue { FK_WebsiteId = PublicFixtureSite, FK_PaymentTypesId = payment.Id, Used = true });
        await box.Db.SaveChangesAsync();
        var service = new PaymentAvailabilityService(box.Db);
        Require(!(await service.GetAvailableAsync(PublicFixtureSite, logistics.Id, 99)).Single().IsAvailable, "Payment accepted an amount below the minimum.");
        Require((await service.GetAvailableAsync(PublicFixtureSite, logistics.Id, 100)).Single().IsAvailable, "Payment rejected the minimum boundary.");
        Require(!(await service.GetAvailableAsync(PublicFixtureSite, logistics.Id, 501)).Single().IsAvailable, "Payment accepted an amount above the maximum.");
        var denied = false;
        try { await service.GetAvailableAsync(PublicFixtureSite + 1, logistics.Id, 100); } catch (Exception) { denied = true; }
        Require(denied, "Payment availability crossed website ownership.");
        box.Db.LogisticsType_Payments.Add(new LogisticsPaymentRestriction { FK_LogisticsSettingId = logistics.Id, FK_PaymentTypeId = payment.Id, IsEnabled = false });
        await box.Db.SaveChangesAsync();
        Require(!(await service.GetAvailableAsync(PublicFixtureSite, logistics.Id, 100)).Single().IsAvailable, "Disabled logistics payment remained available.");
    }

    // Fixed, local authentication boundary for a fresh InMemory fixture only.
    private sealed class PublicFixtureTokens : ITokenAppService
    {
        public Task<Guid> GetUUID() => Task.FromResult(PublicFixtureMember);
        public Guid GetUUID(Guid oldUUID) => PublicFixtureMember;
        public Task<List<Guid>> GetAllUUID(Guid uuid) => Task.FromResult(new List<Guid> { PublicFixtureMember });
        public Task<TokenResponseDto> CheckToken(string? token) => Task.FromResult(new TokenResponseDto { Success = true, RefreshToken = PublicFixtureToken, IsLogin = true });
        public Task<TokenResponseDto> CreateToken() => CheckToken(null);
        public Task<TokenResponseDto> RefreshToken(Guid? id) => CheckToken(null);
        public Task<TokenKeyItem> NewToken(string? account = null, Guid? uuid = null, long? userId = null) =>
            Task.FromResult(new TokenKeyItem { UUID = PublicFixtureMember, RefreshToken = PublicFixtureToken, IsLogin = true });
        public Task<string> CreateToken(string account, Guid secret, CookiePurposeEnum tokenPurpose = CookiePurposeEnum.BackstageAuthToken, string position = "") => throw new NotSupportedException();
        public Task<bool> DelToken() => throw new NotSupportedException();
        public Task<bool> IsTokenRevoked(string token) => Task.FromResult(false);
        public Task<ResponseMessageDto> AgreePrivacy() => throw new NotSupportedException();
    }
}
