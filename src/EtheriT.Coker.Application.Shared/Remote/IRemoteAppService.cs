using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Remote;
using EtheriT.Coker.Application.Shared.Dto.UserHabits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Remote
{
	public interface IRemoteAppService
	{
		public Task<ResponseMessageDto> insertRemote(RemoteInputDto dto);
		public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
		public Task<JsonResult> GetPageList(DataSourceLoadOptions loadOptions);
		public Task<ResponseMessageDto> GetRemoteCount(GetRemoteCountInputDto dto);
		public Task UpdateRemoteTime(SetTrackTimeDto dto);

    }
}
