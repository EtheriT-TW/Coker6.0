using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Application.Shared.Dto.Remote
{
    public sealed class RemoteTrackingCollectDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        public Guid EventId { get; set; }

        [Range(0, 300)]
        public int VisibleSeconds { get; set; }

        public bool HasInteraction { get; set; }
    }
}
