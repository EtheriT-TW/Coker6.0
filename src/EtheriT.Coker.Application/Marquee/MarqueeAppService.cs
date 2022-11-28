using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EtheriT.Coker.Application.Marquee
{
    public class MarqueeAppService : IMarqueeAppService
    {
        private readonly CokerDbContext db;
        public MarqueeAppService(
            CokerDbContext db
        )
        {
            this.db = db;
        }
        public async Task<ResponseMessageDto> Add(MarqueeAddDto dto)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                Core.Models.Marquee m = new Core.Models.Marquee
                {
                    FK_WebsiteId = dto.WebsiteId,
                    placement = dto.placement,
                    title = dto.title,
                    disp_opt = dto.disp_opt,
                    ser_no = dto.ser_no,
                    link = dto.link,
                    target = dto.target,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    permanent = dto.permanent
                };
                db.Marquees.Add(m);
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
        public async Task<ResponseMessageDto> Update(MarqueeUpdateDto dto)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var result = db.Marquees.Where(e => e.Id == dto.Id).FirstOrDefault();

                if (result != null)
                {
                    result.FK_WebsiteId = dto.WebsiteId;
                    result.placement = dto.placement;
                    result.title = dto.title;
                    result.disp_opt = dto.disp_opt;
                    result.ser_no = dto.ser_no;
                    result.link = dto.link;
                    result.target = dto.target;
                    result.StartTime = dto.StartTime;
                    result.EndTime = dto.EndTime;
                    result.permanent = dto.permanent;
                    result.LastModificationTime = DateTime.Now;
                    db.SaveChanges();
                    output.Success = true;
                }
                else throw new Exception("查無跑馬燈資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<MarqueeGetDto> Get(int id)
        {
            try
            {
                var result = db.Marquees.Where(e => e.Id == id && !e.IsDeleted).FirstOrDefault();

                if (result != null)
                {
                    MarqueeGetDto output = new MarqueeGetDto()
                    {
                        Id = result.Id,
                        placement = result.placement,
                        title = result.title,
                        disp_opt = result.disp_opt,
                        ser_no = result.ser_no,
                        link = result.link,
                        target = result.target,
                        StartTime = result.StartTime,
                        EndTime = result.EndTime,
                        permanent = result.permanent
                    };

                    return output;
                }
                else throw new Exception("查無跑馬燈資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<JsonResult> GetAll(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = db.Marquees;

                if (result != null)
                {
                    var dataQuery = from e in result
                                        where !e.IsDeleted
                                        select new MarqueeGetDto
                                        {
                                            Id = e.Id,
                                            placement = e.placement,
                                            title = e.title,
                                            disp_opt = e.disp_opt,
                                            ser_no = e.ser_no,
                                            link = e.link,
                                            target = e.target,
                                            StartTime = e.StartTime,
                                            EndTime = e.EndTime,
                                            permanent = e.permanent
                                        };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無跑馬燈資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<MarqueeGetDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetAll(long webid, string placement)
        {
            try
            {
                var result = db.Marquees;

                if (result != null)
                {
                    var output = await (from e in result 
                                        where e.FK_WebsiteId == webid && e.disp_opt && !e.IsDeleted && e.placement == placement 
                                        where e.permanent || ((DateTime.Compare((DateTime)e.StartTime, DateTime.Now) < 0) && (DateTime.Compare((DateTime)e.EndTime, DateTime.Now) > 0))
                                        orderby e.ser_no
                                        select new MarqueeDisplayDto
                                        {
                                            title = e.title,
                                            link = e.link,
                                            target = e.target
                                        }).Take(10).ToArrayAsync();
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無跑馬燈資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<MarqueeGetDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() }); ;
        }
        public async Task<ResponseMessageDto> Delete(int id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var result = db.Marquees.Where(e => e.Id == id).FirstOrDefault();

                if (result != null)
                {
                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    db.SaveChanges();
                    output.Success = true;
                }
                else throw new Exception("查無跑馬燈資料");
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
