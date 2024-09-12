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
using System.Security.Cryptography;
using EtheriT.Coker.Application.Shared.Dto.enumType;

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
                            Title = data.Title,
                            CreatorUserId = usetId,
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
        public async Task<ResponseMessageDto> TagGroupAddUp(DevExpressDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            var data = JsonConvert.DeserializeObject<TagGroupGetAllListDto>(dto.Values);


            try
            {
                long webid = await loginUserData.GetWebsiteId();
                long usetId = await loginUserData.GetUserId();

                if (data != null)
                {
                    long assoid = 0;
                    if (dto.Key == null)
                    {
                        Core.Models.Tag_Group tg = new Core.Models.Tag_Group
                        {
                            Title = data.Title,
                            Disp_Opt = data.Disp_Opt == null ? true : (bool)data.Disp_Opt,
                            CreatorUserId = usetId,
                            FK_WebsiteId = webid,
                        };
                        db.Tag_Groups.Add(tg);
                        db.SaveChanges();
                        assoid = tg.Id;
                    }
                    else
                    {
                        var db_tg = db.Tag_Groups.Where(e => e.Id == dto.Key).FirstOrDefault();

                        if (db_tg != null)
                        {
                            if (data.Title != null) db_tg.Title = data.Title;
                            if (data.Disp_Opt != null) db_tg.Disp_Opt = (bool)data.Disp_Opt;
                            db_tg.LastModifierUserId = usetId;
                            db_tg.LastModificationTime = DateTime.Now;
                            db.SaveChanges();
                            assoid = (long)dto.Key;
                        }
                    }

                    if (data.FK_Tid.Count > 0 && assoid > 0)
                    {
                        output = await this.Tag_TagGroupAddUp(new Tag_TagGroupListDto()
                        {
                            FK_TGId = assoid,
                            FK_TId = data.FK_Tid,
                        });
                    }
                    else
                    {
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
        public async Task<ResponseMessageDto> Tag_TagGroupAddUp(Tag_TagGroupListDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                for (var i = 0; i < dto.FK_TId.Count; i++)
                {
                    var result = await db.Tag_TagGroups.Where(e => e.FK_TGId == dto.FK_TGId && e.FK_TId == dto.FK_TId[i] && !e.IsDeleted).FirstOrDefaultAsync();
                    if (result == null)
                    {
                        Core.Models.Tag_TagGroup t_tg = new Core.Models.Tag_TagGroup
                        {
                            FK_TGId = dto.FK_TGId,
                            FK_TId = dto.FK_TId[i],
                            CreatorUserId = usetId,
                        };
                        db.Tag_TagGroups.Add(t_tg);
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
                List<long> siteIds = await db.MappingWebsiteRelationship.Where(e => e.FatherId == webid).Where(e => !e.IsDeleted).Select(e => e.Id).ToListAsync();
                siteIds.Add(webid);

                var dataQuery = from t in db.Tags
                                join s in db.Websites on t.FK_WebsiteId equals s.Id
                                where !t.IsDeleted && siteIds.Contains(t.FK_WebsiteId)
                                orderby t.FK_WebsiteId
                                select new TagGetAllListDto
                                {
                                    Id = t.Id,
                                    Title = t.Title,
                                    SiteNameTitle = s.Title
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<TagGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetAllGroupList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long webid = await loginUserData.GetWebsiteId();

                var dataQuery = from tg in db.Tag_Groups
                                where !tg.IsDeleted && tg.FK_WebsiteId == webid
                                select new TagGroupGetAllListDto
                                {
                                    Id = tg.Id,
                                    Title = tg.Title,
                                    Disp_Opt = tg.Disp_Opt,
                                    FK_Tid = null,
                                    TagTitle = null,
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                if (output != null)
                {
                    foreach (var data in output.data)
                    {
                        var tag_taggroups = await db.Tag_TagGroups.Where(e => e.FK_TGId == (long)data.GetType().GetProperty("Id").GetValue(data, null) && !e.IsDeleted).ToListAsync();
                        if (tag_taggroups.Count > 0)
                        {
                            var temp_tagids = new List<long>();
                            var temp_tagtitles = new List<string>();
                            for (var i = 0; i < tag_taggroups.Count; i++)
                            {
                                var tag_name = await db.Tags.Where(e => e.Id == tag_taggroups[i].FK_TId && !e.IsDeleted).FirstOrDefaultAsync();
                                if (tag_name != null)
                                {
                                    temp_tagids.Add(tag_name.Id);
                                    temp_tagtitles.Add(tag_name.Title);
                                }
                            }
                            data.GetType().GetProperty("FK_Tid").SetValue(data, temp_tagids);
                            data.GetType().GetProperty("TagTitle").SetValue(data, temp_tagtitles);
                        }
                    }
                }

                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<TagGroupGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> TagAssociateAddDelect(List<TagAssociateDto> dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                List<Core.Models.Tag_Associate> TagBindings = new List<Core.Models.Tag_Associate>();
                var all = db.Tag_Associates.Select(e => new { e.Id, e.FK_AId, e.FK_TId }).ToList();

                for (int i = 0; i < dto.Count; i++)
                {
                    var data = dto[i];
                    var ass = await db.Tag_Associates.Where(e => e.FK_AId == data.FK_AId && e.FK_TId == data.FK_TId && e.Type == data.Type).FirstOrDefaultAsync();
                    if (ass != null) data.Id = ass.Id;
                    else data.Id = 0;
                    if (data.Id == 0 && !data.IsDeleted)
                    {
                        Core.Models.Tag_Associate ta = new Core.Models.Tag_Associate
                        {
                            FK_TId = data.FK_TId,
                            FK_AId = data.FK_AId,
                            Type = data.Type,
                            CreatorUserId = usetId,
                        };
                        TagBindings.Add(ta);
                    }
                    else if (data.Id > 0 && !data.IsDeleted && ass!=null) {
                        ass.IsDeleted = false;
                        ass.DeleterUserId = null;
                        ass.DeletionTime = null;
                    }
                    else if (data.Id > 0 && data.IsDeleted)
                    {
                        await this.TagAssociateDelete((long)data.Id);
                    }
                }
                db.Tag_Associates.AddRange(TagBindings);
                await db.SaveChangesAsync();
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
                List<long> siteIds = await db.MappingWebsiteRelationship.Where(e => e.FatherId == WebsiteID).Where(e => !e.IsDeleted).Select(e => e.Id).ToListAsync();
                siteIds.Add(WebsiteID);

                var output = from ta in db.Tag_Associates
                             where ta.FK_AId == dto.Fk_Aid && ta.Type == dto.Type && !ta.IsDeleted
                             join t in db.Tags on ta.FK_TId equals t.Id
                             where !t.IsDeleted && siteIds.Contains(t.FK_WebsiteId)
                             group t by new { t.Id,t.Title} into g
                             select new TagGetSelectedDto
                             {
                                 FK_TId = g.Key.Id,
                                 Tag_Name = g.Key.Title
                             };

                return await output.ToListAsync();

            }
            catch (Exception e) { }

            return null;
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
        public async Task<List<TagGetAllDataDto>> GetAdvertiseDataAll(long AdId)
        {
            try
            {
                var output = await (from ta in db.Tag_Associates
                                    where !ta.IsDeleted && ta.FK_AId == AdId && ta.Type == (int)TagAssociateTypeEnum.廣告
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
        public async Task<TagGetAllListDto?> GetTagByName(string name)
        {
            try
            {
                var siteId = await loginUserData.GetWebsiteId();
                var output = await (from ta in db.Tags.Include(e => e.Website)
                                    where !ta.IsDeleted && ta.Title.Contains(name) && ta.FK_WebsiteId == siteId
                                    select new TagGetAllListDto
                                    {
                                        Id = ta.Id,
                                        Title = ta.Title,
                                        SiteNameTitle = ta.Website == null ? "" : ta.Website.Title
                                    }).FirstOrDefaultAsync();

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

                var db_ttgs = await db.Tag_TagGroups.Where(e => e.FK_TId == Id).ToListAsync();
                foreach (var db_ttg in db_ttgs)
                {
                    db_ttg.IsDeleted = true;
                    db_ttg.DeleterUserId = userId;
                    db_ttg.DeletionTime = DateTime.Now;
                    var db_ttg_remain = await db.Tag_TagGroups.Where(e => e.FK_TGId == db_ttg.FK_TGId && !e.IsDeleted).ToListAsync();
                    if (db_ttg_remain.Count == 1)
                    {
                        var db_tg = await db.Tag_Groups.Where(e => e.Id == db_ttg.FK_TGId && !e.IsDeleted).FirstOrDefaultAsync();
                        db_tg.Disp_Opt = false;
                        db_tg.LastModifierUserId = userId;
                        db_tg.LastModificationTime = DateTime.Now;
                    }
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
        public async Task<ResponseMessageDto> TagGroupDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long userId = await loginUserData.GetUserId();
                var db_ttgs = await db.Tag_TagGroups.Where(e => e.FK_TGId == Id).ToListAsync();
                foreach (var db_ttg in db_ttgs)
                {
                    db_ttg.IsDeleted = true;
                    db_ttg.DeleterUserId = userId;
                    db_ttg.DeletionTime = DateTime.Now;
                    db.SaveChanges();
                }

                var db_tg = db.Tag_Groups.Where(e => e.Id == Id).FirstOrDefault();
                if (db_tg != null)
                {
                    db_tg.IsDeleted = true;
                    db_tg.DeletionTime = DateTime.Now;
                    db_tg.DeleterUserId = userId;
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
