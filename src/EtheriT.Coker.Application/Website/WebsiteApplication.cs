using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Webs.Dto;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
	public class WebsiteApplication : IWebsiteApplication
	{
		private readonly CokerDbContext db;
		private readonly ILoginUserDataApplication loginUserDataApplication;
        private readonly IHttpContextAccessor httpContextAccessor;
        public WebsiteApplication(
			CokerDbContext db,
            ILoginUserDataApplication loginUserDataApplication,
            IHttpContextAccessor httpContextAccessor
        ) {
			this.db = db;
			this.httpContextAccessor = httpContextAccessor;
			this.loginUserDataApplication = loginUserDataApplication;
		}

        [Authorize]
        public async Task<List<WebsDto>> GetAll()
		{
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            var date = from w in db.Websites
					   join bind in db.MappingUserAndWebsites on w.Id equals bind.WebsiteId
					   join u in db.Users on bind.UserId equals u.Id
					   where u.Account == name
                       select new WebsDto { 
							Id = w.Id,
							Name = w.Title,
							Description = w.Description??"",
							Images = w.Icon??""
					   };
            if (date.Any())
            {
                long siteId = await loginUserDataApplication.GetWebsiteId();
                var output = await date.ToListAsync();
                if (siteId == 0 && output.Any()) {
                    siteId = output.FirstOrDefault().Id;
                    await Exchange(new WebExchangeDto { Id = siteId });
                }
                var item = output.Find(e => e.Id == siteId);
                if (item != null) item.Check = true;
                return output;
            }
            else return new List<WebsDto>();
        }
		public async Task<ResponseMessageDto> Exchange(WebExchangeDto dto) {
            ResponseMessageDto responseMessageDto= new ResponseMessageDto();
			try
			{
				if ((await loginUserDataApplication.CheckedWebSiteId(dto.Id)))
				{
                    ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
                    string name = user.Identity?.Name;
                    Guid secret = loginUserDataApplication.GetSecret();
                    var token = await db.Tokens.Where(t => t.id == secret).FirstOrDefaultAsync();
					if (token!=null)
					{
                        token.websiteId= dto.Id;
						db.SaveChanges();
                    }
                    else throw new Exception("金鑰失效");
                }
				else throw new Exception("網站不存在");
			} catch (Exception e) {
                responseMessageDto.Success = false;
                responseMessageDto.Error = e.Message;
            }
			return responseMessageDto;
        }

    }
}
