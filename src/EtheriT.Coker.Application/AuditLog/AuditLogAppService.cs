using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto.AuditLog;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.AuditLog
{
	public class AuditLogAppService: IAuditLogAppService
	{
		private readonly CokerDbContext db;
		private readonly LoginUserData loginUserData;
		private readonly IMapper mapper;
		public AuditLogAppService(CokerDbContext db, LoginUserData loginUserData, IMapper mapper) { 
			this.db = db;
			this.loginUserData = loginUserData;
			this.mapper = mapper;
		}

		public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
		{
			long siteId = await loginUserData.GetWebsiteId();
			var data = await db.AuditLogs.Where(e => e.FK_WebsiteId == siteId).ToListAsync();
			if (data != null)
			{
				var dataQuery = mapper.Map<List<AuditLogListDto>>(data);
				var output = DataSourceLoader.Load(dataQuery, loadOptions);
				return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			}
			else throw new Exception("查無資料");
		}
	}
}
