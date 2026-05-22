using System;
using System.Collections.Generic;

namespace EtheriT.Coker.Application.Shared.Dto.Contact
{
    /// <summary>
    /// 聯絡表單匯出查詢條件；由後台匯出視窗送到 API。
    /// </summary>
    public class ContactExportRequestDto
    {
        /// <summary>
        /// 表單類別對應的 WebMenu Id。
        /// </summary>
        public long FormTypeId { get; set; }

        /// <summary>
        /// 匯出起始時間，後端以 Contact.CreationTime 做含起始邊界查詢。
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 匯出結束時間，後端以 Contact.CreationTime 做含結束邊界查詢。
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 可選的處理狀態篩選；目前規格只允許未處理與已完成。
        /// </summary>
        public List<int>? Statuses { get; set; }
    }
}
