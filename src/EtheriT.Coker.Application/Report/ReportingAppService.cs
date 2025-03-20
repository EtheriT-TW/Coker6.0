using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Reporting;
using EtheriT.Coker.Application.Shared.Dto.ReportingModels;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<R001檢貨單Model?> GetR001ModelAsync(long id)
        {
            R001檢貨單Model? r001 = null;
            try
            {
                long siteId = await loginUserData.GetWebsiteId();
                var siteName = await loginUserData.GetWebsiteName();
                var order = db.Order_Headers.Include(e => e.Order_Details).ThenInclude(o => o.ShoppingCart)
                    .Where(x => x.Id == id && x.FK_WebsiteId == siteId).FirstOrDefault();
                if (order != null)
                {
                    r001 = new R001檢貨單Model
                    {
                        列印時間 = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        訂單日期 = order.CreationTime.ToString("yyyy/MM/dd"),
                        訂單編號 = order.Fk_Tid.ToString(),
                        客戶名稱 = order.Orderer,
                        收件人 = order.Recipient,
                        收件人地址 = order.RecipientAddress,
                        收件人電話 = order.RecipientCellPhone,
                        支付方式 = order.Payment.ToString(),
                        運費 = order.Freight,
                        用戶備註 = order.Memo ?? "",
                        網站名稱 = siteName,
                        訂單折抵 = order.Discount ?? 0,
                        紅利折抵 = order.Bonus ?? 0,
                        訂單總金額 = order.Subtotal + order.Freight - (order.Discount ?? 0),
                        發票載具 = order.InvoiceRecipient == 1 ? "訂購人" : order.InvoiceRecipient == 2 ? "收件人" : "公司(三聯)",
                        優惠券折抵 = 0,
                        送貨方式 = ((ShippingTypeEnum)order.Shipping).ToString(),
                        訂單明細 = order.Order_Details.Where(e => e.ShoppingCart != null).Select(x =>
                            new R001檢貨單Model.訂單明細Item
                            {
                                商品名稱 = x.ShoppingCart!.Ser_No.ToString(),
                                商品規格 = $"{x.ShoppingCart.FK_S1id} / {x.ShoppingCart.FK_S2id}",
                                商品單價 = x.ShoppingCart.Price,
                                商品數量 = x.ShoppingCart.Quantity,
                                商品小計 = x.ShoppingCart.Price * x.ShoppingCart.Quantity,
                                商品折扣 = 0,
                            }
                        ).ToList()
                    };
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
