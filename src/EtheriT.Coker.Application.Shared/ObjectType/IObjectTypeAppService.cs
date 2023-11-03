using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.ObjectType;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public interface IObjectTypeAppService
    {
        public Task<ObjectTypeGetAlldto> GetAll();
        public Task<ResponseMessageDto> CreateOrEdit(ObjectTypeItemDto dto);
        public Task<ResponseMessageDto> DeleteHtmlContent(DataDelectDto dto);
        public Task<ResponseMessageDto> UpdateSerNo(UpdateSerNoListDto dto);
        public Task<HtmlContentGetHtmlDto> GetConten(SearchIDDto dto);
        public Task<HtmlContentGetHtmlDto> GetNewsletterConten(); 
        public Task<ResponseMessageDto> SaveConten(HtmlContentDetailDto dto);
    }
}
