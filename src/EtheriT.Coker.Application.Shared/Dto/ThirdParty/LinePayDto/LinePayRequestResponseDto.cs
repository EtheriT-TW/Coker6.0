
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public class LinePayRequestResponseDto
    {
        public string ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public LinePayRequestInfoDto? Info { get; set; }
    }
}
