using AutoMapper;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Advertise;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Freight;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FileUploadController : Controller
    {
        private readonly IFileUploadAppService fileUploadAppService;

        public FileUploadController(IFileUploadAppService fileUploadAppService)
        {
            this.fileUploadAppService = fileUploadAppService;
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ResponseMessageDto> uploadFiles(IList<IFormFile> files, [FromForm] int type, [FromForm] long? id, [FromForm] long? sid, [FromForm] int serno, [FromForm] bool convert = true)
        {
            FileBindTypeEnum s = (FileBindTypeEnum)type;
            switch (s)
            {
                case FileBindTypeEnum.產品:
                    return await fileUploadAppService.uploadMediaFiles(files, type, (long)sid, serno, "Product", convert);
                case FileBindTypeEnum.產品檔案:
                    return await fileUploadAppService.uploadFiles(files, type, (long)sid, serno, "Product/File");
                case FileBindTypeEnum.選單圖:
                    return await fileUploadAppService.uploadMediaFiles(files, type, (long)sid, serno, "Menu", convert);
                case FileBindTypeEnum.選單Icon:
                    return await fileUploadAppService.uploadMediaFiles(files, type, (long)sid, serno, "MenuIcon", convert);
                case FileBindTypeEnum.選單覆蓋:
                    return await fileUploadAppService.uploadMediaFiles(files, type, (long)sid, serno, "MenuMouseOver", convert);
                case FileBindTypeEnum.技術證照:
                    return await fileUploadAppService.uploadMediaFiles(files, type, (long)sid, serno, "TechnicalCertificate", convert);
                case FileBindTypeEnum.右側浮動廣告:
                    return await fileUploadAppService.uploadMediaFiles(files, type, (long)sid, serno, "RightSideAd", convert);
                case FileBindTypeEnum.文章管理:
                    return await fileUploadAppService.uploadMediaFiles(files, type, (long)sid, serno, "Article", convert);
                case FileBindTypeEnum.進入廣告:
                    return await fileUploadAppService.uploadMediaFiles(files, type, (long)sid, serno, "EnterAd", convert);
                case FileBindTypeEnum.自訂廣告:
                    return await fileUploadAppService.uploadMediaFiles(files, type, (long)sid, serno, "CustomAd", convert);
                case FileBindTypeEnum.網站圖示:
                case FileBindTypeEnum.網站Logo:
                    return await fileUploadAppService.uploadMediaFiles(files, type, (long)sid, serno, "Website", convert);
                default:
                    return await fileUploadAppService.uploadHtmlContentFiles(files);
            }
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<UploadFileOutputDto> upload360Files(IList<IFormFile> files, [FromForm] int type, [FromForm] long? sid)
        {
            FileBindTypeEnum s = (FileBindTypeEnum)type;
            switch (s)
            {
                case FileBindTypeEnum.產品:
                    return await fileUploadAppService.upload360Files(files, type, (long)sid, "Product");
                default:
                    return null;
            }
        }
        [HttpPost]
        public async Task<ResponseMessageDto> uploadYTLink(FileYTLinkUploadDto dto)
        {
            FileBindTypeEnum s = (FileBindTypeEnum)dto.Type;
            switch (s)
            {
                case FileBindTypeEnum.產品:
                case FileBindTypeEnum.自訂廣告:
                    return await fileUploadAppService.uploadYTLink(dto);
                default:
                    return null;
            }
        }
        [HttpPost]
        public async Task<ResponseMessageDto> getFileList(GetFileListDto dto)
        {
            return await fileUploadAppService.getHtmlContentFiles(dto);
        }
        [HttpPost]
        public async Task<List<FileGetImgDto>> getImgFiles(FileGetImgInputDto dto)
        {
            return await fileUploadAppService.getImgFiles(dto);
        }
        [HttpGet]
        public async Task<List<FileGetDisplayDto>> getAdvertiseFiles(long Aid, int type)
        {
            return await fileUploadAppService.getAdvertiseFiles(Aid, type);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> fileSortChange(FileChangeSortDto dto)
        {
            return await fileUploadAppService.fileSortChange(dto);
        }
        [HttpDelete]
        public async Task<ResponseMessageDto> DeleteFile(DeleteDtoByKey dto)
        {
            return await fileUploadAppService.deleteFile(dto.key);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> DeleteFileById(FileDeleteDto dto)
        {
            return await fileUploadAppService.deleteFileById(dto);
        }
    }
}
