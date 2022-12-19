using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Webs.Dto;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor httpContextAccessor;
        public WebsiteApplication(
			CokerDbContext db,
            IHttpContextAccessor httpContextAccessor
        ) {
			this.db = db;
			this.httpContextAccessor = httpContextAccessor;
		}
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
			if (string.IsNullOrEmpty(httpContextAccessor.HttpContext.Request.Cookies["WebSiteId"])) {
				httpContextAccessor.HttpContext.Response.Cookies.Append("WebSiteId", (await date.FirstAsync()).Id.ToString());
            }
            long siteId = 0;
			var output = await date.ToListAsync();
			if(long.TryParse(httpContextAccessor.HttpContext.Request.Cookies["WebSiteId"],out siteId)){
				var item = output.Find(e => e.Id == siteId);
				if(item!=null) item.Check=true;
            }
            return output;

        }
	}
}
