using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Specification;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Specification;
using EtheriT.Coker.Web.MVC.Models.ProductManagement;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ProductManagementController : Controller
    {
        private readonly IProductAppService productAppService;
        private readonly ISpecificationAppService specificationAppService;
        public ProductManagementController(IProductAppService productAppService, ISpecificationAppService specificationAppService)
        {
            this.productAppService = productAppService;
            this.specificationAppService = specificationAppService;
        }
        public async Task<IActionResult> ProductListAsync()
        {
            var spec_type = new List<SpecTypeListDto>(await specificationAppService.GetPickTypeList());
            ProductManagementModel model = new ProductManagementModel
            {
                SpecType = spec_type,
            };
            return View("ProductList", model);
        }
        public IActionResult TechnicalCertificate()
        {
            return View("TechnicalCertificate");
        }
        public IActionResult SpecSetting()
        {
            return View("SpecSetting");
        }
    }
}
