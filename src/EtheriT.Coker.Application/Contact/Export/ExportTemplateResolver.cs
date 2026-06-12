using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Contact.Export
{
    /// <summary>
    /// 匯出模板欄位中繼資料；由 JSON 範本定義欄位 key、顯示名稱與是否輸出。
    /// </summary>
    public class ExportColumnMetadata
    {
        /// <summary>
        /// 對應 FromDate 欄位 key。
        /// </summary>
        public string ColumnKey { get; set; } = string.Empty;

        /// <summary>
        /// Excel 顯示欄名。
        /// </summary>
        public string ColumnTitle { get; set; } = string.Empty;

        /// <summary>
        /// 是否輸出此欄位。
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 保留給未來格式化使用，例如日期或數字格式。
        /// </summary>
        public string? Format { get; set; }
    }

    /// <summary>
    /// 範本解析結果；HasTemplate 用來區分「沒有範本」與「範本存在但沒有可輸出欄位」。
    /// </summary>
    public class ExportTemplateResolveResult
    {
        /// <summary>
        /// 是否找到指定表單類別的 JSON 範本。
        /// </summary>
        public bool HasTemplate { get; set; }

        /// <summary>
        /// 範本內定義的欄位清單。
        /// </summary>
        public IReadOnlyList<ExportColumnMetadata> Columns { get; set; } = new List<ExportColumnMetadata>();
    }

    /// <summary>
    /// 聯絡表單匯出模板解析器；每個表單類別以一個 JSON 檔定義欄位與順序。
    /// </summary>
    public class ExportTemplateResolver
    {
        // 範本資料夾由 Web.MVC appsettings 控制，方便部署時調整位置。
        private const string TemplateDirectoryConfigKey = "ContactExport:TemplateDirectory";
        // 未設定時使用專案內 Resources，讓本機開發與發佈後路徑一致。
        private const string DefaultTemplateDirectory = "Resources/ContactExportTemplates";

        private readonly IConfiguration configuration;
        private readonly IHostEnvironment environment;
        private readonly ILogger<ExportTemplateResolver> logger;

        public ExportTemplateResolver(
            IConfiguration configuration,
            IHostEnvironment environment,
            ILogger<ExportTemplateResolver> logger)
        {
            this.configuration = configuration;
            this.environment = environment;
            this.logger = logger;
        }

        /// <summary>
        /// 讀取指定表單類別的 JSON 範本；找不到範本時標示 HasTemplate=false，讓匯出流程改走動態解析。
        /// </summary>
        public async Task<ExportTemplateResolveResult> GetTemplateAsync(long formTypeId)
        {
            var templateDirectory = ResolveTemplateDirectory();
            if (!System.IO.Directory.Exists(templateDirectory))
            {
                logger.LogWarning("Contact export template directory not found. Directory: {TemplateDirectory}", templateDirectory);
                return new ExportTemplateResolveResult();
            }

            var files = System.IO.Directory.GetFiles(templateDirectory, $"form-{formTypeId}-*.json")
                .OrderBy(e => e, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!files.Any())
            {
                return new ExportTemplateResolveResult();
            }

            if (files.Count > 1)
            {
                // 同一表單類別只能有一份範本，避免欄位順序來源不明確。
                throw new InvalidOperationException($"聯絡表單匯出範本重複：FormTypeId={formTypeId}，Files={string.Join(", ", files.Select(Path.GetFileName))}");
            }

            var filePath = files.Single();
            ContactExportTemplateDefinition? template;
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                template = JsonSerializer.Deserialize<ContactExportTemplateDefinition>(json, new JsonSerializerOptions
                {
                    // 範本由維護人員手動調整，允許大小寫不同、註解與尾逗號，降低格式維護成本。
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"聯絡表單匯出範本 JSON 格式錯誤：{Path.GetFileName(filePath)}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"聯絡表單匯出範本讀取失敗：{Path.GetFileName(filePath)}", ex);
            }

            if (template == null)
            {
                throw new InvalidOperationException($"聯絡表單匯出範本無法解析：{Path.GetFileName(filePath)}");
            }

            if (template.FormTypeId != formTypeId)
            {
                throw new InvalidOperationException($"聯絡表單匯出範本 FormTypeId 與檔名不一致：File={Path.GetFileName(filePath)}，FormTypeId={template.FormTypeId}");
            }

            if (template.Columns == null)
            {
                throw new InvalidOperationException($"聯絡表單匯出範本缺少 columns：{Path.GetFileName(filePath)}");
            }

            return new ExportTemplateResolveResult
            {
                HasTemplate = true,
                Columns = NormalizeColumns(formTypeId, template.Columns)
            };
        }

        /// <summary>
        /// 保留欄位清單 API 供既有呼叫端使用；新流程需判斷 HasTemplate 時請使用 GetTemplateAsync。
        /// </summary>
        public async Task<IReadOnlyList<ExportColumnMetadata>> GetTemplateColumnsAsync(long formTypeId)
        {
            var template = await GetTemplateAsync(formTypeId);
            return template.Columns;
        }

        /// <summary>
        /// 解析設定路徑；相對路徑以網站 ContentRootPath 為起點。
        /// </summary>
        private string ResolveTemplateDirectory()
        {
            var configuredDirectory = configuration.GetValue<string>(TemplateDirectoryConfigKey);
            var templateDirectory = string.IsNullOrWhiteSpace(configuredDirectory)
                ? DefaultTemplateDirectory
                : configuredDirectory.Trim();

            return Path.IsPathRooted(templateDirectory)
                ? templateDirectory
                : Path.Combine(environment.ContentRootPath, templateDirectory);
        }

        /// <summary>
        /// 清理範本欄位並排除 captcha；欄位順序保留 JSON 陣列順序，重複 key 保留範本中第一筆。
        /// </summary>
        private IReadOnlyList<ExportColumnMetadata> NormalizeColumns(long formTypeId, List<ExportColumnMetadata> columns)
        {
            var result = new List<ExportColumnMetadata>();
            var knownKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var column in columns)
            {
                var normalizedKey = FromDateParser.NormalizeKey(column.ColumnKey);
                if (string.IsNullOrWhiteSpace(normalizedKey))
                {
                    throw new InvalidOperationException($"聯絡表單匯出範本欄位缺少 columnKey：FormTypeId={formTypeId}");
                }

                if (string.Equals(normalizedKey, "captcha", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!knownKeys.Add(normalizedKey))
                {
                    logger.LogWarning("Contact export template duplicated column key. FormTypeId: {FormTypeId}, ColumnKey: {ColumnKey}", formTypeId, column.ColumnKey);
                    continue;
                }

                result.Add(new ExportColumnMetadata
                {
                    ColumnKey = column.ColumnKey.Trim(),
                    ColumnTitle = string.IsNullOrWhiteSpace(column.ColumnTitle) ? column.ColumnKey.Trim() : column.ColumnTitle.Trim(),
                    Visible = column.Visible,
                    Format = column.Format
                });
            }

            return result;
        }
    }

    /// <summary>
    /// 聯絡表單匯出 JSON 範本檔的根節點。
    /// </summary>
    public class ContactExportTemplateDefinition
    {
        /// <summary>
        /// 對應 WebMenus.Id，用來確認範本檔未放錯表單。
        /// </summary>
        public long FormTypeId { get; set; }

        /// <summary>
        /// 範本辨識名稱，僅供維護人員閱讀。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 範本定義的動態欄位集合。
        /// </summary>
        public List<ExportColumnMetadata> Columns { get; set; } = new List<ExportColumnMetadata>();
    }
}
