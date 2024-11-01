using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Favorites;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Favorites;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

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
        public async Task<List<FavoritesGetDisplayDto>> GetDisplay()
        {
            return await favoritesAppService.GetDisplay();
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Delete(long Fid)
        {
            return await favoritesAppService.Delete(Fid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> CheckIsFavorites(long Pid)
        {
            return await favoritesAppService.CheckIsFavorites(Pid);
        }
    }
}
