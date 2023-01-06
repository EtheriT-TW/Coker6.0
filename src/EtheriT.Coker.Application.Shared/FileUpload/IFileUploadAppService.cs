using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public interface IFileUploadAppService
    {
        public Task<UploadFileOutputDto> uploadHtmlContentFiles(IList<IFormFile> files);
        public Task<UploadFileOutputDto> uploadProdtFiles(IList<IFormFile> files,long id);
        public Task<UploadFileOutputDto> getHtmlContentFiles();
        public Task<ResponseMessageDto> deleteFile(Guid key);
    }
}
