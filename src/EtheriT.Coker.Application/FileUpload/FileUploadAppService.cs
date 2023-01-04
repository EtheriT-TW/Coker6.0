using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.FileUpload
{
    public class FileUploadAppService
    {
        public readonly FileAllow fileAllow;
        public FileUploadAppService(FileAllow fileAllow) {
            this.fileAllow = fileAllow;
        }
        public async Task<ResponseMessageDto> uploadFile(IFormFile file) {
            
            return new ResponseMessageDto();
        }
        public async Task<ResponseMessageDto> uploadFiles(List<IFormFile> file) {

            return new ResponseMessageDto();
        }
    }
}
