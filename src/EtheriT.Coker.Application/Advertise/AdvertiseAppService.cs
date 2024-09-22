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

namespace EtheriT.Coker.Application.Advertise
{
    public class AdvertiseAppService : IAdvertiseAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly ITagAppService tagAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly string ApplicationName;
        public AdvertiseAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            ITagAppService tagAppService,
            IFileUploadAppService fileUploadAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.tagAppService = tagAppService;
            this.fileUploadAppService = fileUploadAppService;
            this.ApplicationName = "Advertise";
        }
        public async Task<ResponseMessageDto> AddUp(AdvertiseDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tag_response = new ResponseMessageDto() { Success = false };
            var asoid = dto.Id;

            try
            {
                var datatata = loginUserData;
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

                if (asoid != null)
                {
                    var tagitem = new List<TagAssociateDto>();
                    foreach (var data in dto.TagSelected)
                    {
                        tagitem.Add(new TagAssociateDto()
                        {
                            Id = data.Id,
                            FK_AId = (long)asoid,
                            FK_TId = data.FK_TId,
                            Type = (int)TagAssociateTypeEnum.廣告,
                            IsDeleted = data.IsDeleted
                        });
                    }

                    tag_response = await tagAppService.TagAssociateAddDelect(tagitem);
                    output.Message = asoid.ToString();
                }

                output.Success = tag_response.Success;
                output.Message = asoid.ToString();
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            finally
            {
                await loginUserData.SetLogs(ApplicationName, "AddUp", JsonConvert.SerializeObject(new { asoid }), JsonConvert.SerializeObject(output));
            }
            return output;
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
                            Type = (int)TagAssociateTypeEnum.廣告,
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
                    var tagids = await db.Tag_Associates.Where(e => e.FK_AId == Id && e.Type == (int)TagAssociateTypeEnum.廣告 && !e.IsDeleted).ToListAsync();

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
                await loginUserData.SetLogs(ApplicationName, "Delete", JsonConvert.SerializeObject(new { Id }), JsonConvert.SerializeObject(output));
            }
            return output;
        }
        public async Task<ResponseMessageDto> ActivityLog(AdvertiseLogDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var db_t = db.Tokens.Where(e => e.id == dto.FK_Tid).FirstOrDefault();
                if (db_t != null)
                {
                    var db_ad = db.Advertise.Where(e => e.Id == dto.FK_Aid).FirstOrDefault();
                    if(db_ad != null)
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

                        Core.Models.Advertise_Log adl = new Core.Models.Advertise_Log
                        {
                            FK_Adid = dto.FK_Aid,
                            FK_Tid = dto.FK_Tid,
                            FK_Uid = db_t.UserID,
                            Action = dto.Action,
                        };
                        db.Advertise_Logs.Add(adl);
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
    }
}
