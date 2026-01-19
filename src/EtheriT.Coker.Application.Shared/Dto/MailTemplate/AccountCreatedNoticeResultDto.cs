using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.MailTemplate
{
    public class AccountCreatedNoticeResultDto
    {
        /// <summary>
        /// 網站顯示名稱
        /// </summary>
        public string WebsiteName { get; set; } = string.Empty;

        /// <summary>
        /// 會員帳號（Email）
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 帳號建立時間（顯示用，建議已轉為當地時間）
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 會員條款與隱私權政策連結（完整 URL）
        /// </summary>
        public string PolicyUrl { get; set; } = string.Empty;

        /// <summary>
        /// 可選：加入會員贈送紅利說明文字
        /// 例：系統已自動贈送加入會員紅利 100 點，可至會員中心查看。
        /// </summary>
        public string? BonusText { get; set; }
    }
}
