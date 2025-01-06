using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Recipients
{
    public interface IRecipientsAppService
    {
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
    }
}
