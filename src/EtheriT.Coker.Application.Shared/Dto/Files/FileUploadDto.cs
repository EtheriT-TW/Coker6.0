using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class FileUploadDto
    {
        public IList<IFormFile> file { get; set; }
        public int type { get; set; }
        public long? id { get; set; }
    }
}
