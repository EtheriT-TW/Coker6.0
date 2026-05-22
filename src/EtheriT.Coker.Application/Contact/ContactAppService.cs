using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Contact.Export;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Contact;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Contact;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
// MiniExcelLibs 也定義 IConfiguration，這裡用別名固定指向 ASP.NET Core 設定物件。
using AppConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace EtheriT.Coker.Application.Contact
{
    public class ContactAppService : IContactAppService
    {
        // 匯出筆數上限改由設定檔提供，避免後端限制與頁面提示各自寫死。
        private const string ContactExportMaxRowsConfigKey = "ContactExport:MaxRows";
        // 設定值異常時至少保留 1 筆上限，避免錯誤設定導致無限制匯出。
        private const int MinimumExportRows = 1;

        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ICaptchaAppService captchaAppService;
        private readonly MailAppService mailAppService;
        private readonly AppConfiguration configuration;
        private readonly IMapper mapper;
        private readonly ILogger<ContactAppService> logger;
        private readonly ExportTemplateResolver exportTemplateResolver;
        public ContactAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ICaptchaAppService captchaAppService,
            MailAppService mailAppService,
            AppConfiguration configuration,
            IMapper mapper,
            ILogger<ContactAppService> logger,
            ExportTemplateResolver exportTemplateResolver
        )
        {
            this.db = db;
            this.mapper = mapper;
            this.captchaAppService = captchaAppService;
            this.mailAppService = mailAppService;
            this.configuration = configuration;
            this.loginUserData = loginUserData;
            this.logger = logger;
            this.exportTemplateResolver = exportTemplateResolver;

        }
        public async Task<ResponseMessageDto> submit(FormSubmitDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            long siteId = configuration.GetValue<long>("WebConfig:SiteId");
            try
            {
                var code = dto.forms.Find(e => e.Name == "captcha");
                var StoreSetId = (await db.StoreSet.Where(e => e.key == "EmailNotificationType").FirstOrDefaultAsync())?.Id;
                var StoreSet = StoreSetId != null ? await db.StoreSetDetail.Where(e => e.FK_WebsiteId == siteId && e.FK_StoreSetId == StoreSetId).FirstOrDefaultAsync() : null;
                var EmailNotificationType = int.Parse(StoreSet?.value ?? "0");

                var codeId = dto.forms.Find(e => e.Name == "captchaId");
                if (codeId == null || code == null || !captchaAppService.Validate(codeId.Value, code.Value).Success) throw new Exception(L.get("VerificationCodeError"));
                else
                {
                    var site = await db.Websites.Where(e => !e.IsDeleted).Where(e => e.Id == siteId).FirstOrDefaultAsync();
                    if (site == null) throw new Exception(L.get("WebsiteDataError"));
                    var menu = await db.WebMenus.Where(e => !e.IsDeleted && e.FK_WebsiteId == siteId && e.RouterName == dto.RouterName).FirstOrDefaultAsync();
                    if (menu == null) throw new Exception(L.get("UnknownSource"));
                    MailUserDataDto recipient = new MailUserDataDto();

                    string html = "";
                    switch ((EmailNotificationTypeEnum)EmailNotificationType)
                    {
                        case EmailNotificationTypeEnum.寄送完整表單:
                            html = "<p>您好，感謝您的聯繫。此信件為系統通知，請勿直接回覆，感謝您的理解與配合~謝謝<br></p><table class='table'>";
                            dto.forms.ForEach(e =>
                            {
                                if (!string.IsNullOrEmpty(e.Title))
                                {
                                    html += $@"<tr>
										<td class='title'>{e.Title}</td>
										<td>{e.Value.Replace(Environment.NewLine, "<br/>")}</td>
									</tr>";
                                }
                                if (e.Name == "email") recipient.Email = e.Value;
                                else if (e.Name == "name") recipient.Name = e.Value;
                            });
                            html += "</table>";
                            break;
                        case EmailNotificationTypeEnum.簡易通知:
                            dto.forms.ForEach(e =>
                            {
                                if (!string.IsNullOrEmpty(e.Title))
                                {
                                    html += $@"<tr>
										<td class='title'>{e.Title}</td>
										<td>{e.Value.Replace(Environment.NewLine, "<br/>")}</td>
									</tr>";
                                }
                                if (e.Name == "email") recipient.Email = e.Value;
                                else if (e.Name == "name") recipient.Name = e.Value;
                            });

                            var name = dto.forms.FirstOrDefault(f => f.Name == "name")?.Value ?? "";
                            var gender = dto.forms.FirstOrDefault(f => f.Name == "Gender")?.Value ?? "先生/小姐";
                            string title;

                            if (string.IsNullOrWhiteSpace(name)) title = "親愛的會員";
                            else if (name.Length == 1) title = $"{name} {gender}";
                            else if (name.Length == 2) title = $"{name[0]}○ {gender}";
                            else title = $"{name[0]}○{name[^1]} {gender}";

                            html = $@"<div>{title} 您好</div>
                                                    <p>感謝您透過【{site.Title}】提交 {menu.Title} 表單，我們已經成功收到您的資料。<br>
                                                    我們將盡快進行後續處理，並於需要時與您聯繫。<br><br>
                                                    謝謝您的耐心與支持。</p>";
                            break;
                    }
                    SenderDto senderDto = new SenderDto
                    {
                        Recipients = new List<MailUserDataDto> { recipient },
                        CC = new List<MailUserDataDto> { dto.Sender },
                        Subject = $"{site.Title}-{L.get("ServiceCenter")}",
                        Body = html,
                        Css = ".table{width:800px;} .table td{border-bottom: #ececec solid 1px; padding: 6px 3px;} .table td:last-child{padding-left: 9px;} .title{background-color: #ececec; width:22%; text-align: center; font-weight: bold;}"
                    };
                    senderDto.Sender.Name = string.IsNullOrEmpty(site.Contact) ? site.Title ?? "" : site.Contact;
                    await mailAppService.sendMail(senderDto);

                    // 將前台表單欄位轉成 title/value JSON，匯出時會從 FromDate 還原動態欄位。
                    var dict = dto.forms
                        .Where(f => !string.IsNullOrWhiteSpace(f.Title))
                        .ToDictionary(
                            f => f.Name,
                            f => new
                            {
                                title = string.IsNullOrWhiteSpace(f.Title) ? "" : f.Title.Trim().Replace(" ", "").Replace("　", ""),
                                value = f.Value
                            }
                        );
                    string result = JsonConvert.SerializeObject(dict);
                    var parts = new[]
                    {
                        dto.forms.Find(e => e.Name == "name")?.Value,
                        dto.forms.Find(e => e.Name == "Gender")?.Value
                    };
                    Core.Models.Contact contact = new Core.Models.Contact
                    {
                        FK_WebMenuId = menu.Id,
                        Email = recipient.Email,
                        Name = menu.Title ?? "",
                        TargetEmail = $"{dto.Sender.Name}({dto.Sender.Email})",
                        Html = html,
                        FromDate = result,
                        UserName = string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)))
                    };
                    loginUserData.setOptionParameter(contact, 0);
                    db.Contacts.Add(contact);
                    await db.SaveChangesAsync();
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<JsonResult> GetContactListAll(DataSourceLoadOptions loadOptions)
        {
            long WebsiteID = await loginUserData.GetWebsiteId();
            var dataQuery = from c in db.Contacts.Where(e => !e.IsDeleted)
                            join m in db.WebMenus.Where(e => e.FK_WebsiteId == WebsiteID && !e.IsDeleted) on c.FK_WebMenuId equals m.Id
                            select new ContactListDto
                            {
                                Id = c.Id,
                                Name = c.Name,
                                UserName = c.UserName,
                                TargetEmail = c.TargetEmail,
                                Email = c.Email,
                                Status = c.Status.ToString().Replace("_", "/"),
                                CreationTime = c.CreationTime
                            };
            if (dataQuery.Any())
            {
                var output = DataSourceLoader.Load(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            else
            {
                return new JsonResult(new List<ContactListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
        }

        /// <summary>
        /// 取得目前站台中已有聯絡資料的表單類別，供匯出下拉選單使用。
        /// </summary>
        public async Task<ResponseMessageDto> GetContactExportFormTypesAsync()
        {
            var response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var exportMaxRows = GetExportMaxRows();

                // 只列出目前站台、未刪除且已產生聯絡資料的 WebMenu，避免匯出空表單。
                var rawOptions = await (
                    from menu in db.WebMenus.Where(e => e.FK_WebsiteId == websiteId && !e.IsDeleted)
                    join contact in db.Contacts.Where(e => !e.IsDeleted) on menu.Id equals contact.FK_WebMenuId
                    group menu by new { menu.Id, menu.Title } into g
                    orderby g.Key.Id
                    select new
                    {
                        g.Key.Id,
                        Title = (g.Key.Title ?? string.Empty).Trim()
                    }
                ).ToListAsync();

                // 若不同表單同名，補上 Id 方便後台人員辨識。
                var titleCounts = rawOptions
                    .GroupBy(e => string.IsNullOrWhiteSpace(e.Title) ? "未命名表單" : e.Title)
                    .ToDictionary(e => e.Key, e => e.Count());

                var formTypes = rawOptions.Select(e =>
                {
                    var title = string.IsNullOrWhiteSpace(e.Title) ? "未命名表單" : e.Title;
                    return new SelectDto
                    {
                        Id = e.Id,
                        Name = titleCounts[title] > 1 ? $"{title} ({e.Id})" : title
                    };
                }).ToList();

                // 同一支 API 一併回傳目前匯出上限，讓頁面提示文字與後端限制保持一致。
                response.Object = new
                {
                    formTypes,
                    maxRows = exportMaxRows
                };
                response.Success = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load contact export form types.");
                response.Error = "表單類別載入失敗，請稍後再試。";
            }

            return response;
        }

        /// <summary>
        /// 依查詢條件匯出聯絡表單資料，包含固定欄位與 FromDate 動態欄位。
        /// </summary>
        public async Task<ContactExportResultDto> ExportContactsAsync(ContactExportRequestDto dto)
        {
            var stopwatch = Stopwatch.StartNew();
            var exportMaxRows = GetExportMaxRows();
            var response = new ContactExportResultDto { MaxRows = exportMaxRows };
            object? auditResult = null;

            try
            {
                // 先做登入、站台、時間與狀態基本驗證，失敗時仍會寫入稽核記錄。
                var validation = await ValidateExportRequestAsync(dto, exportMaxRows);
                if (validation != null)
                {
                    auditResult = new
                    {
                        validation.HttpStatusCode,
                        validation.ErrorCodeKey,
                        validation.Error,
                        validation.MaxRows
                    };
                    return validation;
                }

                var websiteId = await loginUserData.GetWebsiteId();

                // 匯出表單必須存在且未刪除；站台歸屬稍後獨立檢查以回傳 403。
                var menu = await db.WebMenus.IgnoreQueryFilters()
                    .Where(e => e.Id == dto.FormTypeId && !e.IsDeleted)
                    .Select(e => new
                    {
                        e.Id,
                        e.FK_WebsiteId,
                        Title = (e.Title ?? string.Empty).Trim()
                    })
                    .FirstOrDefaultAsync();

                if (menu == null)
                {
                    response = CreateExportFailure(HttpStatusCode.BadRequest, "E004", "請選擇有效的表單類別", ErrorCodeEnum.ValidationError, exportMaxRows);
                    auditResult = new { response.HttpStatusCode, response.ErrorCodeKey, response.Error };
                    return response;
                }

                // 防止跨站台以 FormTypeId 猜測匯出其他網站資料。
                if (menu.FK_WebsiteId != websiteId)
                {
                    response = CreateExportFailure(HttpStatusCode.Forbidden, "E005", "無權限存取此表單資料", ErrorCodeEnum.Forbidden, exportMaxRows);
                    auditResult = new { response.HttpStatusCode, response.ErrorCodeKey, response.Error };
                    return response;
                }

                var statuses = dto.Statuses?.Distinct().ToList() ?? new List<int>();

                // 查詢以表單類別、送出時間與可選狀態為條件；資料量上限在後續 Take 控制。
                var query = db.Contacts
                    .AsNoTracking()
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.FK_WebMenuId == dto.FormTypeId)
                    .Where(e => e.CreationTime >= dto.StartTime && e.CreationTime <= dto.EndTime);

                if (statuses.Any())
                {
                    query = query.Where(e => statuses.Contains((int)e.Status));
                }

                var contacts = await query
                    .OrderByDescending(e => e.CreationTime)
                    .ThenByDescending(e => e.Id)
                    .Select(e => new ContactExportSource
                    {
                        Id = e.Id,
                        UserName = e.UserName,
                        Email = e.Email,
                        TargetEmail = e.TargetEmail,
                        Status = e.Status,
                        CreationTime = e.CreationTime,
                        FromDate = e.FromDate
                    })
                    // 多取 1 筆作為超限哨兵，不需要把大量資料全數載入記憶體。
                    .Take(exportMaxRows + 1)
                    .ToListAsync();

                if (!contacts.Any())
                {
                    response = CreateExportFailure(HttpStatusCode.BadRequest, "E002", "查無資料：此條件區間內沒有資料，請重新設定條件。", ErrorCodeEnum.BadRequest, exportMaxRows);
                    auditResult = new { response.HttpStatusCode, response.ErrorCodeKey, response.Error };
                    return response;
                }

                if (contacts.Count > exportMaxRows)
                {
                    response = CreateExportFailure(HttpStatusCode.BadRequest, "E001", $"匯出失敗：單次最多匯出 {exportMaxRows} 筆資料，請縮小時間範圍後再試。", ErrorCodeEnum.BadRequest, exportMaxRows);
                    auditResult = new { response.HttpStatusCode, response.ErrorCodeKey, response.Error, Count = contacts.Count, Limit = exportMaxRows };
                    return response;
                }

                // FromDate 解析出動態欄位；若該表單存在 JSON 範本，則以範本排序與顯示設定為準。
                var parser = new FromDateParser(logger);
                var parsed = parser.Parse(contacts.Select(e => (e.Id, e.FromDate)));
                var template = await exportTemplateResolver.GetTemplateAsync(dto.FormTypeId);
                var dynamicColumns = template.HasTemplate
                    ? template.Columns
                        .Where(e => e.Visible && !string.Equals(FromDateParser.NormalizeKey(e.ColumnKey), "captcha", StringComparison.OrdinalIgnoreCase))
                        // 範本順序由 JSON columns 陣列決定；維護人員調整陣列位置即可改變輸出欄位順序。
                        .Select(e => new FromDateColumn
                        {
                            NormalizedKey = FromDateParser.NormalizeKey(e.ColumnKey),
                            OriginalKey = e.ColumnKey,
                            Title = string.IsNullOrWhiteSpace(e.ColumnTitle) ? e.ColumnKey : e.ColumnTitle
                        })
                        .ToList()
                    : parsed.Columns;

                if (template.HasTemplate)
                {
                    LogTemplateColumnsWithoutMapping(dto.FormTypeId, dynamicColumns, parsed);
                }

                var rows = BuildExportRows(contacts, menu.Title, dynamicColumns, parsed);
                var stream = new MemoryStream();

                // MiniExcel 使用 Dictionary key 作為表頭，rows 內欄位順序即 Excel 欄位順序。
                MiniExcel.SaveAs(stream, rows, sheetName: ToExcelSheetName(menu.Title));

                response.Success = true;
                response.MaxRows = exportMaxRows;
                response.ExportedCount = contacts.Count;
                response.FileContents = stream.ToArray();
                response.FileName = $"form_export_{dto.FormTypeId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                response.HttpStatusCode = (int)HttpStatusCode.OK;

                auditResult = new
                {
                    response.HttpStatusCode,
                    response.ExportedCount,
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Contact export failed.");
                var isTemplateError = ex is InvalidOperationException
                    && ex.Message.StartsWith("聯絡表單匯出範本", StringComparison.Ordinal);
                var statusCode = isTemplateError ? HttpStatusCode.BadRequest : HttpStatusCode.InternalServerError;
                var errorCodeKey = isTemplateError ? "E004" : "E003";
                var errorCode = isTemplateError ? ErrorCodeEnum.ValidationError : ErrorCodeEnum.ServerError;
                var message = isTemplateError
                    ? $"匯出範本設定錯誤：{ex.Message}"
                    : "匯出失敗：系統發生意外錯誤，請稍後再試或聯繫系統管理員。";

                response = CreateExportFailure(statusCode, errorCodeKey, message, errorCode, exportMaxRows);
                auditResult = new
                {
                    response.HttpStatusCode,
                    response.ErrorCodeKey,
                    response.Error,
                    ExceptionType = ex.GetType().Name,
                    ExceptionMessage = ex.Message
                };
                return response;
            }
            finally
            {
                stopwatch.Stop();

                // 不論成功或失敗都要留下匯出條件與結果，方便日後稽核。
                await WriteExportAuditLogAsync(dto, auditResult ?? new { response.HttpStatusCode, response.ErrorCodeKey, response.Error });
            }
        }

        public async Task<ResponseMessageDto> GetDataOne(long id)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var dataQuery = await db.Contacts.Include(e => e.WebMenu).Where(e => e.WebMenu != null && e.WebMenu.FK_WebsiteId == websiteId && e.Id == id).FirstOrDefaultAsync();
                response.Object = mapper.Map<AsrFormDataDto>(dataQuery);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> ReplyContact(ContactReplyDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var contact = await db.Contacts.Include(e => e.WebMenu).Where(e => e.WebMenu != null && e.WebMenu.FK_WebsiteId == websiteId && e.Id == dto.Id).FirstOrDefaultAsync();
                if (contact != null)
                {
                    contact.Status = dto.Status;
                    if (!string.IsNullOrWhiteSpace(dto.Reply))
                    {
                        contact.Reply = dto.Reply;
                        contact.ReplyTime = DateTime.Now;
                    }
                    await loginUserData.SaveChanges(contact);
                    response.Success = true;
                }
                else throw new Exception(L.get("DataNotFound"));
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));

            return response;
        }

        /// <summary>
        /// 驗證匯出條件與目前登入狀態；錯誤碼對應規格定義。
        /// </summary>
        private async Task<ContactExportResultDto?> ValidateExportRequestAsync(ContactExportRequestDto dto, int exportMaxRows)
        {
            if (!loginUserData.IsLoggedIn())
            {
                return CreateExportFailure(HttpStatusCode.Unauthorized, "E006", "請重新登入。", ErrorCodeEnum.Unauthorized, exportMaxRows);
            }

            if (await loginUserData.GetWebsiteId() == 0)
            {
                return CreateExportFailure(HttpStatusCode.Unauthorized, "E006", "請重新登入。", ErrorCodeEnum.Unauthorized, exportMaxRows);
            }

            if (dto.FormTypeId <= 0)
            {
                return CreateExportFailure(HttpStatusCode.BadRequest, "E004", "請選擇有效的表單類別", ErrorCodeEnum.ValidationError, exportMaxRows);
            }

            if (dto.StartTime == default || dto.EndTime == default)
            {
                return CreateExportFailure(HttpStatusCode.BadRequest, "E004", "請選擇時間區間", ErrorCodeEnum.ValidationError, exportMaxRows);
            }

            if (dto.EndTime < dto.StartTime)
            {
                return CreateExportFailure(HttpStatusCode.BadRequest, "E004", "結束時間不可早於開始時間", ErrorCodeEnum.ValidationError, exportMaxRows);
            }

            var statuses = dto.Statuses?.Distinct().ToList() ?? new List<int>();
            if (statuses.Any(e => e != (int)ContactStatusEnum.未處理 && e != (int)ContactStatusEnum.已完成))
            {
                return CreateExportFailure(HttpStatusCode.BadRequest, "E004", "狀態值不合法", ErrorCodeEnum.ValidationError, exportMaxRows);
            }

            return null;
        }

        /// <summary>
        /// 從設定檔取得聯絡表單匯出上限；Take 會多取 1 筆，所以保留整數上限空間。
        /// </summary>
        private int GetExportMaxRows()
        {
            var configuredMaxRows = configuration.GetValue<int>(ContactExportMaxRowsConfigKey);
            if (configuredMaxRows < MinimumExportRows)
            {
                return MinimumExportRows;
            }

            return configuredMaxRows == int.MaxValue ? int.MaxValue - 1 : configuredMaxRows;
        }

        /// <summary>
        /// 建立匯出失敗回應，讓 Service 與 Controller 能使用一致的錯誤格式。
        /// </summary>
        private static ContactExportResultDto CreateExportFailure(HttpStatusCode statusCode, string errorCodeKey, string message, ErrorCodeEnum errorCode, int maxRows)
        {
            return new ContactExportResultDto
            {
                Success = false,
                Error = message,
                Message = message,
                HttpStatusCode = (int)statusCode,
                ErrorCodeKey = errorCodeKey,
                ErrorCode = errorCode,
                MaxRows = maxRows
            };
        }

        /// <summary>
        /// 組出 Excel rows；前段為固定欄位，後段依 FromDate 或模板欄位補上動態欄位。
        /// </summary>
        private static List<Dictionary<string, object?>> BuildExportRows(
            List<ContactExportSource> contacts,
            string formTitle,
            List<FromDateColumn> dynamicColumns,
            FromDateParseResult parsed)
        {
            var rows = new List<Dictionary<string, object?>>();

            foreach (var contact in contacts)
            {
                var row = new Dictionary<string, object?>
                {
                    ["編號"] = contact.Id,
                    ["表單類別"] = formTitle,
                    ["送出時間"] = contact.CreationTime.ToString("yyyy/MM/dd HH:mm"),
                    ["用戶姓名"] = contact.UserName ?? string.Empty,
                    ["用戶信箱"] = contact.Email ?? string.Empty,
                    ["處理信箱"] = contact.TargetEmail ?? string.Empty,
                    ["處理狀態"] = contact.Status.ToString().Replace("_", "/")
                };

                parsed.Records.TryGetValue(contact.Id, out var parsedRecord);

                // 動態欄位找不到值時輸出空白，讓所有資料列維持同一組欄位。
                foreach (var column in dynamicColumns)
                {
                    row[GetUniqueHeader(row, column.Title)] =
                        parsedRecord != null && parsedRecord.Values.TryGetValue(column.NormalizedKey, out var value)
                            ? value
                            : string.Empty;
                }

                rows.Add(row);
            }

            return rows;
        }

        /// <summary>
        /// 避免動態欄位標題與固定欄位或其他動態欄位撞名。
        /// </summary>
        private static string GetUniqueHeader(Dictionary<string, object?> row, string title)
        {
            var header = string.IsNullOrWhiteSpace(title) ? "未命名欄位" : title;
            if (!row.ContainsKey(header))
            {
                return header;
            }

            var index = 2;
            while (row.ContainsKey($"{header} ({index})"))
            {
                index++;
            }

            return $"{header} ({index})";
        }

        /// <summary>
        /// Excel 工作表名稱不能包含特殊字元且長度最多 31 字元。
        /// </summary>
        private static string ToExcelSheetName(string title)
        {
            var sheetName = string.IsNullOrWhiteSpace(title) ? "匯出資料" : title.Trim();
            foreach (var invalidChar in new[] { '\\', '/', '?', '*', '[', ']', ':' })
            {
                sheetName = sheetName.Replace(invalidChar, '_');
            }

            return sheetName.Length > 31 ? sheetName.Substring(0, 31) : sheetName;
        }

        /// <summary>
        /// 範本欄位若本次資料完全找不到對應 key，仍輸出空白欄並記錄警告，方便後續修正範本。
        /// </summary>
        private void LogTemplateColumnsWithoutMapping(long formTypeId, List<FromDateColumn> templateColumns, FromDateParseResult parsed)
        {
            var parsedKeys = parsed.Columns
                .Select(e => e.NormalizedKey)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var column in templateColumns.Where(e => !parsedKeys.Contains(e.NormalizedKey)))
            {
                logger.LogWarning(
                    "Contact export template column did not match any FromDate key. FormTypeId: {FormTypeId}, ColumnKey: {ColumnKey}, ColumnTitle: {ColumnTitle}",
                    formTypeId,
                    column.OriginalKey,
                    column.Title);
            }
        }

        /// <summary>
        /// 寫入匯出稽核紀錄；失敗時只記錄警告，不影響主要匯出流程。
        /// </summary>
        private async Task WriteExportAuditLogAsync(ContactExportRequestDto dto, object result)
        {
            try
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(new
                {
                    dto.FormTypeId,
                    dto.StartTime,
                    dto.EndTime,
                    dto.Statuses
                }), JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to write contact export audit log.");
            }
        }

        /// <summary>
        /// 匯出查詢用的精簡投影，避免載入 Contact 完整實體。
        /// </summary>
        private class ContactExportSource
        {
            public long Id { get; set; }
            public string? UserName { get; set; }
            public string? Email { get; set; }
            public string? TargetEmail { get; set; }
            public ContactStatusEnum Status { get; set; }
            public DateTime CreationTime { get; set; }
            public string? FromDate { get; set; }
        }
    }
}
