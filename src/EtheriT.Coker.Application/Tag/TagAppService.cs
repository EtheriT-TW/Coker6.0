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

namespace EtheriT.Coker.Application.Tag
{
    public class TagAppService : ITagAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        public TagAppService(
            CokerDbContext db,
            LoginUserData loginUserData
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
        }
        public async Task<ResponseMessageDto> AddUp(DevExpressDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            var data = JsonConvert.DeserializeObject<TagGetAllListDto>(dto.Values);

            try
            {
                long usetId = await loginUserData.GetUserId();
                long webid = await loginUserData.GetWebsiteId();

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
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
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
        public async Task<ResponseMessageDto> Delete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var db_t = db.Tags.Where(e => e.Id == Id).FirstOrDefault();
                if (db_t != null)
                {
                    db_t.IsDeleted = true;
                    db_t.DeletionTime = DateTime.Now;
                    db_t.DeleterUserId = usetId;
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
    }
}
