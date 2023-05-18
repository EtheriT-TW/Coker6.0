using AutoMapper;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Freight;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ResponseMessageDto> uploadFiles(IList<IFormFile> files, [FromForm] int type, [FromForm] long? id, [FromForm] long? sid)
        {
            FileBindTypeEnum s = (FileBindTypeEnum)type;
            switch (s)
            {
                case FileBindTypeEnum.產品:
                    return await fileUploadAppService.uploadProdtFiles(files, id ?? 0);
                case FileBindTypeEnum.技術證照:
                    return await fileUploadAppService.uploadTechnicalCertificateFiles(files, type, (long)sid);
                default:
                    return await fileUploadAppService.uploadHtmlContentFiles(files);
            }
            return null;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> getFileList(GetFileListDto dto)
        {
            switch ((FileBindTypeEnum)dto.type)
            {
                default:
                    return await fileUploadAppService.getHtmlContentFiles();
            }
        }
        [HttpGet]
        public async Task<List<FileGetImgDto>> getImgFiles(long? tid, int size)
        {
            return await fileUploadAppService.getImgFiles(tid, size);
        }
        [HttpDelete]
        public async Task<ResponseMessageDto> DeleteFile(DeleteDtoByKey dto)
        {
            return await fileUploadAppService.deleteFile(dto.key);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> DeleteImage(long? imgid)
        {
            return await fileUploadAppService.deleteImg(imgid);
        }
    }
}
