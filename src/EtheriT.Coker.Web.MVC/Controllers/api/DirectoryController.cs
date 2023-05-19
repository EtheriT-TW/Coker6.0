using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Directory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DirectoryController : Controller
    {
        private readonly IDirectoryAppService directoryAppService;

        public DirectoryController(IDirectoryAppService directoryAppService)
        {
            this.directoryAppService = directoryAppService;
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await directoryAppService.GetAllList(loadOptions);
        }
    }
}
