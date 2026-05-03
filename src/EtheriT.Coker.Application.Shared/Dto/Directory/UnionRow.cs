using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class UnionRow
    {
        public long Id { get; set; }
        public DirectoryTypeEnum Type { get; set; }
        public DateTime? NodeDate { get; set; } // 文章/選單可用（選單可能 null）
        public int SerNo { get; set; }
        public string? ItemNo { get; set; }     // 商品用
        public ProdStatusEnum? ProdStatus { get; set; } // 商品用
        public int MatchCount { get; set; } // 使用者加權
        public int SortType { get; set; }          // 商品優先：商品=0，其它=1
        public int SortSoldOut { get; set; }       // 售完：售完=1，其它=0（你要售完排後面就 OrderBy 升冪）
        public int SortDiscontinued { get; set; }  // 停產：停產=1，其它=0
    }
}
