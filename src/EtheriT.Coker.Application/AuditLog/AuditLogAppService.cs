using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto.AuditLog;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
			var baseQuery = db.AuditLogs.AsNoTracking().Where(e => e.FK_WebsiteId == siteId);

            IQueryable<AuditLogListDto> dtoQuery = mapper.ProjectTo<AuditLogListDto>(baseQuery);
            loadOptions.PrimaryKey = new[] { "Id" };
            loadOptions.PaginateViaPrimaryKey = true;

            var output = await DataSourceLoader.LoadAsync(dtoQuery, loadOptions);
            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
		}
	}
}
