namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProdSetCoverImageDto
    {
        public long ProductId { get; set; }
        /// <summary>要設為預設圖片的 FileUpload.Id；null 或 0 表示取消指定</summary>
        public long? FileUploadId { get; set; }
    }
}
