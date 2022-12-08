using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;

namespace EtheriT.Coker.Application.Product
{
    public class ProductAppService : IProductAppService
    {
        private readonly CokerDbContext db;
        public ProductAppService(
            CokerDbContext db
        )
        {
            this.db = db;
        }

        public async Task<ProdGetOneDto> GetProdOne(long id)
        {
            try
            {
                var result = db.Prods.Where(e => e.Id == id).FirstOrDefault();

                if (result != null)
                {
                    ProdGetOneDto output = new ProdGetOneDto()
                    {
                        Id = result.Id,
                        Title = result.Title,
                        Introduction = result.Introduction,
                        Description = result.Description,
                        Price = result.Price
                    };
                    return output;
                }
                else throw new Exception("查無跑馬燈資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
    }
}
