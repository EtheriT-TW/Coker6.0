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

namespace EtheriT.Coker.Application.HtmlContent
{
    public class HtmlContentAppService : IHtmlContentAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        public HtmlContentAppService(
            CokerDbContext db,
            LoginUserData loginUserData
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
        }
        public async Task<ResponseMessageDto> AddUp(HtmlContentDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                if (dto.Id == 0)
                {
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    long WebsiteID = await loginUserData.GetWebsiteId();
                    if (db_t != null)
                    {
                        Core.Models.Html_Content hc = new Core.Models.Html_Content
                        {
                            FK_WebsiteId = WebsiteID,
                            Type = dto.Type,
                            Title = dto.Title,
                            Img = dto.Img,
                            Html = dto.Html,
                            Ser_no = dto.Ser_no,
                            Disp_opt = dto.Disp_opt,
                            ObjectType = dto.ObjectType,
                            Link = dto.Link,
                            Target = dto.Target,
                            StartDate = dto.StartDate,
                            EndDate = dto.EndDate,
                            permanent = dto.permanent,
                            CreatorUserId = (long)db_t.UserID
                        };
                        db.Html_Contents.Add(hc);
                    }
                    else throw new Exception("查無資料");
                }
                else
                {
                    var db_hc = db.Html_Contents.Where(e => e.Id == dto.Id).FirstOrDefault();
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    if (db_hc != null && db_t != null)
                    {
                        db_hc.Type = dto.Type;
                        db_hc.Title = dto.Title;
                        db_hc.Img = dto.Img;
                        db_hc.Html = dto.Html;
                        db_hc.Ser_no = dto.Ser_no;
                        db_hc.Disp_opt = dto.Disp_opt;
                        db_hc.ObjectType = dto.ObjectType;
                        db_hc.Link = dto.Link;
                        db_hc.Target = dto.Target;
                        db_hc.StartDate = dto.StartDate;
                        db_hc.EndDate = dto.EndDate;
                        db_hc.permanent = dto.permanent;

                        db_hc.LastModificationTime = DateTime.Now;
                        db_hc.LastModifierUserId = db_t.UserID;
                    }
                    else throw new Exception("查無資料");
                }
                db.SaveChanges();
                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
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
    }
}
