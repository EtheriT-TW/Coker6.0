using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Remote
{
    public class GetRemoteCountInputDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //轉時區
        public DateTime ConvertToTimeZone(string timeZoneId, DateTime dateTime)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(dateTime, timeZone);
        }
    }
}
