using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Order
{
	public interface IOrderAppService
    {
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ResponseMessageDto> Delete(int id);
    }
}
