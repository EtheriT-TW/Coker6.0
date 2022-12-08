using EtheriT.Coker.Application.Shared.Dto.Product;

namespace EtheriT.Coker.Application.Shared.Product
{
	public interface IProductAppService
	{
        public Task<ProdGetOneDto> GetProdOne(long id);
    }
}
