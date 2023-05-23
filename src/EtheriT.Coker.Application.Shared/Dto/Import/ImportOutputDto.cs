using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Import
{
	public class ImportOutputDto: ResponseMessageDto
	{
		public List<ImportMassageItem> ErrorList { get;set; }

		public static implicit operator ImportOutputDto(UploadFileOutputDto v)
		{
			throw new NotImplementedException();
		}
	}
}
