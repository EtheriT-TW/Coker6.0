
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.ShoppingCart
{
    public interface IShoppingCartAppService
    {
        public Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto);
        public Task<ResponseMessageDto> QuantityUpdate(ShoppingQuantityUpdateDto dto);
        public Task<List<ShoppingCartGetAllDto>> GetAll(String Tid, long siteId);
        public Task<ShoppingCartGetDrop> GetDropOne(long id, long siteId);
        public Task<ResponseMessageDto> DeleteDrop(long id);

    }
}
