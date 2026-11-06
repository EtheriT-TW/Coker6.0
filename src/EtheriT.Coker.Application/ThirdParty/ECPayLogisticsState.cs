using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

namespace EtheriT.Coker.Application.ThirdParty
{
    internal static class ECPayLogisticsState
    {
        // Keep the verified return notification separate from the outbound shipment.
        // This uses the existing audit history; it does not require a schema change.
        internal const string ReturnService = "ECPayLogisticsReturnState";
        internal static string ReturnKey(Order_Logistics logistics) => $"{logistics.FK_OhId}:{logistics.Id}";

        internal static async Task<ECPayLogisticsCallbackReverseDto?> GetReturnAsync(CokerDbContext db, long websiteId, Order_Logistics logistics)
        {
            var key = ReturnKey(logistics);
            var json = await db.AuditLogs.AsNoTracking()
                .Where(e => e.FK_WebsiteId == websiteId && e.ServiceName == ReturnService && e.Parameters == key)
                .OrderByDescending(e => e.ExecutionTime).ThenByDescending(e => e.Id)
                .Select(e => e.ReturnValue).FirstOrDefaultAsync();
            return string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<ECPayLogisticsCallbackReverseDto>(json);
        }

        internal static DateTime? ParseDate(string? value) => DateTime.TryParseExact(value, "yyyy/MM/dd HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;

        internal static string ReturnText(Order_Logistics logistics, ECPayLogisticsCallbackReverseDto? state)
        {
            if (state != null) return $"退貨狀態：{state.RtnCode} {state.RtnMsg}（{state.UpdateStatusDate}）";
            return string.IsNullOrEmpty(logistics.RtnMerchantTradeNo) && string.IsNullOrEmpty(logistics.RtnOrderNo)
                ? "" : "已申請退貨，等待綠界物流通知";
        }

        internal static string StatusText(string? code) => code switch
        {
            "300" or "310" or "311" or "325" => "訂單處理中",
            "2030" or "2041" or "3024" => "物流中心理貨中",
            "2068" or "3032" or "3006" or "3015" or "3120" or "3312" or "3313" or "7004" => "配送中",
            "3003" or "3307" or "3308" or "3309" or "3314" => "已配達",
            "2063" or "2073" or "2098" or "3018" or "3029" => "已配達取件門市",
            "2067" or "3022" => "已取件",
            "2065" or "2074" or "3020" => "消費者未於期限內取件",
            "4001" => "消費者已至門市寄件",
            "2055" or "2076" or "2078" or "3025" or "4002" or "7255" => "退貨商品已退至物流中心",
            "2072" or "2099" or "3019" or "3031" => "已配達退件門市",
            "2044" or "2070" or "3023" or "9001" or "9002" => "已取回退回商品",
            "3303" or "3306" => "投遞失敗，請至綠界查詢原因",
            "3117" or "3305" => "拒收，請至綠界查詢原因",
            "3124" or "3311" or "3315" => "退回中",
            "3310" or "5008" => "已退回",
            "2075" or "2077" or "3021" or "5004" => "退回商品逾期未取",
            null or "" => "",
            _ => $"物流狀態代碼 {code}，請至綠界查詢"
        };

        internal static void SyncOrderState(Order_Header order, string? code)
        {
            // Receipt is not automatic order completion: preserve the merchant's completion/refund window.
            if ((order.State == OrderStatusEnum.待確認 || order.State == OrderStatusEnum.已付款 || order.State == OrderStatusEnum.待付款) &&
                new[] { "2068", "3032", "3006", "3015", "3120", "3312", "3313", "7004", "3003", "3307", "3308", "3309", "3314", "2063", "2073", "2098", "3018", "3029", "2067", "3022" }.Contains(code))
                order.State = OrderStatusEnum.已出貨;
        }

        internal static string TrackingNumber(Order_Logistics? logistics, string? fallback)
        {
            if (!string.IsNullOrWhiteSpace(fallback)) return fallback;
            if (logistics == null) return "";
            if (logistics.LogisticsType == "HOME") return logistics.BookingNote ?? "";
            if (logistics.LogisticsSubType == "UNIMARTC2C" && !string.IsNullOrEmpty(logistics.CVSPaymentNo))
                return logistics.CVSPaymentNo + logistics.CVSValidationNo;
            return logistics.CVSPaymentNo ?? "";
        }
    }
}
