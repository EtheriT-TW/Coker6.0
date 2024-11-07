
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.ShoppingCart
{
    public interface IShoppingCartAppService
    {
        public Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto);
        public Task<ResponseMessageDto> QuantityUpdate(ShoppingQuantityUpdateDto dto);
        public Task<List<ShoppingCartGetAllDto>> GetAll();
        public Task<ShoppingCartGetDrop> GetDropOne(long id, bool isorder);
        public Task<ResponseMessageDto> DeleteDrop(long id);
        public Task<ResponseMessageDto> UpdateUUID(Guid UserUUID, Guid TempUUID);

    }
}
