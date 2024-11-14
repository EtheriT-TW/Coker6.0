namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public class LinePayPackageDto
    {
        public string id { get; set; }
        public string amount { get; set; }
        public string userFee { get; set; }
        public string name { get; set; }
        public List<LinePayProductsDto> products { get; set; }
    }
}
