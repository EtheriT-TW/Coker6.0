using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Recipients;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class RecipientsController : Controller
    {
        private readonly IRecipientsAppService recipientsAppService;
        public RecipientsController(IRecipientsAppService recipientsAppService) 
        { 
            this.recipientsAppService = recipientsAppService;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await recipientsAppService.GetAllList(loadOptions);
        }

        [HttpGet]
        public async Task<List<EtheriT.Coker.Application.Shared.Dto.Recipients.RecipientsDto>> GetCheckoutList()
        {
            return await recipientsAppService.GetCheckoutList();
        }

    }
}
