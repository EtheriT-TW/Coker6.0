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
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Tag;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Tag;
using System.Security.Cryptography;

namespace EtheriT.Coker.Application.UserHabits
{
    public class UserHabitsAppService : IUserHabitsAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITagAppService tagAppService;
        private IMapper mapper;
        public UserHabitsAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITagAppService tagAppService,
            IMapper mapper
        ) {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tagAppService = tagAppService;
            this.mapper = mapper;
        }
        public async Task<JsonResult> GetUserGroupList(DataSourceLoadOptions loadOptions) {
            try {
                var websiteId = await loginUserData.GetWebsiteId();
                var group = from grouing in db.UserGroupings.Where(e => e.FK_WebsiteId == websiteId)
                            select new UserGroupListDto { 
                                Id = grouing.Id,
                                Description = grouing.Description,
                                Title = grouing.Title,
                                Tags = String.Join("、", (
                                    from ta in db.Tag_Associates
                                    where ta.FK_AId == grouing.Id && ta.Type == TagAssociateTypeEnum.使用者分群
                                    join t in db.Tags on ta.FK_TId equals t.Id
                                    where t.FK_WebsiteId == websiteId
                                    select t.Title
                                ).ToList())
                            };
                var dataQuery = mapper.Map<List<UserGroupListDto>>(group);
                var output = DataSourceLoader.Load(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception ex) {
                return new JsonResult(new List<ArticleListGetDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
        }
        public async Task<ResponseMessageDto> AddUpUserGroup(UserGroupAddUpInputDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                UserGrouping? group = await db.UserGroupings.Where(e => e.FK_WebsiteId == websiteId && e.Id == dto.Id).FirstOrDefaultAsync();
                if (group == null)
                {
                    dto.Id = 0;
                    group = mapper.Map<UserGrouping>(dto);
                    group.FK_WebsiteId = websiteId;
                    db.UserGroupings.Add(group);
                }
                else {
                    mapper.Map(dto, group);
                }
                await loginUserData.SaveChanges(group);
                await db.SaveChangesAsync();

                var tags = new List<TagAssociateDto>();
                foreach (var data in dto.Tags)
                {
                    tags.Add(new TagAssociateDto()
                    {
                        Id = data.Id,
                        FK_AId = group.Id,
                        FK_TId = data.FK_TId,
                        Type = TagAssociateTypeEnum.使用者分群,
                        IsDeleted = data.IsDeleted
                    });
                }
                var tag_response = await tagAppService.TagAssociateAddDelect(tags);
                if (!tag_response.Success) throw new Exception(tag_response.Error);
                response.Success = true;
            }
            catch (Exception ex) {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto),JsonConvert.SerializeObject(response));
            return response;
        }
        public async Task<ResponseMessageDto> GetUserGroupOne(long id) {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                UserGrouping? group = await db.UserGroupings.Where(e => e.FK_WebsiteId == websiteId && e.Id == id).FirstOrDefaultAsync();
                if (group != null)
                {
                    UserGroupAddUpDto obj = mapper.Map<UserGroupAddUpDto>(group);
                    var tags = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                        {
                            Fk_Aid = id,
                            Type = TagAssociateTypeEnum.使用者分群,
                        }
                    );
                    if (tags != null) obj.Tags = tags;

                    response.Object = obj;
                }
                else throw new Exception("資料不存在");
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
    }
}
