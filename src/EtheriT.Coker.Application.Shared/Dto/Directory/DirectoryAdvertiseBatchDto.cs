using EtheriT.Coker.Application.Shared.Dto.Advertise;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryAdvertiseBatchInputDto
    {
        public List<List<long>> Groups { get; set; } = new List<List<long>>();
        public int? Take { get; set; }
    }

    public class DirectoryAdvertiseBatchResultDto
    {
        public string Key { get; set; } = string.Empty;
        public List<long> DirectoryIds { get; set; } = new List<long>();
        public List<AdvertiseDisplayDto> Advertisements { get; set; } = new List<AdvertiseDisplayDto>();
    }
}
