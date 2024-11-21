using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Dto.UserHabits;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.UserHabits;
using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.UserHabits
{
    public class UserHabitsAppService : IUserHabitsAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private IMapper mapper;
        public UserHabitsAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper
        ) {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
        }
        public async Task<JsonResult> GetUserGroupList(DataSourceLoadOptions loadOptions) {
            try {
                var websiteId = await loginUserData.GetWebsiteId();
                var group = db.UserGroupings.Where(e => e.FK_WebsiteId == websiteId);
                var dataQuery = mapper.Map<List<UserGroupListDto>>(group);
                var output = DataSourceLoader.Load(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception ex) {
                return new JsonResult(new List<ArticleListGetDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
        }
        public async Task<ResponseMessageDto> AddUpUserGroup() { 
            throw new NotImplementedException();
        }
    }
}
