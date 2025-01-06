using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.UserHabits;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.UserHabits
{
    public interface IUserHabitsAppService
    {
        public Task<JsonResult> GetUserGroupList(DataSourceLoadOptions loadOptions);
        public Task<ResponseMessageDto> AddUpUserGroup(UserGroupAddUpInputDto dto);
        public Task<ResponseMessageDto> GetUserGroupOne(long id);
        public Task<ResponseMessageDto> DeleteUserGroup(long id);
    }
}
