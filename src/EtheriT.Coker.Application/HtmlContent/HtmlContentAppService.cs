using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.EnterAd;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using System.Web;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Files;

namespace EtheriT.Coker.Application.HtmlContent
{
    public class HtmlContentAppService : IHtmlContentAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly string ApplicationName;
        private readonly IFileUploadAppService fileUploadAppService;
        public HtmlContentAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            IFileUploadAppService fileUploadAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.fileUploadAppService = fileUploadAppService;
            ApplicationName = "HtmlContent";
        }
        public async Task<ResponseMessageDto> AddUp(HtmlContentDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                long userid = await loginUserData.GetUserId();
                var ascoid = dto.Id;
                dto.Html = HttpUtility.HtmlEncode(dto.Html);
                if (dto.Id == 0)
                {
                    if (userid != 0)
                    {
                        long WebsiteID = await loginUserData.GetWebsiteId();
                        Html_Content newItem = mapper.Map<Html_Content>(dto);
                        newItem.FK_WebsiteId = WebsiteID;
                        db.Html_Contents.Add(newItem);
                        await loginUserData.SaveChanges(newItem);
                        ascoid = newItem.Id;
                    }
                    else throw new Exception("查無資料");
                }
                else
                {
                    var db_hc = db.Html_Contents.Where(e => e.Id == dto.Id).FirstOrDefault();
                    if (db_hc != null && userid != 0)
                    {
                        mapper.Map(dto, db_hc);
                        await loginUserData.SaveChanges(db_hc);
                    }
                    else throw new Exception("查無資料");
                }
                output.Success = true;
                output.Message = ascoid.ToString();
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            await loginUserData.SetLogs(ApplicationName, "AddUp", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
            return output;
        }
        public async Task<HtmlContentListOutpotDto> GetAllComponent()
        {
            HtmlContentListOutpotDto respose = new HtmlContentListOutpotDto();
            try
            {
                List<long> t = Enum.GetValues(typeof(ObjectTypeEnum)).Cast<ObjectTypeEnum>()
                   .Select(e => { return (long)e; })
                   .ToList();
                var result = await db.Html_Contents
                        .Where(e => t.Contains(e.Type))
                        .Where(e => !e.IsDeleted)
                        .ToListAsync();
                respose.List = mapper.Map<List<HtmlContentDto>>(result);
                respose.Success = true;
            }
            catch (Exception e)
            {
                respose.Error = e.Message;
            }

            return respose;
        }
        public async Task<HtmlContentListOutpotDto> GetComponent(ObjectTypeEnum type)
        {
            HtmlContentListOutpotDto respose = new HtmlContentListOutpotDto();
            try
            {
                var result = await db.Html_Contents.Where(e => e.Id == (long)type).ToListAsync();
                respose.List = mapper.Map<List<HtmlContentDto>>(result);
                respose.Success = true;
            }
            catch (Exception e)
            {
                respose.Error = e.Message;
            }
            return respose;
        }
        public async Task<JsonResult> GetAllList(int type, DataSourceLoadOptions loadOptions)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Html_Contents;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted && e.Type == type && e.FK_WebsiteId == WebsiteID
                                    orderby e.Ser_no
                                    select new HtmlContentGetAllListDto
                                    {
                                        Id = e.Id,
                                        Type = e.Type,
                                        Title = e.Title,
                                        Img = e.Img,
                                        Html = e.Html,
                                        Ser_no = e.Ser_no,
                                        Disp_opt = e.Disp_opt,
                                        ObjectType = e.ObjectType,
                                        Link = e.Link,
                                        Target = e.Target,
                                        StartDate = e.StartDate,
                                        EndDate = e.EndDate,
                                        permanent = e.permanent
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    var htmlType = 0;
                    switch (type)
                    {
                        case (int)HtmlContentTypeEnum.右側浮動廣告:
                            htmlType = (int)FileBindTypeEnum.右側浮動廣告;
                            break;
                        case (int)HtmlContentTypeEnum.進入廣告:
                            htmlType = (int)FileBindTypeEnum.進入廣告;
                            break;
                    }
                    if (output != null && htmlType != -1)
                    {
                        foreach (var data in output.data)
                        {
                            var htmlId = data.GetType().GetProperty("Id").GetValue(data, null);
                            var getImgFileInput = new FileGetImgInputDto
                            {
                                Sid = (long)htmlId,
                                Type = htmlType,
                                Size = 1
                            };
                            var image = await fileUploadAppService.getImgFiles(getImgFileInput);
                            if (image.Count > 0)
                            {
                                data.GetType().GetProperty("Img").SetValue(data, image[0].Link);
                            }
                        }
                    }
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }
            return new JsonResult(new List<HtmlContentGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<HtmlContentDto> GetOne(int id)
        {
            try
            {

                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.Html_Contents.Where(e => e.Id == id && !e.IsDeleted && e.FK_WebsiteId == WebsiteID).FirstOrDefault();

                if (result != null)
                {
                    HtmlContentDto output = new HtmlContentDto()
                    {
                        Id = result.Id,
                        Type = result.Type,
                        Title = result.Title,
                        Img = result.Img,
                        Html = result.Html,
                        Ser_no = result.Ser_no,
                        Disp_opt = result.Disp_opt,
                        ObjectType = result.ObjectType,
                        Link = result.Link,
                        Target = result.Target,
                        StartDate = result.StartDate,
                        EndDate = result.EndDate,
                        permanent = result.permanent
                    };

                    return output;
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<JsonResult> GetDisplay(long webid, int type, int number)
        {
            try
            {
                var result = db.Html_Contents;

                if (result != null)
                {
                    var output = await (from e in result
                                        where !e.IsDeleted && e.Disp_opt && e.Type == type && e.FK_WebsiteId == webid
                                        where e.permanent || (DateTime.Compare(DateTime.Now, (DateTime)e.StartDate) > 0 && DateTime.Compare(DateTime.Now, (DateTime)e.EndDate) < 0)
                                        orderby e.Ser_no
                                        select new HtmlContentDisplayDto
                                        {
                                            Id = e.Id,
                                            Title = e.Title,
                                            Img = e.Img,
                                            Html = e.Html,
                                            Link = e.Link,
                                            Target = e.Target,
                                        }).Take(number).ToArrayAsync();

                    if (output != null)
                    {
                        for (var i = 0; i < output.Length; i++)
                        {
                            var imagedata = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
                            {
                                Sid = output[i].Id,
                                Type = (int)FileBindTypeEnum.右側浮動廣告,
                                Size = 1
                            });
                            output[i].Img = imagedata.Count <= 0 ? "" : imagedata.First().Link;
                        }
                    }

                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });

                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<HtmlContentDisplayDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long userid = await loginUserData.GetUserId();
                var db_hc = db.Html_Contents.Where(e => e.Id == Id).FirstOrDefault();

                if (db_hc != null && userid != 0)
                {
                    db_hc.IsDeleted = true;
                    await loginUserData.SaveChanges(db_hc);
                    switch (db_hc.Type)
                    {
                        case (int)HtmlContentTypeEnum.右側浮動廣告:
                            var delete_image_r = await fileUploadAppService.deleteFileById(new FileDeleteDto()
                            {
                                Sid = db_hc.Id,
                                Type = (int)FileBindTypeEnum.右側浮動廣告,
                            });
                            break;
                        case (int)HtmlContentTypeEnum.進入廣告:
                            var delete_image_e = await fileUploadAppService.deleteFileById(new FileDeleteDto()
                            {
                                Sid = db_hc.Id,
                                Type = (int)FileBindTypeEnum.進入廣告,
                            });
                            break;
                    }
                    output.Success = true;
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<HtmlContentTypeDto> GetTypeList()
        {
            HtmlContentTypeDto response = new HtmlContentTypeDto { Success = true };
            try
            {
                long userId = await loginUserData.GetUserId();
                if (userId == 1)
                {
                    response.Type = Enum.GetValues(typeof(ObjectTypeEnum))
                   .Cast<ObjectTypeEnum>().Select(e =>
                   {
                       return new EnumDictionaryDto { Key = e.ToString(), Value = (int)e };
                   }).ToList();
                }
                else
                {
                    response.Type = new List<EnumDictionaryDto>
                    {
                        new EnumDictionaryDto{ Key = ObjectTypeEnum.自訂.ToString(), Value = (int)ObjectTypeEnum.自訂 }
                    };
                }
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Error = e.Message;
            }
            return response;
        }
    }
}
