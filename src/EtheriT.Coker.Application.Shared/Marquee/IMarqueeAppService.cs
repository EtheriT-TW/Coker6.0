using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;

namespace EtheriT.Coker.Application.Shared.Marquee
{
    public interface IMarqueeAppService
    {

        public Task<ResponseMessageDto> Add(MarqueeAddDto dto);
        public Task<ResponseMessageDto> Update(MarqueeUpdateDto dto);
        public Task<MarqueeGetDto> Get(int id);
        public Task<List<MarqueeGetDto>> GetAll();
        public Task<ResponseMessageDto> Delete(int id);
    }
}
