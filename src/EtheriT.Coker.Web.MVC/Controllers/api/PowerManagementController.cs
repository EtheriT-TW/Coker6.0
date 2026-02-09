using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Permissions;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.Permissions;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Web.MVC.Startup;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class PowerManagementController
	{
		private readonly NavigationProvider navigation;
		private readonly IPermissionsAppService permissionsAppService;
		private readonly IAccountAppService accountAppService;
        private readonly IMemoryCache memoryCache;
        private readonly LoginUserData loginUserData;
        public PowerManagementController(NavigationProvider navigation, IPermissionsAppService permissionsAppService, IAccountAppService accountAppService, IMemoryCache memoryCache, LoginUserData loginUserData)
		{
			this.navigation = navigation;
			this.permissionsAppService = permissionsAppService;
			this.accountAppService = accountAppService;
            this.memoryCache = memoryCache;
            this.loginUserData = loginUserData;
        }
		[HttpGet]
		public async Task<Site> AllMenus() {
            var site = await navigation.getMenus();
            await navigation.SetPower(site);
            await navigation.SetWebsite(site);
            return site;
		}
        [HttpGet]
        public async Task<GetPermissionsOutputDto> AllUsers()
        {
            return await permissionsAppService.GetPermissionsUserData();
        }
        [HttpPost]
        public async Task<GetUserPermissionsRsponseDto> GetPermissions(SavePermissionsDto dto)
        {
            return await permissionsAppService.GetPermissions(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SavePermissions(SavePermissionsDto dto)
        {
            return await permissionsAppService.SavePermissions(dto);
        }
        [HttpPost]
		public async Task<ResponseUserEditDto> GetUser(DataDelectDto dto) {
			return await accountAppService.GetEditUser(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddUser(AddUser dto)
        {
            return await accountAppService.AddUser(dto);
        }
        [HttpDelete]
		public async Task<ResponseMessageDto> RemoveMappingUserAndWebsite(DataDelectDto dto) {
            return await permissionsAppService.RemoveMappingUserAndWebsite(dto);
        }
		[HttpPost]
		public async Task<ResponseMessageDto> MappingUserAndWebsite(AddMapingUserAndWebsiteDto dto) {
            return await permissionsAppService.MappingUserAndWebsite(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddUserToRole(AddUserToRoleDto dto)
        {
            return await permissionsAppService.AddUserToRole(dto);
        }
        [HttpDelete]
        public async Task<ResponseMessageDto> RemoveUserToRole(AddUserToRoleDto dto) {
            return await permissionsAppService.RemoveUserToRole(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddRole(AddRoleDto dto)
        {
            return await permissionsAppService.AddRole(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> EditRole(AddRoleDto dto)
        {
            return await permissionsAppService.EditRole(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> GetPagePermission(GetPagePermissionInputDto dto) {
            return await permissionsAppService.GetPagePermission(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SavePagePermission(SavePagePermissionInputDto dto)
        {
            return await permissionsAppService.SavePagePermission(dto);
        }
        [HttpDelete]
		public async Task<ResponseMessageDto> DeleteRole(DataDelectDto dto)
		{
			return await permissionsAppService.DeleteRole(dto);
		}
        [HttpGet]
        public async Task<JsonResult> GetPermission() {
            var userId = await loginUserData.GetUserId();
            var permisssion = memoryCache.Get($"ThePermission:{userId}");
            if (permisssion != null)
            {
                return new JsonResult(permisssion, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            return new JsonResult(new { success = false }, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
		}
	}
}
