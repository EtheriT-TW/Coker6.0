using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Dto;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.Newsletter;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Advertise;

namespace EtheriT.Coker.Application.Advertise
{
    public class AdvertiseAppService : IAdvertiseAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly StringHandler stringHandler;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly ITagAppService tagAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly string ServiceName;
        public AdvertiseAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            StringHandler stringHandler,
            IMapper mapper,
            IConfiguration configuration,
            ITagAppService tagAppService,
            IFileUploadAppService fileUploadAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.configuration = configuration;
            this.tagAppService = tagAppService;
            this.fileUploadAppService = fileUploadAppService;
            this.stringHandler = stringHandler;
            ServiceName = "Advertise";
        }
        public async Task<ResponseMessageDto> AddUp(AdvertiseDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tag_response = new ResponseMessageDto() { Success = false };

            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                long usetId = await loginUserData.GetUserId();
                var asoid = dto.Id;

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
                var result = db.Article.Where(e => e.Id == Id).FirstOrDefault();

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

                    //var delete_img_dto = new FileDeleteDto
                    //{
                    //    Sid = result.Id,
                    //    Type = (int)FileBindTypeEnum.自訂廣告
                    //};
                    //var imgdelete_response = await fileUploadAppService.deleteFileById(delete_img_dto);

                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    result.DeleterUserId = usetId;

                    db.SaveChanges();

                    output.Success = tagdeleteresponse.Success;
                    //output.Success = tagdeleteresponse.Success && imgdelete_response.Success;
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
                await loginUserData.SetLogs(ServiceName, "Delete", JsonConvert.SerializeObject(new { Id }), JsonConvert.SerializeObject(output));
            }
            return output;
        }
    }
}
