using EtheriT.Coker.Application.Shared.Dto.Role;

namespace EtheriT.Coker.Application.Shared.Member
{
    public interface IFrontRoleContextService
    {
        /// <summary>
        /// 依目前前台網站脈絡取得角色上下文
        /// 若未指定 orgName，使用預設前台網站
        /// 若指定 orgName，會透過子母站規則解析實際 websiteId
        /// </summary>
        public Task<FrontRoleContextDto> GetCurrentContextAsync(string orgName = "");

        /// <summary>
        /// 已知 UUID 與網站脈絡時取得角色上下文
        /// 這支保留給少數需要明確指定 UUID 的情境
        /// </summary>
        public Task<FrontRoleContextDto> GetContextByUuidAsync(Guid uuid, string orgName = "");
    }
}
