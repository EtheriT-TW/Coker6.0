using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EtheriT.Coker.Application.Tag
{
    public class TagAppService : ITagAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;
        public TagAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IConfiguration configuration
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
        }
        public async Task<ResponseMessageDto> TagAddUp(DevExpressDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            var data = JsonConvert.DeserializeObject<TagGetAllListDto>(dto.Values);

            try
            {
                long usetId = await loginUserData.GetUserId();
                long webid = await loginUserData.GetWebsiteId();

                if (data != null)
                {
                    if (dto.Key == null)
                    {
                        Core.Models.Tag t = new Core.Models.Tag
                        {
                            FK_WebsiteId = webid,
                            Title = data.Title
                        };
                        db.Tags.Add(t);
                        db.SaveChanges();
                    }
                    else
                    {
                        var db_t = db.Tags.Where(e => e.Id == dto.Key).FirstOrDefault();

                        if (db_t != null)
                        {
                            db_t.Title = data.Title;
                            db_t.LastModifierUserId = usetId;
                            db_t.LastModificationTime = DateTime.Now;
                            db.SaveChanges();
                        }
                    }

                    output.Success = true;
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> TagAssociateAddDelect(List<TagAssociateDto> dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                foreach (var data in dto)
                {
                    if (data.Id == 0 && !data.IsDeleted)
                    {
                        long usetId = await loginUserData.GetUserId();

                        Core.Models.Tag_Associate ta = new Core.Models.Tag_Associate
                        {
                            FK_TId = data.FK_TId,
                            FK_AId = data.FK_AId,
                            Type = data.Type,
                            CreatorUserId = usetId,
                        };
                        db.Tag_Associates.Add(ta);
                        db.SaveChanges();
                    }
                    else if (data.Id > 0 && data.IsDeleted)
                    {
                        await this.TagAssociateDelete((long)data.Id);
                    }
                }

                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<List<TagGetSelectedDto>> GetTagAssociate(TagAssociateGetDto dto)
        {
            try
            {

                long WebsiteID = await loginUserData.GetWebsiteId();
                if (WebsiteID == 0)
                {
                    WebsiteID = configuration.GetValue<long>("WebConfig:SiteId");
                }

                var output = from ta in db.Tag_Associates
                             where ta.FK_AId == dto.Fk_Aid && ta.Type == dto.Type && !ta.IsDeleted
                             join t in db.Tags on ta.FK_TId equals t.Id
                             where !t.IsDeleted && t.FK_WebsiteId == WebsiteID
                             select new TagGetSelectedDto
                             {
                                 Id = ta.Id,
                                 FK_TId = ta.FK_TId,
                                 Tag_Name = t.Title
                             };

                return await output.ToListAsync();

            }
            catch (Exception e) { }

            return null;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long webid = await loginUserData.GetWebsiteId();

                var dataQuery = from t in db.Tags
                                where !t.IsDeleted && t.FK_WebsiteId == webid
                                select new TagGetAllListDto
                                {
                                    Id = t.Id,
                                    Title = t.Title,
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<TagGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<List<TagGetAllDataDto>> GetProductDataAll(long PId)
        {
            try
            {
                var output = await (from ta in db.Tag_Associates
                                    where !ta.IsDeleted && ta.FK_AId == PId && ta.Type == 1
                                    from t in db.Tags
                                    where ta.FK_TId == t.Id
                                    select new TagGetAllDataDto
                                    {
                                        Id = ta.Id,
                                        FK_TId = ta.FK_TId,
                                        Title = t.Title
                                    }).ToListAsync();

                return output;
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<ResponseMessageDto> TagDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long userId = await loginUserData.GetUserId();
                var db_tas = await db.Tag_Associates.Where(e => e.FK_TId == Id).ToListAsync();
                foreach (var db_ta in db_tas)
                {
                    db_ta.IsDeleted = true;
                    db_ta.DeleterUserId = userId;
                    db_ta.DeletionTime = DateTime.Now;
                    db.SaveChanges();
                }

                var db_t = db.Tags.Where(e => e.Id == Id).FirstOrDefault();
                if (db_t != null)
                {
                    db_t.IsDeleted = true;
                    db_t.DeletionTime = DateTime.Now;
                    db_t.DeleterUserId = userId;
                    db.SaveChanges();
                    output.Success = true;
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> TagAssociateDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var db_ta = await db.Tag_Associates.Where(e => e.Id == Id).ToListAsync();

                if (db_ta != null)
                {
                    foreach (var item in db_ta)
                    {
                        item.IsDeleted = true;
                        item.DeletionTime = DateTime.Now;
                        item.DeleterUserId = usetId;
                        db.SaveChanges();
                        output.Success = true;
                    }
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
    }
}
