using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Recipients
{
    public interface IRecipientsAppService
    {
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<List<EtheriT.Coker.Application.Shared.Dto.Recipients.RecipientsDto>> GetCheckoutList();
        public Task<EtheriT.Coker.Application.Dto.ResponseMessageDto> SaveCheckoutRecipient(EtheriT.Coker.Application.Shared.Dto.Recipients.RecipientsDto dto);
    }
}
