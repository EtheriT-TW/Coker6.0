
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public class LinePayConfirmResponseDto
    {
        public string ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public LinePayConfirmInfoDto? Info { get; set; }
    }
}
