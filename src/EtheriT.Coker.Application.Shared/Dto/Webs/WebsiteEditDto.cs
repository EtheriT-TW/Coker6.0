using EtheriT.Coker.Application.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Webs
{
    /// <summary>
    /// 後台「網站資料」頁存檔用。存檔時以 mapper.Map(dto, website) 寫回，這裡有的欄位會被表單值整個覆蓋。
    /// ⚠ 不可加入客戶管理平台維護的欄位（ServiceStatus、SuspendedDate、TerminatedDate、HostLocation、
    /// IsDomainPending、FK_PlatformDomainId、PlatformRemark）；真的要加，必須在 CustomDtoMapper 的
    /// ReverseMap 設 Ignore，否則後台一存檔就會把平台資料洗成預設值（StartDate／EndDate 曾因此被清空）。
    /// </summary>
    public class WebsiteEditDto
    {
        public long? Id { get; set; }
        public string? DefaultUrl { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Contact { get; set; }
        public string? ContactMail { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? OrgName { get; set; }
    }
}
