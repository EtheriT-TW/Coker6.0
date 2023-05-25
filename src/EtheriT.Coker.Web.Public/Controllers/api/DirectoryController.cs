using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DirectoryController : Controller
    {

        private readonly IDirectoryAppService directoryAppService;
        public DirectoryController(IDirectoryAppService directoryAppService)
        {
            this.directoryAppService = directoryAppService;
        }

        [HttpPost]
        public async Task<DirectoryReleInfoGetDto> GetReleInfo(DirectoryReleInfoInputDto dto)
        {
            return await directoryAppService.GetReleInfo(dto);
        }

    }
}
