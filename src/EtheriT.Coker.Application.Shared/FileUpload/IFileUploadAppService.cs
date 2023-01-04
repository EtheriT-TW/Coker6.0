using EtheriT.Coker.Application.Dto;
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
        public Task<ResponseMessageDto> uploadFile(IFormFile file); 
        public Task<ResponseMessageDto> uploadFiles(List<IFormFile> file);
    }
}
