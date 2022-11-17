using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;

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
        public async Task<ResponseMessageDto> Add(MarqueeDto dto)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                Core.Models.Marquee m = new Core.Models.Marquee
                {
                    FK_WebsiteId = dto.WebsiteId,
                    title = dto.title,
                    disp_opt = dto.disp_opt,
                    ser_no = dto.ser_no,
                    link = dto.link,
                    target = dto.target,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime
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
        public async Task<MarqueeDto> Get(int id)
        {
            try
            {
                var result = db.Marquees.Where(e => e.Id == id).FirstOrDefault();

                if (result != null)
                {
                    MarqueeDto output = new MarqueeDto()
                    {
                        WebsiteId = result.FK_WebsiteId,
                        title = result.title,
                        disp_opt = result.disp_opt,
                        ser_no = result.ser_no,
                        link = result.link,
                        target = result.target,
                        StartTime = result.StartTime,
                        EndTime = result.EndTime

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
    }
}
