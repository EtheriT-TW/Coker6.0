using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EtheriT.Coker.Web.MVC.Extensions
{
    public class DynamicCorsPolicyProvider : ICorsPolicyProvider
    {
        private readonly IServiceProvider _sp;

        public DynamicCorsPolicyProvider(IServiceProvider sp) => _sp = sp;

        public async Task<CorsPolicy> GetPolicyAsync(HttpContext context, string? policyName)
        {
            var origin = context.Request.Headers["Origin"].ToString();
            if (string.IsNullOrWhiteSpace(origin)) return null!;

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
                return null!;

            // 只保留 scheme + host + port，形成「正規化 Origin」
            string NormalizeOrigin(Uri u)
                => u.IsDefaultPort ? $"{u.Scheme}://{u.Host}" : $"{u.Scheme}://{u.Host}:{u.Port}";

            var normalizedOrigin = NormalizeOrigin(originUri);

            using var scope = _sp.CreateScope();
            var env = _sp.GetRequiredService<IHostEnvironment>();

            string[] allowed;
            if (env.IsProduction())
            {
                var db = scope.ServiceProvider.GetRequiredService<CokerDbContext>();

                // 從 DB 取出、解析、正規化，只留下 Origin
                var urls = await db.Websites
                    .Where(w => w.DefaultUrl != null)
                    .Where(w => EF.Functions.Like(w.DefaultUrl!, "http%"))
                    .Select(w => w.DefaultUrl!.Trim())   // 先去除前後空白
                    .Distinct()
                    .ToListAsync();

                allowed = urls
                    .Select(s => Uri.TryCreate(s, UriKind.Absolute, out var u) ? NormalizeOrigin(u) : null)
                    .Where(s => s != null)
                    .Cast<string>()
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            else
            {
                // 開發/測試環境：允許目前的 Origin
                allowed = new[] { normalizedOrigin };
            }

            // 大小寫不敏感比對
            var isAllowed = allowed.Contains(normalizedOrigin, StringComparer.OrdinalIgnoreCase);

            if (!isAllowed) return null!;

            // 回應時必須填「精確的」Origin
            return new CorsPolicyBuilder()
                .WithOrigins(normalizedOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials() // 需要 cookie / auth header 時
                .Build();
        }
    }
}
