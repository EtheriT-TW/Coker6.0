using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Contact;
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
    }
}
