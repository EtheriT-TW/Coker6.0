using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Dto;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using EtheriT.Coker.Application.Token;

namespace EtheriT.Coker.Application.Advertise
{
    public class AdvertiseAppService : IAdvertiseAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly ITagAppService tagAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly ITokenAppService tokenAppService;
        private readonly string ApplicationName;
        public AdvertiseAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            ITagAppService tagAppService,
            IFileUploadAppService fileUploadAppService,
            ITokenAppService tokenAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.tagAppService = tagAppService;
            this.fileUploadAppService = fileUploadAppService;
            this.tokenAppService = tokenAppService;
            this.ApplicationName = "Advertise";
        }
        public async Task<ResponseMessageDto> AddUp(AdvertiseDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tag_response = new ResponseMessageDto() { Success = false };
            var asoid = dto.Id;

            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                long usetId = await loginUserData.GetUserId();

                if (dto.Id == null || dto.Id == 0)
                {
                    Core.Models.Advertise a = mapper.Map<Core.Models.Advertise>(dto);
                    a.FK_WebsiteId = WebsiteID;
                    db.Advertise.Add(a);
                    await loginUserData.SaveChanges(a);
                    asoid = a.Id;
                }
                else
                {
                    var result = db.Advertise.Where(e => e.Id == dto.Id).FirstOrDefault();

                    if (result != null)
                    {
                        mapper.Map(dto, result);
                        await loginUserData.SaveChanges(result);
                    }
                    else throw new Exception("查無廣告資料");
                }

                if (asoid != null && dto.TagSelected != null)
                {
                    var tagitem = new List<TagAssociateDto>();
                    foreach (var data in dto.TagSelected)
                    {
                        tagitem.Add(new TagAssociateDto()
                        {
                            Id = data.Id,
                            FK_AId = (long)asoid,
                            FK_TId = data.FK_TId,
                            Type = TagAssociateTypeEnum.廣告,
                            IsDeleted = data.IsDeleted
                        });
                    }

                    tag_response = await tagAppService.TagAssociateAddDelect(tagitem);
                    output.Message = asoid.ToString();
                    output.Success = tag_response.Success;
                }
                else if (dto.Type == (int)AdvertiseTypeEnum.進入廣告 || dto.Type == (int)AdvertiseTypeEnum.右側浮動廣告)
                {
                    output.Success = true;
                }

                output.Message = asoid.ToString();
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            finally
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(new { asoid }), JsonConvert.SerializeObject(output));
            }
            return output;
        }
        public async Task<JsonResult> GetList(DataSourceLoadOptions loadOptions, int Type)
        {
            long WebsiteID = await loginUserData.GetWebsiteId();
            string error = string.Empty;
            try
            {
                var dataQuery = from a in db.Advertise.Where(e => !e.IsDeleted)
                                where a.Type == Type
                                where a.IsDeleted == false
                                select new AdvertiseDto
                                {
                                    Id = a.Id,
                                    Title = a.Title,
                                    StartTime = a.StartDate,
                                    EndTime = a.EndDate,
                                    SerNO = a.SerNO,
                                    Visible = a.Visible,
                                };
                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);

                if (output != null)
                {
                    foreach (var data in output.data)
                    {
                        var htmlId = data.GetType().GetProperty("Id").GetValue(data, null);
                        var getImgFileInput = new FileGetImgInputDto
                        {
                            Sid = (long)htmlId,
                            Type = (int)FileBindTypeEnum.右側浮動廣告,
                            Size = 1
                        };
                        var image = await fileUploadAppService.getImgFiles(getImgFileInput);
                        if (image.Count > 0)
                        {
                            data.GetType().GetProperty("ImgLink").SetValue(data, image[0].Link);
                        }
                    }
                }
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            return new JsonResult(new { error }, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<AdvertiseGetDataDto> GetDataOne(long Id)
        {
            try
            {
                var result = db.Advertise;

                if (result != null)
                {
                    var output = await (from e in result
                                        where e.Id == Id
                                        where !e.IsDeleted
                                        select new AdvertiseGetDataDto
                                        {
                                            Id = e.Id,
                                            Title = e.Title,
                                            Describe = e.Describe,
                                            Visible = e.Visible,
                                            SerNO = e.SerNO,
                                            Link = e.Link,
                                            Target = e.Target,
                                            TagDatas = new List<TagGetSelectedDto>(),
                                            StartTime = e.StartDate,
                                            EndTime = e.EndDate,
                                            permanent = e.Permanent,
                                        }).FirstOrDefaultAsync();

                    if (output != null)
                    {
                        var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                        {
                            Fk_Aid = output.Id,
                            Type = TagAssociateTypeEnum.廣告,
                        }
                        );

                        if (tagDatas != null)
                        {
                            output.TagDatas = tagDatas;
                        }
                    }

                    return output;
                }
                else throw new Exception("查無廣告資料");
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<ResponseMessageDto> Delete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tagdeleteresponse = new ResponseMessageDto() { Success = true };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var result = db.Advertise.Where(e => e.Id == Id).FirstOrDefault();

                if (result != null)
                {
                    var tagids = await db.Tag_Associates.Where(e => e.FK_AId == Id && e.Type == TagAssociateTypeEnum.廣告 && !e.IsDeleted).ToListAsync();

                    if (tagids != null)
                    {
                        foreach (var tagid in tagids)
                        {

                            tagdeleteresponse = await tagAppService.TagAssociateDelete(tagid.Id);
                        }
                    }

                    var delete_img_dto = new FileDeleteDto
                    {
                        Sid = result.Id,
                        Type = (int)FileBindTypeEnum.自訂廣告
                    };
                    var imgdelete_response = await fileUploadAppService.deleteFileById(delete_img_dto);

                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    result.DeleterUserId = usetId;

                    db.SaveChanges();

                    output.Success = tagdeleteresponse.Success && imgdelete_response.Success;
                }
                else throw new Exception("查無廣告資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            finally
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(new { Id }), JsonConvert.SerializeObject(output));
            }
            return output;
        }
        public async Task<ResponseMessageDto> ActivityLog(AdvertiseLogDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                Guid UUID = await tokenAppService.GetUUID();

                var db_ad = db.Advertise.Where(e => e.Id == dto.FK_Aid).FirstOrDefault();
                if (db_ad != null)
                {
                    switch (dto.Action)
                    {
                        case (int)LogActionEnum.顯示:
                            db_ad.Exposure += 1;
                            await loginUserData.SaveChanges(db_ad);
                            break;
                        case (int)LogActionEnum.點擊:
                            db_ad.Clicks += 1;
                            await loginUserData.SaveChanges(db_ad);
                            break;
                    }

                    var userid = await db.FrontUsers.Where(e => e.UUID == UUID).Select(e=>e.FK_User).FirstOrDefaultAsync();

                    Core.Models.Advertise_Log ad_log = new Core.Models.Advertise_Log
                    {
                        FK_Adid = dto.FK_Aid,
                        FK_UserId = userid,
                        UUID = UUID,
                        Action = dto.Action,
                    };

                    db.Advertise_Logs.Add(ad_log);
                    db.SaveChanges();
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
        public async Task<JsonResult> GetDisplay(long webid, int type, int number)
        {
            try
            {
                var result = db.Advertise;

                if (result != null)
                {
                    var output = await (from e in result
                                        where !e.IsDeleted && e.Visible && e.Type == type && e.FK_WebsiteId == webid
                                        where e.Permanent || (DateTime.Compare(DateTime.Now, (DateTime)e.StartDate) > 0 && DateTime.Compare(DateTime.Now, (DateTime)e.EndDate) < 0)
                                        orderby e.SerNO
                                        select new AdvertiseDisplayDto
                                        {
                                            Id = e.Id,
                                            Title = e.Title,
                                            Link = e.Link,
                                            Target = e.Target,
                                        }).Take(number).ToListAsync();

                    if (output != null)
                    {
                        switch (type)
                        {
                            case (int)AdvertiseTypeEnum.右側浮動廣告:
                                for (var i = 0; i < output.Count; i++)
                                {
                                    output[i].FileLink = await fileUploadAppService.getAdvertiseFiles(output[i].Id, (int)FileBindTypeEnum.右側浮動廣告);
                                }
                                break;
                            case (int)AdvertiseTypeEnum.進入廣告:
                                for (var i = 0; i < output.Count; i++)
                                {
                                    output[0].FileLink = await fileUploadAppService.getAdvertiseFiles(output[i].Id, (int)FileBindTypeEnum.進入廣告);
                                }
                                break;
                        }
                    }
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e) { }
            return new JsonResult(new List<AdvertiseDisplayDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
    }
}
