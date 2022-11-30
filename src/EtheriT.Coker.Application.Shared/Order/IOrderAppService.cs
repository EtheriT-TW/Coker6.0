using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Order;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Order
{
	public interface IOrderAppService
    {
        public Task<ResponseMessageDto> Add(OrderHeaderAddDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ResponseMessageDto> Delete(int id);
    }
}
