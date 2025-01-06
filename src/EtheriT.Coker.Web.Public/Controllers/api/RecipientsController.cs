using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Recipients;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipientsController : ControllerBase
    {
        private readonly IRecipientsAppService recipientsAppService;
        public RecipientsController(IRecipientsAppService recipientsAppService) { this.recipientsAppService = recipientsAppService; }

        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await recipientsAppService.GetAllList(loadOptions);
        }
    }
}
