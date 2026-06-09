namespace EtheriT.Coker.Application.Shared.Dto.enumType.Marketing
{
    /// <summary>
    /// 行銷優惠給付方式。
    /// 用於 MarketingReward.DeliveryType，表示優惠結果是在本次訂單立即套用，還是在付款或訂單完成後發放。
    /// 第一階段主要使用 ApplyImmediately。
    /// </summary>
    public enum MarketingRewardDeliveryTypeEnum
    {
        /// <summary>
        /// 本次訂單立即套用。
        /// 表示優惠在購物車、結帳或下單時計算，直接折抵本次訂單金額。
        /// 例如：滿 1000 折 100、滿 2000 打 9 折。
        /// 第一階段主要使用此給付方式。
        /// </summary>
        ApplyImmediately = 1,

        /// <summary>
        /// 付款完成後發放。
        /// 表示優惠不會直接折抵本次訂單，而是在訂單付款成功後發放。
        /// 適合用於發送優惠券、紅利點數等情境。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        IssueAfterPaid = 2,

        /// <summary>
        /// 訂單完成後發放。
        /// 表示優惠不會直接折抵本次訂單，而是在訂單狀態完成後發放。
        /// 適合用於避免退貨、取消訂單後仍取得優惠券或紅利的情境。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        IssueAfterOrderCompleted = 3
    }
}