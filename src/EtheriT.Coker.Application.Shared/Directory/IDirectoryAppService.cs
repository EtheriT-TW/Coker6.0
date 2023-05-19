using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Directory
{
    public interface IDirectoryAppService
    {
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
    }
}
