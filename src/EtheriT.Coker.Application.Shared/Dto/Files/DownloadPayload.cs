
namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class DownloadPayload
    {
        public bool Success { get; set; } = true;
        public bool IsEncryptedFile { get; set; }
        public string? PhysicalPath { get; set; }
        public byte[]? Bytes { get; set; }
        public string? ContentType { get; set; }
        public string? FileName { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
