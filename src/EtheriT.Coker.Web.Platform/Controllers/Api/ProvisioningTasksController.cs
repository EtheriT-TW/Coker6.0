using System.Data;
using System.Net;
using System.Text.Json;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Provisioning;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/provisioning/tasks")]
public sealed class ProvisioningTasksController(CokerDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpGet("servers")]
    public IReadOnlyList<ProvisioningServerDto> GetServers() => GetConfiguredServers()
        .Select(x => new ProvisioningServerDto(x.Id, string.IsNullOrWhiteSpace(x.DisplayName) ? x.Id : x.DisplayName, x.IsDnsServer))
        .ToList();

    [HttpGet]
    public async Task<IReadOnlyList<ProvisioningTaskDto>> GetList()
    {
        var tasks = await db.ProvisioningTasks.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(100)
            .ToListAsync(HttpContext.RequestAborted);
        return tasks.Select(ToDto).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<ProvisioningTaskDto>> Create(CreateProvisioningTaskRequest request)
    {
        Validate(request);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var task = new ProvisioningTask
        {
            TargetServerId = request.TargetServerId.Trim(),
            Type = request.Type,
            Status = ProvisioningTaskStatus.Pending,
            PayloadJson = JsonSerializer.Serialize(new ProvisioningTaskPayload(
                Clean(request.SiteName), request.StartSite, Clean(request.ZoneName),
                Clean(request.RecordName), Clean(request.IPv4Address),
                NormalizeHosts(request.HostNames))),
            CreatedAtUtc = DateTime.UtcNow
        };
        db.ProvisioningTasks.Add(task);
        await db.SaveChangesAsync(HttpContext.RequestAborted);
        return Created($"/api/provisioning/tasks/{task.Id}", ToDto(task));
    }

    private void Validate(CreateProvisioningTaskRequest request)
    {
        var servers = GetConfiguredServers();
        if (string.IsNullOrWhiteSpace(request.TargetServerId) ||
            !servers.Any(x => string.Equals(x.Id, request.TargetServerId.Trim(), StringComparison.OrdinalIgnoreCase)))
            ModelState.AddModelError(nameof(request.TargetServerId), "目標伺服器不在允許清單中。");

        if (!Enum.IsDefined(request.Type))
            ModelState.AddModelError(nameof(request.Type), "不支援的任務類型。");

        if (request.Type == ProvisioningTaskType.SetIisSiteState && string.IsNullOrWhiteSpace(request.SiteName))
            ModelState.AddModelError(nameof(request.SiteName), "請輸入 IIS 網站名稱。");

        if (request.Type is ProvisioningTaskType.CreateDnsARecord or ProvisioningTaskType.DeleteDnsARecord)
        {
            if (!servers.Any(x => x.IsDnsServer && string.Equals(x.Id, request.TargetServerId?.Trim(), StringComparison.OrdinalIgnoreCase)))
                ModelState.AddModelError(nameof(request.TargetServerId), "DNS 任務只能交由 DNS Server 執行。");
            if (!IsDnsName(request.ZoneName)) ModelState.AddModelError(nameof(request.ZoneName), "DNS Zone 格式不正確。");
            if (!IsRecordName(request.RecordName)) ModelState.AddModelError(nameof(request.RecordName), "DNS 記錄名稱格式不正確。");
            if (!IPAddress.TryParse(request.IPv4Address, out var address) || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                ModelState.AddModelError(nameof(request.IPv4Address), "請輸入正確的 IPv4 位址。");
        }

        if (request.Type == ProvisioningTaskType.InstallSsl)
        {
            var hosts = NormalizeHosts(request.HostNames);
            if (hosts.Count == 0) ModelState.AddModelError(nameof(request.HostNames), "請至少輸入一個 SSL 主機名稱。");
            else if (hosts.Count > 100) ModelState.AddModelError(nameof(request.HostNames), "一次最多處理 100 個 SSL 主機名稱。");
            else if (hosts.Any(x => !IsDnsName(x))) ModelState.AddModelError(nameof(request.HostNames), "SSL 主機名稱格式不正確。");
        }
    }

    private static bool IsDnsName(string? value) =>
        !string.IsNullOrWhiteSpace(value) && Uri.CheckHostName(value.Trim()) == UriHostNameType.Dns;

    private static bool IsRecordName(string? value) =>
        !string.IsNullOrWhiteSpace(value) && (value.Trim() == "@" ||
        Uri.CheckHostName(value.Trim()) == UriHostNameType.Dns);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private List<ProvisioningServerOptions> GetConfiguredServers() =>
        (configuration.GetSection("Provisioning:Servers").Get<List<ProvisioningServerOptions>>() ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToList();

    private static IReadOnlyList<string> NormalizeHosts(IReadOnlyList<string>? hosts) =>
        (hosts ?? []).SelectMany(x => x.Split([',', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    internal static ProvisioningTaskDto ToDto(ProvisioningTask task) => new(
        task.Id, task.TargetServerId, task.Type, task.Status,
        JsonSerializer.Deserialize<ProvisioningTaskPayload>(task.PayloadJson) ?? new(null, null, null, null, null, null),
        task.AttemptCount, task.CreatedAtUtc, task.StartedAtUtc, task.CompletedAtUtc, task.ResultMessage);
}

[ApiController]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[Route("api/provisioning/agent/tasks")]
public sealed class ProvisioningAgentTasksController(
    CokerDbContext db,
    ProvisioningAgentAuthenticator authenticator) : ControllerBase
{
    private const string ApiKeyHeader = "X-Provisioning-Key";

    [HttpPost("claim")]
    public async Task<ActionResult<ProvisioningTaskDto>> Claim(ClaimProvisioningTaskRequest request)
    {
        if (!Authenticate(request.ServerId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.ServerId) || string.IsNullOrWhiteSpace(request.WorkerId)) return BadRequest();

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, HttpContext.RequestAborted);
        var now = DateTime.UtcNow;
        // 無法得知 Worker 中斷前是否已完成主機操作，故逾時任務標記失敗、禁止自動重跑，避免重複重啟或重複異動 DNS。
        await db.ProvisioningTasks
            .Where(x => x.TargetServerId == request.ServerId &&
                x.Status == ProvisioningTaskStatus.Running && x.LeaseExpiresAtUtc < now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, ProvisioningTaskStatus.Failed)
                .SetProperty(x => x.CompletedAtUtc, now)
                .SetProperty(x => x.LeaseExpiresAtUtc, (DateTime?)null)
                .SetProperty(x => x.ResultMessage, "Worker 執行逾時；為避免重複操作，系統未自動重試。請人工確認主機狀態。"),
                HttpContext.RequestAborted);
        var task = await db.ProvisioningTasks
            .Where(x => x.TargetServerId == request.ServerId &&
                x.Status == ProvisioningTaskStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        if (task is null)
        {
            await transaction.CommitAsync(HttpContext.RequestAborted);
            return NoContent();
        }

        task.Status = ProvisioningTaskStatus.Running;
        task.ClaimedBy = request.WorkerId.Trim();
        task.StartedAtUtc ??= now;
        task.LeaseExpiresAtUtc = now.AddMinutes(10);
        task.AttemptCount++;
        await db.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);
        return ProvisioningTasksController.ToDto(task);
    }

    [HttpPost("{id:long}/complete")]
    public async Task<IActionResult> Complete(long id, CompleteProvisioningTaskRequest request)
    {
        if (!Authenticate(request.ServerId)) return Unauthorized();
        var task = await db.ProvisioningTasks.FirstOrDefaultAsync(x => x.Id == id, HttpContext.RequestAborted);
        if (task is null) return NotFound();
        if (!string.Equals(task.TargetServerId, request.ServerId, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(task.ClaimedBy, request.WorkerId, StringComparison.Ordinal)) return Conflict();

        task.Status = request.Succeeded ? ProvisioningTaskStatus.Succeeded : ProvisioningTaskStatus.Failed;
        task.CompletedAtUtc = DateTime.UtcNow;
        task.LeaseExpiresAtUtc = null;
        task.ResultMessage = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim()[..Math.Min(request.Message.Trim().Length, 4000)];
        await db.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private bool Authenticate(string? serverId)
    {
        return authenticator.IsValid(serverId, Request.Headers[ApiKeyHeader].FirstOrDefault());
    }
}
