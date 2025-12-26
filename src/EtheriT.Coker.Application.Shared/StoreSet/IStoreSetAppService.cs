using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Dto.StoreSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;

namespace EtheriT.Coker.Application.StoreSet
{
    public interface IStoreSetAppService
    {
        public Task<StoreSetResponseMessageDto> getAll(List<long> StoreSetGroupId);
        public Task<StoreSetResponseMessageDto> find(string key);
        public Task<StoreSetResponseMessageDto> getValues(StoreSetGetValueInput dto);
        public Task<StoreSetResponseMessageDto> getGroupStructure(StoreSetGetValueInput dto);
        public Task<ResponseMessageDto> CreateOrUpdate(List<StoreSetDetailOutputDto> datas);
    }
}
