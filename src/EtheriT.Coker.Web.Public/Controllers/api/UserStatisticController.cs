using EtheriT.Coker.Application.Shared.Remote;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserStatisticController : Controller
    {
        private readonly IRemoteAppService remoteAppService;
        public UserStatisticController(IRemoteAppService remoteAppService) { 
            this.remoteAppService = remoteAppService;
        }
        [HttpPost]
        public async Task trackTime()
        {
            using (var reader = new StreamReader(HttpContext.Request.Body)) {
                var body = await reader.ReadToEndAsync();  // 讀取原始 JSON 字串

                // 使用 System.Text.Json 或 Newtonsoft.Json 解析資料
                var json = JsonSerializer.Deserialize<Dictionary<string, int>>(body);
                if (json != null && json.ContainsKey("timeSpan")) {
                    int timeSpan = json["timeSpan"];
                    await remoteAppService.UpdateRemoteTime(timeSpan);
                }
            }
        }
    }
}
