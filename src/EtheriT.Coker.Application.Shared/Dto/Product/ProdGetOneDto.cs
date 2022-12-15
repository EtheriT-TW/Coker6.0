
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
	public class ProdGetOneDto
	{
		public long Id { get; set; }
        public string Title { get; set; }
        public string Introduction { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double? Discount { get; set; }
    }
}
