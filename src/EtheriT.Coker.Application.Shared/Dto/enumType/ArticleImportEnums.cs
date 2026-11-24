namespace EtheriT.Coker.Application.Shared.Dto.enumType
{
    public enum ArticleImportRowStatusEnum
    {
        // 等待 AI 處理
        Pending = 0,
        // AI 已填好，等使用者在預覽頁確認
        Ready = 1,
        // AI 或圖片處理失敗，跳過並列在結果
        Failed = 2,
        // 已建立或更新文章
        Imported = 3,
        // 使用者在預覽頁取消
        Excluded = 4
    }
}