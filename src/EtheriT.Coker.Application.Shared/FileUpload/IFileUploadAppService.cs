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
        public Task<UploadFileOutputDto> uploadImageFiles(IList<IFormFile> files, int type, long sid, string page);
        public Task<UploadFileOutputDto> getHtmlContentFiles();
        public Task<string> getImgUrl(long? imgid, long websiteid);
        public Task<List<FileGetImgDto>> getImgFiles(FileGetImgInputDto dto);
        public Task<ResponseMessageDto> deleteFile(Guid key);
        public Task<ResponseMessageDto> deleteImgByImgId(long? imgid);
        public Task<ResponseMessageDto> deleteImgBySId(FileGetImgInputDto dto);
		public Task<ResponseMessageDto> deleteFile(string path);
    }
}
