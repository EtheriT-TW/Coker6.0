using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Search;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Search
{
    public interface ICustSearchAppService
    {
        public Task<JsonResult> GetAll(DataSourceLoadOptions loadOptions);
        public Task<CustSearchEditDto?> GetDataOne(long id);
        public Task<CustSearchOptionsDto> GetOptions();
        public Task<ResponseMessageDto> AddUp(CustSearchEditDto dto);
        public Task<ResponseMessageDto> Delete(long id);
        public Task<ResponseMessageDto> SetDefault(long id);
        public Task<List<SearchItemDto>> GetSearchList(long sid);
        public Task SaveSearchLog(SaveSearchLogDto dto);
        public Task<SearchKeyListDto> GetSearchKeyList(SearchKeyInputDto dto);
    }
}
