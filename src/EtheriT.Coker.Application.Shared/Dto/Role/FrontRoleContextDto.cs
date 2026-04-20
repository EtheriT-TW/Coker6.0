using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Role
{
    public class FrontRoleContextDto
    {
        /// <summary>
        /// 目前網站Id
        /// </summary>
        public long WebsiteId { get; set; }
        /// <summary>
        /// 目前解析出的角色 Id
        /// 訪客預設為 1
        /// </summary>
        public long CurrentRoleId { get; set; }

        /// <summary>
        /// 目前解析出的角色名稱
        /// </summary>
        public string CurrentRoleName { get; set; } = string.Empty;

        /// <summary>
        /// 是否為訪客
        /// </summary>
        public bool IsGuest { get; set; }

        /// <summary>
        /// 當前角色在 RoleLevels 中的索引
        /// </summary>
        public int RoleIndex { get; set; }

        /// <summary>
        /// 由低到高的可見角色 Id
        /// 例如：1, 101, 102
        /// </summary>
        public List<long> VisibleRoleIds { get; set; } = new();

        /// <summary>
        /// 由低到高的角色層級
        /// 第一筆固定是非會員
        /// </summary>
        public List<FrontRoleLevelDto> RoleLevels { get; set; } = new();
    }
}
