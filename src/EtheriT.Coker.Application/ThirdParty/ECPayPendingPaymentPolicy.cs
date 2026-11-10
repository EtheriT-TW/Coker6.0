using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.Core.Models;

namespace EtheriT.Coker.Application.ThirdParty
{
    /// <summary>
    /// URICH、ZINGALA、TWQR 建立交易後，付款結果要等綠界後續通知。
    /// 等待期間不可重新付款或取消，否則新交易編號或已取消的訂單會對不上綠界的結果。
    /// </summary>
    internal static class ECPayPendingPaymentPolicy
    {
        private const string UrichCode = "ECPayURICH";
        private const string ZingalaCode = "ECPayZINGALA";
        private const string TwqrCode = "ECPayTWQR";

        // 綠界說明裕富、中租審核約 1～3 個工作日，7 天可涵蓋連假。
        public const int BnplReviewDays = 7;
        // 綠界文件：TWQR 付款期限為 10 分鐘。
        private const int TwqrResultMinutes = 10;

        // 訂單時間早於綠界交易時間，多留緩衝讓查單先判定結果。
        private const int PendingGraceMinutes = 10;

        private const string BnplPendingMessage =
            "訂單已成立，無卡分期審核中，審核結果將由分期公司通知，請勿重複下單，謝謝您的訂購！";

        public static readonly string[] ResultWatchedCodes = { UrichCode, ZingalaCode, TwqrCode };

        public static bool IsResultWatched(string? paymentCode)
        {
            return ResultWatchedCodes.Contains(paymentCode, StringComparer.OrdinalIgnoreCase);
        }

        public static TimeSpan? GetResultWindow(string? paymentCode)
        {
            if (IsBnpl(paymentCode))
                return TimeSpan.FromDays(BnplReviewDays);

            if (string.Equals(paymentCode, TwqrCode, StringComparison.OrdinalIgnoreCase))
                return TimeSpan.FromMinutes(TwqrResultMinutes);

            return null;
        }

        public static bool IsResultPending(Order_Header order, PaymentType? payment, DateTime now)
        {
            if (order.State != OrderStatusEnum.待付款)
                return false;

            var resultWindow = GetResultWindow(payment?.Code);
            if (!resultWindow.HasValue)
                return false;

            var baseTime = order.RepayDate ?? order.CreationTime;
            return now < baseTime.Add(resultWindow.Value).AddMinutes(IsBnpl(payment?.Code) ? PendingGraceMinutes : 0);
        }

        public static DateTime? GetCountdownAvailableAt(Order_Header order, PaymentType? payment)
        {
            if (!string.Equals(payment?.Code, TwqrCode, StringComparison.OrdinalIgnoreCase))
                return null;

            var baseTime = order.RepayDate ?? order.CreationTime;
            return baseTime.AddMinutes(TwqrResultMinutes);
        }

        public static string GetPendingMessage(string? paymentCode)
        {
            return IsBnpl(paymentCode) ? BnplPendingMessage : "";
        }

        private static bool IsBnpl(string? paymentCode)
        {
            return string.Equals(paymentCode, UrichCode, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(paymentCode, ZingalaCode, StringComparison.OrdinalIgnoreCase);
        }
    }
}
