using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application
{
    public interface IFileUploadAppService
    {
        public Task<UploadFileOutputDto> uploadTempFiles(IList<IFormFile> files);
        public Task<UploadFileOutputDto> uploadHtmlContentFiles(IList<IFormFile> files);
        public Task<UploadFileOutputDto> uploadMediaFiles(IList<IFormFile> files, int type, long sid, int serno, string page, bool convert);
        public Task<UploadFileOutputDto> uploadFiles(IList<IFormFile> files, int type,long id, long sid, int serno, string page, bool isEncryption);
        public Task<UploadFileOutputDto> upload360Files(IList<IFormFile> files, int type, long? sid, string page);
        public Task<ResponseMessageDto> uploadYTLink(FileYTLinkUploadDto dto);
        public Task<ResponseMessageDto> uploadImageLink(List<FileImageImportDto> dto);
		public Task<ResponseMessageDto> uploadImageLink(FileImageImportDto dto);
        public Task<string> getImgUrl(long? imgid, long websiteid);
        public Task<List<FileGetImgDto>> getImgFiles(FileGetImgInputDto dto);
        public Task<List<FileGetImgDto>> getImgsFiles(FileGetImgsInputDto dto);
        public Task<List<string>> getImgFilesById(List<long> Ids, int size);
        public Task<List<FileGetProdDisplayDto>> getProdFiles(long Pid);
        public Task<List<FileGetArticleDisplayDto>> getArticleFiles(long Aid);
        public Task<List<FileGetDisplayDto>> getAdvertiseFiles(long Aid, int type);
        public Task<List<FileGetProdDisplayDto>> getProdMultimedia(long Pid, int size);
        public Task<ResponseMessageDto> fileSortChange(FileChangeSortDto dto);
        public Task<ResponseMessageDto> deleteFile(Guid key);
        public Task<ResponseMessageDto> deleteFileById(FileDeleteDto dto);
        public Task<ResponseMessageDto> deleteFile(string path);
        public Task<ResponseMessageDto> insertNotFondFile(InsertNotFoundFileDto dto);
        public Task<Dictionary<long, string>> GetMinImageMapAsync(List<long> prodIds);
        public Task<DownloadPayload> DecryptFile(long fid);

    }
}
