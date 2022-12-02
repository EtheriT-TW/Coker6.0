
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.ShoppingCart
{
    public interface IShoppingCartAppService
    {
        public Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto);
        public Task<List<ShoppingCartGetDrop>> GetDrop(String id);
        public Task<ResponseMessageDto> DeleteDrop(long id);

    }
}
