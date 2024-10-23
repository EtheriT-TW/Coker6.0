using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.AuditLog;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Newsletter;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Tag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.Newsletter;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class NewsletterController : Controller
    {
        private readonly INewsletterAppService newsletterAppService;
        private readonly IArticleAppService articleAppService;
        private readonly ITagAppService tagAppService;
        public NewsletterController(INewsletterAppService newsletterAppService, IArticleAppService articleAppService, ITagAppService tagAppService)
        {
            this.newsletterAppService = newsletterAppService;
            this.articleAppService = articleAppService;
            this.tagAppService = tagAppService;
        }
        [HttpGet]
        public async Task<JsonResult> GetNewsletters(DataSourceLoadOptions loadOptions)
        {
            return await articleAppService.GetNewsletterList(loadOptions);
        }
        [HttpGet]
        public async Task<JsonResult> GetRecipients(DataSourceLoadOptions loadOptions)
        {
            return await newsletterAppService.GetRecipients(loadOptions);
        }
        [HttpGet]
        public async Task<TagGetAllListDto?> GetRecipientsTag() {
            return await tagAppService.GetTagByName("電子報");
        }
        [HttpGet]
        public JsonResult GetTargetLookup(DataSourceLoadOptions loadOptions)
        {
            List<TargetDto> dataQuery = new List<TargetDto> {
                new TargetDto { Name="另開視窗", newWindow=true},
                new TargetDto { Name="直接連結", newWindow=false}
            };
            var output = DataSourceLoader.Load(dataQuery, loadOptions);
            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ResponseMessageDto> SaveRecipients([FromForm] DevExpressDto dto)
        {
            return await newsletterAppService.RecipientAddUp(dto);
        }
        [HttpDelete]
        public async Task<ResponseMessageDto> DeleteRecipients(DataDelectDto dto)
        {
            return await newsletterAppService.DeleteRecipients(dto.Id);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> Send(DataDelectDto dto) {
            return await newsletterAppService.Send(dto.Id);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> UpdateJson(NewsletterFrameDto dto) {
            return await newsletterAppService.UpdateJson(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveConten(HtmlContentDetailDto dto) {
            return await newsletterAppService.SaveConten(dto);
        }
    }
}
