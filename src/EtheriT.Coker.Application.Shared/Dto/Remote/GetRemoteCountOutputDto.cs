using EtheriT.Coker.Application.Shared.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Remote
{
    public class GetRemoteCountOutputDto: ResponseObject
    {
        public List<RemoteListOtputDto> remoteListOtputDtos { get; set; }
        public long AllCount {  get; set; }
        public long AllMemCount { get; set; }
    }
}
