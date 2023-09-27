using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Search
{
    public interface ICustSearchAppService
    {
        public Task<JsonResult> GetAll(DataSourceLoadOptions loadOptions);
    }
}
