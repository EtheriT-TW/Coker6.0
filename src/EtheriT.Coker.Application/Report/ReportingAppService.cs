using EtheriT.Coker.Application.Shared.Reporting;
using EtheriT.Coker.Application.Shared.Dto.ReportingModels;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Application.Marketing;
using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.Report
{
    public class ReportingAppService : IReportingAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        public ReportingAppService(CokerDbContext db, LoginUserData loginUserData)
        {
            this.db = db;
            this.loginUserData = loginUserData;
        }
        public async Task<R001撿貨單Model?> GetR001ModelAsync(long id)
        {
            R001撿貨單Model? r001 = null;
            try
            {
                long siteId = await loginUserData.GetWebsiteId();
                var siteName = await loginUserData.GetWebsiteName();
                var order = db.Order_Headers.Include(e => e.PaymentType).Include(e => e.LogisticsSetting).Include(e => e.Order_Details).ThenInclude(o => o.ShoppingCart)
                    .Where(x => x.Id == id && x.FK_WebsiteId == siteId).FirstOrDefault();
                if (order != null)
                {
                    var detailItems = order.Order_Details
                        .Where(x => x.ShoppingCart != null)
                        .Select(x => new R001撿貨單Model.訂單明細Item
                        {
                            商品Id = x.ShoppingCart!.ProductId ?? 0,
                            IsAdditional = x.ShoppingCart.IsAdditional,
                            FK_MarketingRewardItemId =
                                x.ShoppingCart.FK_MarketingRewardItemId,

                            商品名稱 = x.ShoppingCart.ProdName ?? "",

                            商品規格 =
                                string.IsNullOrWhiteSpace(x.ShoppingCart.S1Title) &&
                                string.IsNullOrWhiteSpace(x.ShoppingCart.S2Title)
                                    ? "無"
                                    : !string.IsNullOrWhiteSpace(x.ShoppingCart.S1Title) &&
                                      !string.IsNullOrWhiteSpace(x.ShoppingCart.S2Title)
                                        ? $"{x.ShoppingCart.S1Title} / {x.ShoppingCart.S2Title}"
                                        : x.ShoppingCart.S1Title ??
                                          x.ShoppingCart.S2Title ??
                                          "無",

                            商品單價 =
                                x.ShoppingCart.Price == 0 &&
                                (x.ShoppingCart.Bonus ?? 0) > 0
                                    ? $"紅利：{x.ShoppingCart.Bonus ?? 0}"
                                    : x.ShoppingCart.Price.ToString("$#,##0")
                                      + ((x.ShoppingCart.Bonus ?? 0) > 0
                                          ? $"\n紅利：{x.ShoppingCart.Bonus ?? 0}"
                                          : ""),

                            商品紅利 = x.ShoppingCart.Bonus ?? 0,

                            商品金額 = x.ShoppingCart.Price,

                            商品數量 = x.ShoppingCart.Quantity,

                            商品小計 =
                                x.ShoppingCart.Price == 0 &&
                                (x.ShoppingCart.Bonus ?? 0) > 0
                                    ? $"紅利：{(x.ShoppingCart.Bonus ?? 0) *
                                                x.ShoppingCart.Quantity}"
                                    : (x.ShoppingCart.Price *
                                       x.ShoppingCart.Quantity).ToString("$#,##0")
                                      + ((x.ShoppingCart.Bonus ?? 0) > 0
                                          ? $"\n紅利：{(x.ShoppingCart.Bonus ?? 0) *
                                                      x.ShoppingCart.Quantity}"
                                          : ""),

                            商品折扣 = 0
                        })
                        .ToList();
                    var rewardInfos =
                        await MarketingCartOrdering.LoadRewardInfosAsync(
                            db,
                            detailItems.Select(x => x.FK_MarketingRewardItemId)
                        );

                    // 判斷訂單層級 / 商品層級優惠
                    foreach (var item in detailItems.Where(x => x.IsAdditional))
                    {
                        if (!item.FK_MarketingRewardItemId.HasValue)
                            continue;

                        if (!rewardInfos.TryGetValue(
                                item.FK_MarketingRewardItemId.Value,
                                out var rewardInfo))
                            continue;

                        item.IsOrderLevelAdditional =
                            rewardInfo.ConditionType ==
                                MarketingConditionTypeEnum.OrderAmount ||
                            rewardInfo.ConditionType ==
                                MarketingConditionTypeEnum.OrderQuantity;
                    }

                    // 跟購物車 / 訂單顯示使用相同排序規則
                    detailItems = MarketingCartOrdering.Sort(
                        detailItems,
                        x => x.IsAdditional,
                        x => x.商品Id,
                        x => x.FK_MarketingRewardItemId,
                        rewardInfos
                    );

                    // 組出撿貨單實際要顯示的文字
                    foreach (var item in detailItems)
                    {
                        if (!item.IsAdditional)
                            continue;

                        if (item.IsOrderLevelAdditional)
                        {
                            item.商品名稱 =
                                $"【訂單優惠】 {item.商品名稱}";
                        }
                        else
                        {
                            item.商品名稱 =
                                $"↳ 加價購／贈品　{item.商品名稱}";
                        }
                    }

                    r001 = new R001撿貨單Model
                    {
                        列印時間 = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        訂單日期 = order.CreationTime.ToString("yyyy/MM/dd HH:mm"),
                        訂單編號 = ("000000000" + order.Id.ToString()).Substring(order.Id.ToString().Length, 9),
                        客戶名稱 = order.Orderer,
                        收件人 = order.Recipient,
                        收件人地址 = order.RecipientAddress,
                        收件人電話 = order.RecipientCellPhone,
                        支付方式 = order.PaymentType.Title ?? "",
                        運費 = order.Freight,
                        用戶備註 = order.Remark ?? "",
                        網站名稱 = siteName,
                        訂單折抵 = order.Discount ?? 0,
                        紅利折抵 = order.Bonus ?? 0,
                        // Subtotal 是下單時已套用行銷活動與紅利折抵後保存的商品應付金額。
                        // 列印明細應與訂單明細、付款及退款流程一致，直接使用訂單保存的金額，
                        // 不可再次扣除 Discount，否則行銷活動折抵會被重複計算。
                        訂單總金額 = order.Subtotal + order.Freight,
                        發票資訊 = $"{(
                            string.IsNullOrEmpty(order.Carrier) ?
                                string.IsNullOrEmpty(order.UniformId) ? "" : $"統一編號：{order.UniformId}\n公司抬頭：{order.InvoiceTitle}\n公司地址：{order.InvoiceAddress}" :
                                $"手機條碼：{order.Carrier}"
                        )}",
                        優惠券折抵 = 0,
                        送貨方式 = order.LogisticsSetting.Title,
                        訂單明細 = detailItems
                    };
                    r001.合計 = r001.訂單明細.Sum(x =>
                    {
                        return x.商品金額 * x.商品數量;
                    });
                    r001.商品使用紅利 = r001.訂單明細.Sum(x =>
                    {
                        return x.商品紅利 * x.商品數量;
                    });
                    r001.訂單紅利折抵 = r001.紅利折抵 - r001.商品使用紅利;
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return r001;
        }
    }
}
