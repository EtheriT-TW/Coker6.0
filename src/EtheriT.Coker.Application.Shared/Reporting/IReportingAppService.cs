using EtheriT.Coker.Application.Shared.Dto.ReportingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Reporting
{
    public interface IReportingAppService
    {
        public Task<R001檢貨單Model?> GetR001ModelAsync(long id);
    }
}
