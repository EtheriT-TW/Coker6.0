using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
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
        private readonly LoginUserData loginUserData;
        private readonly IFileUploadAppService fileUploadAppService;
        public TechnicalCertificateAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IFileUploadAppService fileUploadAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.fileUploadAppService = fileUploadAppService;
        }
        public async Task<ResponseMessageDto> AddUp(TechCertDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                if (dto.Id == 0)
                {
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    if (db_t != null)
                    {
                        long WebsiteID = await loginUserData.GetWebsiteId();
                        Core.Models.TechnicalCertificate tc = new Core.Models.TechnicalCertificate
                        {
                            FK_WebsiteId = WebsiteID,
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
                        db.SaveChanges();
                        output.Message = tc.Id.ToString();
                    }
                    else throw new Exception("查無資料");
                }
                else
                {
                    var db_tc = db.TechnicalCertificates.Where(e => e.Id == dto.Id).FirstOrDefault();
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    if (db_tc != null && db_t != null)
                    {
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
                        db.SaveChanges();
                        output.Message = db_tc.Id.ToString();
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
            return output;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long webid = await loginUserData.GetWebsiteId();

                var dataQuery = from e in db.TechnicalCertificates
                                where !e.IsDeleted && e.FK_WebsiteId == webid
                                select new TechCertGetAllListDto
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
            catch (Exception e)
            {

            }
            return new JsonResult(new List<TechCertGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetChoseList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long webid = await loginUserData.GetWebsiteId();

                var dataQuery = from tc in db.TechnicalCertificates
                                where !tc.IsDeleted && tc.FK_WebsiteId == webid
                                select new TechCertGetAllChoseListDto
                                {
                                    Id = tc.Id,
                                    Img = tc.Img,
                                    Title = tc.Title
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<TechCertGetAllChoseListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<TechCertDto> GetOne(int id)
        {
            try
            {
                var result = db.TechnicalCertificates.Where(e => e.Id == id && !e.IsDeleted).FirstOrDefault();

                if (result != null)
                {
                    TechCertDto output = new TechCertDto()
                    {
                        Id = result.Id,
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
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var db_tc = db.TechnicalCertificates.Where(e => e.Id == Id).FirstOrDefault();
                long usetId = await loginUserData.GetUserId();

                if (db_tc != null)
                {
                    if (db_tc.Img != "")
                    {
                        var delete_img = await fileUploadAppService.deleteImg(long.Parse(db_tc.Img));
                    }

                    db_tc.IsDeleted = true;
                    db_tc.DeletionTime = DateTime.Now;
                    db_tc.DeleterUserId = usetId;
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
