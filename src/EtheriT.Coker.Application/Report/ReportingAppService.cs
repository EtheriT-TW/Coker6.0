using EtheriT.Coker.Application.Shared.Reporting;
using EtheriT.Coker.Application.Shared.Dto.ReportingModels;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;

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
                    r001 = new R001撿貨單Model
                    {
                        列印時間 = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        訂單日期 = order.CreationTime.ToString("yyyy/MM/dd HH:mm"),
                        訂單編號 = ("000000000" + order.Id.ToString()).Substring(order.Id.ToString().Length, 9),
                        客戶名稱 = order.Orderer,
                        收件人 = order.Recipient,
                        收件人地址 = order.RecipientAddress,
                        收件人電話 = order.RecipientCellPhone,
                        支付方式 = order.PaymentType.Title??"",
                        運費 = order.Freight,
                        用戶備註 = order.Memo ?? "",
                        網站名稱 = siteName,
                        訂單折抵 = order.Discount ?? 0,
                        紅利折抵 = order.Bonus ?? 0,
                        訂單總金額 = order.Subtotal + order.Freight - (order.Discount ?? 0),
                        發票載具 = order.InvoiceRecipient == 1 ? "訂購人" : order.InvoiceRecipient == 2 ? "收件人" : $"公司(三聯){order.UniformId}\n{order.InvoiceTitle}\n{order.InvoiceAddress}",
                        優惠券折抵 = 0,
                        送貨方式 = order.LogisticsSetting.Title,
                        訂單明細 = (from x in order.Order_Details
                            where x.ShoppingCart != null
                            join s1 in db.Prod_Specs on x.ShoppingCart.FK_S1id equals s1.Id into s1Group
                            from s1 in s1Group.DefaultIfEmpty() // LEFT JOIN，當 FK_S1id 為 NULL 時，s1 會是 null

                            join s2 in db.Prod_Specs on x.ShoppingCart.FK_S2id equals s2.Id into s2Group
                            from s2 in s2Group.DefaultIfEmpty() // LEFT JOIN，當 FK_S2id 為 NULL 時，s2 會是 null

                            select new R001撿貨單Model.訂單明細Item
                            {
                                商品名稱 = x.ShoppingCart?.ProdName ?? "",
                                商品規格 = s1 == null && s2 == null
                                ? "無"
                                : s1 != null && s2 != null
                                    ? $"{s1.Title} / {s2.Title}"
                                    : s1?.Title ?? s2?.Title,
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
