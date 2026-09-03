using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto.AuditLog;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		private const int DefaultHistoryDays = 30;
		private const int MaxHistoryDays = 90;
		private const int TodayHistoryLimit = 20;
		private const int TotalHistoryLimit = 30;

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

		public async Task<CanvasAuditLogOutputDto> GetCanvasHistory(CanvasAuditLogInputDto input)
		{
			var output = new CanvasAuditLogOutputDto();

			try
			{
				if (input.Id <= 0)
				{
					throw new ArgumentException("畫布資料 Id 不正確");
				}

				var source = GetCanvasAuditLogSource(input.Source);
				var today = DateTime.Today;
				var endDate = (input.EndDate ?? today).Date;
				var startDate = (input.StartDate ?? endDate.AddDays(-(DefaultHistoryDays - 1))).Date;

				if (endDate > today)
				{
					endDate = today;
				}

				if (startDate > endDate)
				{
					throw new ArgumentException("開始日期不可晚於結束日期");
				}

				if ((endDate - startDate).TotalDays > MaxHistoryDays)
				{
					throw new ArgumentException($"開始與結束日期最多相差 {MaxHistoryDays} 天");
				}

				var siteId = await loginUserData.GetWebsiteId();
				var endExclusive = endDate.AddDays(1);
				var firstMethod = source.SaveMethod ?? source.PublishMethod;
				var secondMethod = source.PublishMethod ?? source.SaveMethod;
				var publishMethod = source.PublishMethod ?? string.Empty;

				output.StartDate = startDate;
				output.EndDate = endDate;
				output.Items = await db.Database
					.SqlQuery<CanvasAuditLogItemDto>(
						$"""
						WITH [Filtered] AS
						(
						    SELECT
						        [log].[Id],
						        [log].[ExecutionTime],
						        COALESCE([log].[ClientName], N'') AS [ClientName],
						        COALESCE([log].[MethodName], N'') AS [MethodName],
						        CASE
						            WHEN [log].[MethodName] = {publishMethod} THEN N'publish'
						            ELSE N'save'
						        END AS [Operation],
						        ROW_NUMBER() OVER
						        (
						            PARTITION BY CONVERT(date, [log].[ExecutionTime])
						            ORDER BY [log].[ExecutionTime] DESC, [log].[Id] DESC
						        ) AS [DailyRowNumber]
						    FROM [dbo].[AuditLogs] AS [log] WITH (READPAST)
						    WHERE [log].[FK_WebsiteId] = {siteId}
						      AND [log].[ServiceName] = {source.ServiceName}
						      AND [log].[MethodName] IN ({firstMethod}, {secondMethod})
						      AND [log].[ExecutionTime] >= {startDate}
						      AND [log].[ExecutionTime] < {endExclusive}
						      AND TRY_CONVERT
						          (
						              bigint,
						              JSON_VALUE
						              (
						                  CASE WHEN ISJSON([log].[Parameters]) = 1 THEN [log].[Parameters] END,
						                  '$.Id'
						              )
						          ) = {input.Id}
						      AND JSON_VALUE
						          (
						              CASE WHEN ISJSON([log].[ReturnValue]) = 1 THEN [log].[ReturnValue] END,
						              '$.Success'
						          ) = N'true'
						),
						[TodayRows] AS
						(
						    SELECT TOP ({TodayHistoryLimit})
						        [Id], [ExecutionTime], [ClientName], [MethodName], [Operation]
						    FROM [Filtered]
						    WHERE [ExecutionTime] >= {today}
						      AND [ExecutionTime] < {today.AddDays(1)}
						    ORDER BY [ExecutionTime] DESC, [Id] DESC
						),
						[PreviousDailyRows] AS
						(
						    SELECT [Id], [ExecutionTime], [ClientName], [MethodName], [Operation]
						    FROM [Filtered]
						    WHERE [ExecutionTime] < {today}
						      AND [DailyRowNumber] = 1
						),
						[Combined] AS
						(
						    SELECT * FROM [TodayRows]
						    UNION ALL
						    SELECT * FROM [PreviousDailyRows]
						)
						SELECT TOP ({TotalHistoryLimit})
						    [Id], [ExecutionTime], [ClientName], [MethodName], [Operation],
						    CAST(0 AS bit) AS [IsCurrent]
						FROM [Combined]
						ORDER BY [ExecutionTime] DESC, [Id] DESC
						""")
					.ToListAsync();

				if (endDate == today && output.Items.Count > 0)
				{
					output.Items[0].IsCurrent = true;
				}

				output.Success = true;
			}
			catch (Exception ex)
			{
				output.Error = ex.Message;
			}

			return output;
		}

		public async Task<CanvasAuditLogDetailOutputDto> GetCanvasHistoryDetail(CanvasAuditLogDetailInputDto input)
		{
			var output = new CanvasAuditLogDetailOutputDto();

			try
			{
				if (input.AuditLogId <= 0 || input.Id <= 0)
				{
					throw new ArgumentException("畫布歷程資料不正確");
				}

				var source = GetCanvasAuditLogSource(input.Source);
				var siteId = await loginUserData.GetWebsiteId();
				var allowedMethods = new[] { source.SaveMethod, source.PublishMethod }
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.Distinct()
					.ToList();

				var auditLog = await db.AuditLogs
					.AsNoTracking()
					.Where(x => x.Id == input.AuditLogId)
					.Where(x => x.FK_WebsiteId == siteId)
					.Where(x => x.ServiceName == source.ServiceName)
					.Where(x => allowedMethods.Contains(x.MethodName!))
					.Select(x => new
					{
						x.Parameters,
						x.ReturnValue,
						x.ExecutionTime,
						x.ClientName,
						x.MethodName
					})
					.FirstOrDefaultAsync();

				if (auditLog == null)
				{
					throw new InvalidOperationException("找不到畫布歷程紀錄");
				}

				var parameters = ParseJsonObject(auditLog.Parameters, "畫布歷程內容格式不正確");
				var loggedId = GetJsonValue(parameters, "Id")?.Value<long?>();

				if (loggedId != input.Id)
				{
					throw new InvalidOperationException("畫布歷程紀錄與目前資料不符");
				}

				var returnValue = ParseJsonObject(auditLog.ReturnValue, "畫布歷程執行結果格式不正確");
				if (GetJsonValue(returnValue, "Success")?.Value<bool?>() != true)
				{
					throw new InvalidOperationException("失敗的操作紀錄不可還原");
				}

				var htmlToken = GetJsonValue(parameters, "SaveHtml") ?? GetJsonValue(parameters, "Html");
				var cssToken = GetJsonValue(parameters, "SaveCss") ?? GetJsonValue(parameters, "Css");

				if (htmlToken == null)
				{
					throw new InvalidOperationException("此紀錄沒有可還原的畫布內容");
				}

				output.Html = htmlToken.Type == JTokenType.Null ? string.Empty : htmlToken.Value<string>() ?? string.Empty;
				output.Css = cssToken == null || cssToken.Type == JTokenType.Null
					? string.Empty
					: cssToken.Value<string>() ?? string.Empty;
				output.ExecutionTime = auditLog.ExecutionTime;
				output.ClientName = auditLog.ClientName ?? string.Empty;
				output.Operation = source.PublishMethod != null && auditLog.MethodName == source.PublishMethod
					? "publish"
					: "save";
				output.Success = true;
			}
			catch (Exception ex)
			{
				output.Error = ex.Message;
			}

			return output;
		}

		private static JObject ParseJsonObject(string? json, string errorMessage)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				throw new InvalidOperationException(errorMessage);
			}

			try
			{
				return JObject.Parse(json);
			}
			catch (JsonException ex)
			{
				throw new InvalidOperationException(errorMessage, ex);
			}
		}

		private static JToken? GetJsonValue(JObject json, string propertyName)
		{
			return json.GetValue(propertyName, StringComparison.OrdinalIgnoreCase);
		}

		private static CanvasAuditLogSourceSetting GetCanvasAuditLogSource(CanvasAuditLogSource source)
		{
			return source switch
			{
				CanvasAuditLogSource.Advertise => new("Advertise", "SaveConten", "ImportConten"),
				CanvasAuditLogSource.Article => new("Article", "SaveConten", "ImportConten"),
				CanvasAuditLogSource.ObjectType => new("ObjectType", null, "SaveConten"),
				CanvasAuditLogSource.WebMenu => new("WebMenu", "saveConten", "importConten"),
				CanvasAuditLogSource.Product => new("Product", "SaveConten", "ImportConten"),
				CanvasAuditLogSource.TechnicalCertificate => new("TechnicalCertificate", null, "SaveConten"),
				CanvasAuditLogSource.TemplateFooter => new("Template", "saveDefaultFooter", "importDefaultFooter"),
				_ => throw new ArgumentException("不支援的畫布歷程來源")
			};
		}

		private sealed record CanvasAuditLogSourceSetting(
			string ServiceName,
			string? SaveMethod,
			string? PublishMethod);
	}
}
