using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;

namespace EtheriT.Coker.Application.Shared.Marquee
{
    public interface IMarqueeAppService
    {
        public Task<ResponseMessageDto> Add(MarqueeDto dto);
        public Task<MarqueeDto> Get(int id);
    }
}
