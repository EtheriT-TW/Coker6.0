using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using System.Collections.Generic;

namespace EtheriT.Coker.Application.Dto.Contact
{
    public class FormSubmitDto
    {
        /// <summary>
        /// 使用者送出表單時所在頁面的 RouterName。
        /// 舊版流程仍用它查 WebMenu，取得 FK_WebMenuId。
        /// </summary>
        public string RouterName { get; set; } = string.Empty;

        /// <summary>
        /// 表單顯示名稱。
        /// 例如：聯絡我們、預約諮詢、障礙申告。
        /// 沒有傳時，後端 fallback 使用 menu.Title。
        /// </summary>
        public string? FormTitle { get; set; }

        /// <summary>
        /// 表單內容來源類型。
        /// 例如：選單、廣告、文章、商品。
        /// 沒有傳時，後端 fallback 使用 HtmlSanitizeSourceType.選單。
        /// </summary>
        public HtmlSanitizeSourceType? SourceType { get; set; }

        /// <summary>
        /// 表單內容來源 Id。
        /// 例如：Advertise.Id、Article.Id、WebMenu.Id。
        /// 沒有傳時，後端 fallback 使用 menu.Id。
        /// </summary>
        public long? SourceId { get; set; }

        public MailUserDataDto Sender { get; set; } = new MailUserDataDto();

        public List<FormFieldDto> forms { get; set; } = new List<FormFieldDto>();
    }
}