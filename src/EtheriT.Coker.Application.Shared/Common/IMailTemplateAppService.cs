using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.MailTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Common
{
    public interface IMailTemplateAppService
    {
        public Task<List<MailTemplateResultDto>> GetTemplateRenderAsync(MailTemplateTypeEnum templateType, List<MailTemplateInputDto> input);
    }
}
