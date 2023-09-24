using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto.StoreSet
{
	public class StoreSetResponseMessageDto: ResponseMessageDto
	{
		public List<StoreSetGroupOutputDto>? storeSets {  get; set; }
		public List<StoreSetDetailOutputDto>? storeSetDetails { get; set; }
		public StoreSetOutputDto? item {  get; set; }
		public StoreSetDetailOutputDto? detailItem { get; set; }
    }
}
