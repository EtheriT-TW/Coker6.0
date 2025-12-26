using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Member;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Member
{
    public interface IMemberAppService
    {
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
		public Task<JsonResult> GetAllFrontList(DataSourceLoadOptions loadOptions);
		public Task<JsonResult> GetAllManagerList(DataSourceLoadOptions loadOptions);
		public Task<MemberGetAllDataDto> GetAllData(long id);
        public Task<MemberGetAllDataDto> GetFrontAllData(long id);
        public Task<MemberGetAllDataDto> GetSelfData();
		public Task<ResponseMessageDto> Update(MemberUpdateDto dto);
        public Task<ResponseMessageDto> FrontAddUpdate(MemberUpdateDto dto);
        public Task<List<SelectDto>> GetAllRole();
        public Task<JsonResult> GetDevAllRole(DataSourceLoadOptions loadOptions);
        public Task<ResponseMessageDto> RoleAddUp([FromForm] DevExpressDto dto);
        public Task<ResponseMessageDto> RoleDelete(long id);
        public Task<ResponseMessageDto> ResendFrontUserCreateNoticeMailAsync(long frontUserId);
    }
}
