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
        private readonly EtheriT.Coker.Application.LoginUserData loginUserData;

        public MarketingController(
            IMarketingAppService marketingAppService,
            EtheriT.Coker.Application.LoginUserData loginUserData)
        {
            this.marketingAppService = marketingAppService;
            this.loginUserData = loginUserData;
        }

        [HttpPost]
        public async Task<ResponseMessageDto> GetCartMarketingCampaigns()
        {
            var response = await marketingAppService.GetCartMarketingCampaigns();
            var websiteId = await loginUserData.GetCommonWebsiteId();
            var orgName = await loginUserData.GetWebsiteOrgName(websiteId);
            if (response.Object is CartMarketingCampaignsDto campaigns)
            {
                foreach (var item in campaigns.AddOnCampaigns.SelectMany(x => x.RewardItems))
                    item.ImageUrl = ToPublicProductImageUrl(item.ImageUrl, orgName);
                foreach (var item in campaigns.AddOnCampaigns.SelectMany(x => x.ScopeProducts))
                    item.ImageUrl = ToPublicProductImageUrl(item.ImageUrl, orgName);
            }
            return response;
        }

        [HttpGet]
        public async Task<ResponseMessageDto> GetProductAddOnCampaigns(long productId)
        {
            var response = await marketingAppService.GetProductAddOnCampaigns(productId);
            var websiteId = await loginUserData.GetCommonWebsiteId();
            var orgName = await loginUserData.GetWebsiteOrgName(websiteId);

            // FileUploadAppService 回傳後台使用的 /upload/{orgName}/...；
            // 前台的 upload 目錄已依目前網站完成對應，不應再帶 orgName。
            if (response.Object is IEnumerable<ProductAddOnCampaignDto> campaigns)
            {
                foreach (var item in campaigns.SelectMany(x => x.RewardItems))
                {
                    item.ImageUrl = ToPublicProductImageUrl(item.ImageUrl, orgName);
                }
            }

            return response;
        }

        private static string ToPublicProductImageUrl(string imageUrl, string orgName)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return "/images/noImg.jpg";

            const string uploadPrefix = "/upload/";
            const string productFolder = "/Product/";

            if (!imageUrl.StartsWith(uploadPrefix, StringComparison.OrdinalIgnoreCase))
                return imageUrl;

            // FileUploadAppService 為後台路徑補上 /upload/{orgName}/，但前台的
            // /upload 已映射到目前網站目錄。以實際 orgName 精確移除該節點，
            // 才能同時處理 Product、ordImage 與 Excel 匯入的其他舊圖片目錄。
            if (!string.IsNullOrWhiteSpace(orgName))
            {
                var websitePrefix = uploadPrefix + orgName.Trim('/') + "/";
                if (imageUrl.StartsWith(websitePrefix, StringComparison.OrdinalIgnoreCase))
                    return uploadPrefix + imageUrl[websitePrefix.Length..];
            }

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
