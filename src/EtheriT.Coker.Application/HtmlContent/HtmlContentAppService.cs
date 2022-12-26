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
using EtheriT.Coker.Application.Shared.Dto;
using AutoMapper;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Application.HtmlContent
{
    public class HtmlContentAppService : IHtmlContentAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly string ApplicationName;
        public HtmlContentAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            ApplicationName = "HtmlContent";
        }
        public async Task<ResponseMessageDto> AddUp(HtmlContentDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                long userid = await loginUserData.GetUserId();
                if (dto.Id == 0)
                {
                    if (userid != 0)
                    {
                        long WebsiteID = await loginUserData.GetWebsiteId();
                        Html_Content newItem = mapper.Map<Html_Content>(dto);
                        newItem.FK_WebsiteId = WebsiteID;
                        db.Html_Contents.Add(newItem);
                        await loginUserData.SaveChanges(newItem);
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
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            await loginUserData.SetLogs(ApplicationName, "AddUp", JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
            return output;
        }
        public async Task<JsonResult> GetAllList(int type, DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = db.Html_Contents;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted && e.Type == type
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
                var result = db.Html_Contents.Where(e => e.Id == id && !e.IsDeleted).FirstOrDefault();

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
                                            Title = e.Title,
                                            Img = e.Img,
                                            Html = e.Html,
                                            Link = e.Link,
                                            Target = e.Target,
                                        }).Take(number).ToArrayAsync();
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無運費資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<HtmlContentDisplayDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> Delete(DataDelectDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var db_hc = db.Html_Contents.Where(e => e.Id == dto.Id).FirstOrDefault();
                var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();

                if (db_hc != null && db_t != null)
                {
                    db_hc.IsDeleted = true;
                    db_hc.DeletionTime = DateTime.Now;
                    db_hc.DeleterUserId = db_t.UserID;
                    db.SaveChanges();
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
        public async Task<HtmlContentTypeDto> GetTypeList() {
            HtmlContentTypeDto response = new HtmlContentTypeDto { Success = true };
            try
            {
                long userId = await loginUserData.GetUserId();
                if (userId == 1)
                {
                    response.Type = Enum.GetValues(typeof(ObjectTypeEnum))
                   .Cast<ObjectTypeEnum>().Select(e => {
                       return new EnumDictionaryDto { Key = e.ToString(),Value = (int)e };
                   }).ToList();
                }
                else {
                    response.Type = new List<EnumDictionaryDto>
                    {
                        new EnumDictionaryDto{ Key = ObjectTypeEnum.自訂.ToString(), Value = (int)ObjectTypeEnum.自訂 }
                    };
                }
            }
            catch(Exception e)
            {
                response.Success = false;
                response.Error = e.Message;
            }
            return response;
        }
    }
}
