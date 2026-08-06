using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marketing;
using EtheriT.Coker.Application.Shared.Marketing;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MarketingController : Controller
    {
        private readonly IMarketingAppService marketingAppService;

        public MarketingController(IMarketingAppService marketingAppService)
        {
            this.marketingAppService = marketingAppService;
        }

        [HttpPost]
        public async Task<ResponseMessageDto> GetCartMarketingCampaigns()
        {
            var response = await marketingAppService.GetCartMarketingCampaigns();
            if (response.Object is CartMarketingCampaignsDto campaigns)
            {
                foreach (var item in campaigns.AddOnCampaigns.SelectMany(x => x.RewardItems))
                    item.ImageUrl = ToPublicProductImageUrl(item.ImageUrl);
            }
            return response;
        }

        [HttpGet]
        public async Task<ResponseMessageDto> GetProductAddOnCampaigns(long productId)
        {
            var response = await marketingAppService.GetProductAddOnCampaigns(productId);

            // FileUploadAppService 回傳後台使用的 /upload/{orgName}/Product/...；
            // 前台的 upload 目錄已依目前網站完成對應，不應再帶 orgName。
            if (response.Object is IEnumerable<ProductAddOnCampaignDto> campaigns)
            {
                foreach (var item in campaigns.SelectMany(x => x.RewardItems))
                {
                    item.ImageUrl = ToPublicProductImageUrl(item.ImageUrl);
                }
            }

            return response;
        }

        private static string ToPublicProductImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return "/images/noImg.jpg";

            const string uploadPrefix = "/upload/";
            const string productFolder = "/Product/";

            if (!imageUrl.StartsWith(uploadPrefix, StringComparison.OrdinalIgnoreCase))
                return imageUrl;

            var productFolderIndex = imageUrl.IndexOf(
                productFolder,
                uploadPrefix.Length,
                StringComparison.OrdinalIgnoreCase);

            // 已是 /upload/Product/... 時不處理；只有夾著 orgName 時才移除。
            if (productFolderIndex <= uploadPrefix.Length)
                return imageUrl;

            return uploadPrefix + imageUrl[(productFolderIndex + 1)..];
        }
    }
}
