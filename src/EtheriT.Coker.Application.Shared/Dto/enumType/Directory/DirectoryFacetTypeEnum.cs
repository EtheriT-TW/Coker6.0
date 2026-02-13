using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.enumType.Directory
{
    public enum DirectoryFacetTypeEnum
    {
        None = 0,

        Year = 10,        // 依日期（年）
        Month = 11,       // 依日期（1–12，跨年份彙總）
        YearMonth = 12,  // 依日期（2025-01、...、2026-01 單月份匯種）

        Tag = 20,         // 標籤
        DocumentType = 31, // 技術文件類型
    }
}