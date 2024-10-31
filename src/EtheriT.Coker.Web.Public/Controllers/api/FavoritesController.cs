using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Favorites;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FavoritesController : Controller
    {
        private readonly IFavoritesAppService favoritesAppService;
        public FavoritesController(IFavoritesAppService favoritesAppService)
        {
            this.favoritesAppService = favoritesAppService;
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Add(long Pid)
        {
            return await favoritesAppService.Add(Pid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Delete(long Fid)
        {
            return await favoritesAppService.Delete(Fid);
        }
    }
}
