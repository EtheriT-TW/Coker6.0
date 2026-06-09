namespace EtheriT.Coker.Application.Shared.Dto.enumType.Marketing
{
    /// <summary>
    /// 行銷活動狀態。
    /// 用於 MarketingCampaign.Status，表示活動目前的管理狀態與列表顯示狀態。
    /// 實際判斷活動是否可套用時，不應只看此欄位，仍需同時檢查 StartTime、EndTime、NeverEnd。
    /// 第一階段可直接存放此 enum，但建議只有「草稿、活動中、已關閉」由後台手動設定。
    /// 「未開始、已結束」通常由系統依活動起訖時間計算後顯示。
    /// </summary>
    public enum MarketingDisplayStatusEnum
    {
        /// <summary>
        /// 草稿。
        /// 表示活動尚未正式啟用，通常是後台建立中或尚未設定完成。
        /// 草稿狀態不會參與購物車、結帳或訂單優惠計算。
        /// </summary>
        草稿 = 0,

        /// <summary>
        /// 未開始。
        /// 表示活動已設定為可啟用狀態，但目前時間尚未到達 StartTime。
        /// 此狀態通常由系統依時間計算顯示，不建議由後台手動設定。
        /// 未開始狀態不會參與優惠計算。
        /// </summary>
        未開始 = 1,

        /// <summary>
        /// 活動中。
        /// 表示活動允許參與優惠計算。
        /// 實際是否可套用仍需檢查目前時間是否落在 StartTime 與 EndTime 之間。
        /// 只有此狀態且時間符合時，活動才應被購物車或結帳流程採用。
        /// </summary>
        活動中 = 2,

        /// <summary>
        /// 已結束。
        /// 表示目前時間已超過 EndTime。
        /// 此狀態通常由系統依時間計算顯示，不建議由後台手動設定。
        /// 已結束狀態不會參與優惠計算。
        /// </summary>
        已結束 = 3,

        /// <summary>
        /// 已關閉。
        /// 表示活動被後台手動停用。
        /// 不論目前時間是否符合活動起訖時間，已關閉狀態都不會參與優惠計算。
        /// </summary>
        已關閉 = 4,
    }
}