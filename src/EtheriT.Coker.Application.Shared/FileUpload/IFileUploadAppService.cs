using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Files;
using Microsoft.AspNetCore.Http;

namespace EtheriT.Coker.Application
{
    public interface IFileUploadAppService
    {
		public Task<UploadFileOutputDto> uploadTempFiles(IList<IFormFile> files);
		public Task<UploadFileOutputDto> uploadHtmlContentFiles(IList<IFormFile> files);
        public Task<UploadFileOutputDto> uploadProdtFiles(IList<IFormFile> files, long id);
        public Task<UploadFileOutputDto> uploadTechnicalCertificateFiles(IList<IFormFile> files, int type, long sid);
        public Task<UploadFileOutputDto> getHtmlContentFiles();
        public Task<string> getImgUrl(long? imgid, long websiteid);
        public Task<List<ImgGetDto>> getImgThumbnail(long? tid);
        public Task<ResponseMessageDto> deleteFile(Guid key);
        public Task<ResponseMessageDto> deleteImg(long? imgid);
    }
}
