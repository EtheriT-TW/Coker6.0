
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
	public class ProductDto
    {
        public long Id { get; set; }
        public long? FK_WebsiteId { get; set; }
        public string Title { get; set; }
        public bool Disp_Opt { get; set; }
        public int Ser_No { get; set; }
        public string Introduction { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double? Discount { get; set; }
        public long? FK_S1id { get; set; }
        public long? FK_S2id { get; set; }
        public int? Stock { get; set; }
        public int? Min_Qty { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public bool Permanent { get; set; }
    }
}
