using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Newsletter;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Newsletter
{
    public interface INewsletterAppService
    {
        public Task<JsonResult> GetRecipients(DataSourceLoadOptions loadOptions);
        public Task<ResponseMessageDto> RecipientAddUp(DevExpressDto dto);
        public Task<ResponseMessageDto> DeleteRecipients(long Id);
        public Task<ResponseMessageDto> Send(long Id);
        public Task<ResponseMessageDto> UpdateJson(NewsletterFrameDto dto);
        public Task<ResponseMessageDto> SaveConten(HtmlContentDetailDto dto);
        public Task<ResponseMessageDto> UpdateText(NewsletterTextUpdateDto dto);
    }
}
