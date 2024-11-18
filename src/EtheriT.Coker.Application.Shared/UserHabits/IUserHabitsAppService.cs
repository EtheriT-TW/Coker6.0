using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.UserHabits
{
    public interface IUserHabitsAppService
    {
        public Task<JsonResult> GetUserGroupList(DataSourceLoadOptions loadOptions);
    }
}
