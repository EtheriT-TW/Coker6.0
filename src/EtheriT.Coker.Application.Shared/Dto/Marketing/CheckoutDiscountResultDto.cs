using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class CheckoutDiscountResultDto
    {
        public decimal TotalDiscountAmount { get; set; }

        public List<CheckoutDiscountAppliedDto> AppliedDiscounts { get; set; } = new();

        public List<CheckoutBenefitDto> Benefits { get; set; } = new();

        public string? Memo { get; set; }
    }
}
