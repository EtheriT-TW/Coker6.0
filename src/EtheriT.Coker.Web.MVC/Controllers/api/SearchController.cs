using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ICustSearchAppService custSearchAppService;
        public SearchController(ICustSearchAppService custSearchAppService) { 
            this.custSearchAppService = custSearchAppService;        
        }
        [HttpGet]
        public async Task<JsonResult> GetAll(DataSourceLoadOptions loadOptions)
        {
            return await custSearchAppService.GetAll(loadOptions);
        }
    }
}
