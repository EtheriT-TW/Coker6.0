using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Web.MVC.Models.ProductManagement;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ProductManagementController : Controller
    {
        private readonly IProductAppService productAppService;
        public ProductManagementController(IProductAppService productAppService)
        {
            this.productAppService = productAppService;
        }
        public async Task<IActionResult> ProductListAsync()
        {
            var spec_type = new List<ProdIdTitleDto>(await productAppService.GetSpecType());
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
