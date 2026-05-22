using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Contact;
using EtheriT.Coker.Application.Shared.Dto.Contact;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Contact
{
	public interface IContactAppService
	{
		public Task<ResponseMessageDto> submit(FormSubmitDto dto);
        public Task<JsonResult> GetContactListAll(DataSourceLoadOptions loadOptions);
        public Task<ResponseMessageDto> GetDataOne(long id);
        public Task<ResponseMessageDto> ReplyContact(ContactReplyDto dto);

        /// <summary>
        /// 取得目前站台可匯出的聯絡表單類別清單。
        /// </summary>
        public Task<ResponseMessageDto> GetContactExportFormTypesAsync();

        /// <summary>
        /// 依匯出條件產生聯絡表單 Excel 檔案。
        /// </summary>
        public Task<ContactExportResultDto> ExportContactsAsync(ContactExportRequestDto dto);
    }
}
