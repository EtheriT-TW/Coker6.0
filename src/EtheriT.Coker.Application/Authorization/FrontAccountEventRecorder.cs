using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EtheriT.Coker.Application.Authorization;

public sealed class FrontAccountEventRecorder
{
    private readonly IServiceScopeFactory scopes;
    private readonly IHttpContextAccessor accessor;
    private readonly ILogger<FrontAccountEventRecorder> logger;
    public FrontAccountEventRecorder(IServiceScopeFactory scopes, IHttpContextAccessor accessor,
        ILogger<FrontAccountEventRecorder> logger)
    {
        this.scopes = scopes;
        this.accessor = accessor;
        this.logger = logger;
    }

    public static string? Fingerprint(Guid? key) => key == null || key == Guid.Empty ? null :
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key.Value.ToString("N"))));

    public async Task RecordAsync(Account_Log entry)
    {
        var context = accessor.HttpContext;
        entry.CreationTime = DateTime.Now;
        entry.ClientIpAddress = Limit(context?.Connection.RemoteIpAddress?.ToString(), 64);
        entry.BrowserInfo = Limit(context?.Request.Headers["User-Agent"].ToString(), 512);
        entry.CorrelationId = Limit(context?.TraceIdentifier, 128);
        entry.OldEmail = Limit(entry.OldEmail, 150);
        entry.NewEmail = Limit(entry.NewEmail, 150);
        entry.RecipientEmail = Limit(entry.RecipientEmail, 150);
        try
        {
            // Independent context: recording a rejected operation must not save its pending changes.
            using var scope = scopes.CreateScope();
            var eventDb = scope.ServiceProvider.GetRequiredService<CokerDbContext>();
            eventDb.Account_Logs.Add(entry);
            await eventDb.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Do not change an already-completed business result, or print secrets from an exception.
            logger.LogError("Front account event could not be persisted. Event={Event} Trace={Trace}",
                entry.EventName, entry.CorrelationId);
        }
    }

    private static string? Limit(string? value, int length) => value?.Length > length ? value[..length] : value;
}
