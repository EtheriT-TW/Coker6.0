using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Application.Shared.Dto.Specification;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Specification;
using EtheriT.Coker.Web.MVC.Models.ProductManagement;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ProductManagementController : Controller
    {
        private readonly IProductAppService productAppService;
        private readonly ISpecificationAppService specificationAppService;
        private readonly IStoreSetAppService storeSetAppService;
        public ProductManagementController(IProductAppService productAppService, ISpecificationAppService specificationAppService, IStoreSetAppService storeSetAppService)
        {
            this.productAppService = productAppService;
            this.specificationAppService = specificationAppService;
            this.storeSetAppService = storeSetAppService;
        }
        public async Task<IActionResult> ProductListAsync()
        {
            var spec_type = new List<SpecTypeListDto>(await specificationAppService.GetPickTypeList());
            var chackHasAnyItemNo = await productAppService.HasAnyItemNo();
            var storeBuyStatus = await storeSetAppService.getValues(new StoreSetGetValueInput { key = "storeBuyState" });
            bool priceOptional = storeBuyStatus.Success && storeBuyStatus.detailItem?.value != null && storeBuyStatus.detailItem.value.Contains("noPayNoShow");

            ProductManagementModel model = new ProductManagementModel
            {
                SpecType = spec_type,
                ProdStatus = Enum.GetValues(typeof(ProdStatusEnum)).Cast<ProdStatusEnum>().ToList(),
                Roles = JsonConvert.DeserializeObject<List<AddRoleDto>>(JsonConvert.SerializeObject((await productAppService.GetRolesAll()).Value)),
                HasAnyItemNo = chackHasAnyItemNo.Success,
                PriceOptional = priceOptional
            };
            return View("ProductList", model);
        }
        public async Task<IActionResult> SaleQuantityStaging() {
            return View("SaleQuantityStaging");
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
