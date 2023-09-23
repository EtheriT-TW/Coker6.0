using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.StoreSet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class StoreSetController : Controller
    {
        private readonly IStoreSetAppService storeSetAppService;
        public StoreSetController(IStoreSetAppService storeSetAppService) { 
            this.storeSetAppService = storeSetAppService;
        }
        [HttpPost]
        public async Task<StoreSetResponseMessageDto> getValues(StoreSetGetValueInput dto){ 
            return await storeSetAppService.getValues(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> CreateOrUpdate(List<StoreSetDetailOutputDto> datas)
        {
            return await storeSetAppService.CreateOrUpdate(datas);
        }
    }
}
