using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Remote;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Web.MVC.Models.Dacshboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class RemoteController : Controller
	{
        private readonly LoginUserData loginUserData;
        private readonly IRemoteAppService remoteAppService;
        private readonly IConfiguration configuration;
        public RemoteController(LoginUserData loginUserData, IRemoteAppService remoteAppService, IConfiguration configuration) {
            this.loginUserData = loginUserData;
            this.remoteAppService = remoteAppService;
            this.configuration = configuration;
        }
		[HttpGet]
		public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
		{
			return await remoteAppService.GetAllList(loadOptions);
		}
		[HttpGet]
		public async Task<JsonResult> GetPageList(DataSourceLoadOptions loadOptions)
		{
			return await remoteAppService.GetPageList(loadOptions);
		}
        [HttpPost]
        public async Task<IActionResult> GetRemoteCount(GetRemoteCountInputDto dto)
        {
            var startInTaipeiTime = dto.ConvertToTimeZone("Taipei Standard Time", dto.StartDate).Date;//上午00:00
            var endInTaipeiTime = dto.ConvertToTimeZone("Taipei Standard Time", dto.EndDate);
            string orgName = await loginUserData.GetWebsiteOrgName();//獲取後台登入後選擇編輯哪個站點
            long orgId = loginUserData.GetFrontWebsiteId();//獲取站台Id
            string filePath = $"{configuration.GetValue<string>("VirtualDirectory:upload")}\\{orgName}";
            var result = await remoteAppService.GetRemoteCount(dto);
            var remoteItem = new List<long>();
            var remoteMemCount = new List<long>();
            var dateItem = new List<string>();
            var today = DateTime.Today;
            if (result.Success)
            {
                var items = ((GetRemoteCountOutputDto)result.Object).remoteListOtputDtos;
                List<DateTime> dividedDates = new List<DateTime>();
                // 计算日期范围的总天数
                TimeSpan totalDays = endInTaipeiTime - startInTaipeiTime;
                // 每个区间的天数
                double intervalDays = totalDays.TotalDays / 7;
                // 添加起始日期
                dividedDates.Add(startInTaipeiTime);
                if (totalDays.TotalDays >= 7)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        DateTime d = startInTaipeiTime.AddDays(intervalDays * i).Date;
                        RemoteListOtputDto? item = items.Find(e => e.date == d);
                        if (item == null)
                        {
                            remoteItem.Add(0);
                            remoteMemCount.Add(0);
                        }
                        else
                        {
                            remoteItem.Add(item.count);
                            remoteMemCount.Add(item.MemCount);
                        }
                        dateItem.Add(d.ToString("MM/dd"));
                    }
                } else
                {
                    for (int i = 0; i <= totalDays.TotalDays; i++)
                    {
                        DateTime d = startInTaipeiTime.AddDays(i);
                        RemoteListOtputDto? item = items.Find(e => e.date == d);
                        if (item == null)
                        {
                            remoteItem.Add(0);
                            remoteMemCount.Add(0);
                        }
                        else
                        {
                            remoteItem.Add(item.count);
                            remoteMemCount.Add(item.MemCount);
                        }
                        dateItem.Add(d.ToString("MM/dd"));
                    }
                }
            }

            var responseDto = new WebsitesRemote
            {
                WebsitesRemotesCount = remoteItem,
                WebsitesRemotesMemCount = remoteMemCount,
                WebsitesRemotesDate = dateItem
            };
            return Json(responseDto);
        }
    }
}
