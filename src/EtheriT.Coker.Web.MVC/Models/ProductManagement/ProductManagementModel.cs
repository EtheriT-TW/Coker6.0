using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Application.Shared.Dto.Specification;

namespace EtheriT.Coker.Web.MVC.Models.ProductManagement
{
    public class ProductManagementModel
    {
        public List<SpecTypeListDto> SpecType { get; set; }
        public List<AddRoleDto>? Roles { get; set; }
        public List<ProdStatusEnum> ProdStatus { get; set; }
    }
}
