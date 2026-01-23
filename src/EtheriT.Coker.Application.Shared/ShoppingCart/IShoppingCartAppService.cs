
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.ShoppingCart
{
    public interface IShoppingCartAppService
    {
        public Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto);
        public Task<ResponseMessageDto> QuantityUpdate(List<ShoppingQuantityUpdateDto> dtos);
        public Task<List<ShoppingCartDisplayDto>> GetAll();
        public Task<ShoppingCartDisplayDto> GetDropOne(long id, bool isorder);
        public Task<List<ShoppingCartDisplayDto>> GetDisplay(List<long> scids);
        public Task<ResponseMessageDto> Reorder(List<long> scids);
        public Task<List<ShoppingCartDisplayDto>> CheckStockPrice(List<long> scids);
        public Task<ResponseMessageDto> DeleteDrop(long id);
        public Task<ResponseMessageDto> UpdateUUID(Guid UserUUID, Guid TempUUID);
        public Task<bool> checkBonusCanUse(Guid uuid, List<OrderDetailAddDto> OrderDetails);

    }
}
