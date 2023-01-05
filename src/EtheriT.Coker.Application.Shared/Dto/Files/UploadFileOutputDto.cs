using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto.Files
{
    public class UploadFileOutputDto: ResponseMessageDto
    {
        public List<FileItemDto>? Files { get; set; }
        public List<string>? ErrorFiles { get; set; }
    }
}
