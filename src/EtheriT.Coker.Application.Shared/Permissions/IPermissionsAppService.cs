using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Permissions;
using EtheriT.Coker.Application.Shared.Dto.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Permissions
{
    public interface IPermissionsAppService
    {
        public Task<GetPermissionsOutputDto> GetPermissionsUserData();
        public Task<GetUserPermissionsRsponseDto> GetPermissions(SavePermissionsDto dto);
        public Task<List<SavePermissionsItem>> GetLoginUserPermissions();
        public Task<List<SavePermissionsItem>> GetWebsitePermissions();
        public Task<bool> IsPowerUserPermissions();
		public Task<ResponseMessageDto> SavePermissions(SavePermissionsDto dto);
        public Task<ResponseMessageDto> RemoveMappingUserAndWebsite(DataDelectDto dto);
        public Task<ResponseMessageDto> MappingUserAndWebsite(AddMapingUserAndWebsiteDto dto);
        public Task<ResponseMessageDto> AddRole(AddRoleDto dto);
        public Task<ResponseMessageDto> AddUserToRole(AddUserToRoleDto dto);
        public Task<ResponseMessageDto> RemoveUserToRole(AddUserToRoleDto dto);
        public Task<ResponseMessageDto> EditRole(AddRoleDto dto);
        public Task<ResponseMessageDto> DeleteRole(DataDelectDto dto);
        public Task<ResponseMessageDto> GetPagePermission(GetPagePermissionInputDto dto);
        public Task<ResponseMessageDto> SavePagePermission(SavePagePermissionInputDto dto);
    }
}
