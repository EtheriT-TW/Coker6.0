using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.EnterAd;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;

namespace EtheriT.Coker.Application.TechnicalCertificate
{
    public class TechnicalCertificateAppService : ITechnicalCertificateAppService
    {
        private readonly CokerDbContext db;
        public TechnicalCertificateAppService(
            CokerDbContext db
        )
        {
            this.db = db;
        }
        public async Task<ResponseMessageDto> AddUp(TechnicalCertificateDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                if (dto.Id == 0)
                {
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    if (db_t != null)
                    {
                        Core.Models.TechnicalCertificate tc = new Core.Models.TechnicalCertificate
                        {
                            FK_WebsiteId = dto.FK_WebsiteId,
                            Disp_opt = dto.Disp_opt,
                            Img = dto.Img,
                            Title = dto.Title,
                            Description = dto.Description,
                            Ser_no = dto.Ser_no,
                            StartDate = dto.StartDate,
                            EndDate = dto.EndDate,
                            Permanent = dto.Permanent,
                            CreatorUserId = (long)db_t.UserID
                        };
                        db.TechnicalCertificates.Add(tc);
                    }
                    else throw new Exception("查無資料");
                }
                else
                {
                    var db_tc = db.TechnicalCertificates.Where(e => e.Id == dto.Id).FirstOrDefault();
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    if (db_tc != null && db_t != null)
                    {

                        db_tc.FK_WebsiteId = dto.FK_WebsiteId;
                        db_tc.Disp_opt = dto.Disp_opt;
                        db_tc.Img = dto.Img;
                        db_tc.Title = dto.Title;
                        db_tc.Description = dto.Description;
                        db_tc.Ser_no = dto.Ser_no;
                        db_tc.StartDate = dto.StartDate;
                        db_tc.EndDate = dto.EndDate;
                        db_tc.Permanent = dto.Permanent;
                        db_tc.CreatorUserId = (long)db_t.UserID;

                        db_tc.LastModificationTime = DateTime.Now;
                        db_tc.LastModifierUserId = db_t.UserID;
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
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = db.TechnicalCertificates;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted
                                    select new TechnicalCertificateGetAllListDto
                                    {
                                        Id = e.Id,
                                        Disp_opt = e.Disp_opt,
                                        Img = e.Img,
                                        Title = e.Title,
                                        Description = e.Description,
                                        Ser_no = e.Ser_no,
                                        StartDate = e.StartDate,
                                        EndDate = e.EndDate,
                                        Permanent = e.Permanent,
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }
            return new JsonResult(new List<TechnicalCertificateGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<TechnicalCertificateDto> GetOne(int id)
        {
            try
            {
                var result = db.TechnicalCertificates.Where(e => e.Id == id && !e.IsDeleted).FirstOrDefault();

                if (result != null)
                {
                    TechnicalCertificateDto output = new TechnicalCertificateDto()
                    {
                        Id = result.Id,
                        FK_WebsiteId = result.FK_WebsiteId,
                        Disp_opt = result.Disp_opt,
                        Img = result.Img,
                        Title = result.Title,
                        Description = result.Description,
                        Ser_no = result.Ser_no,
                        StartDate = result.StartDate,
                        EndDate = result.EndDate,
                        Permanent = result.Permanent,
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
        public async Task<ResponseMessageDto> Delete(TechnicalCertificateDelectDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var db_hc = db.TechnicalCertificates.Where(e => e.Id == dto.Id).FirstOrDefault();
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
