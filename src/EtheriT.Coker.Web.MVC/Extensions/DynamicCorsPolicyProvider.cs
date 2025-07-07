using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EtheriT.Coker.Web.MVC.Extensions
{
    public class DynamicCorsPolicyProvider : ICorsPolicyProvider
    {
        private readonly IServiceProvider _serviceProvider;
        public DynamicCorsPolicyProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task<CorsPolicy> GetPolicyAsync(HttpContext context, string? policyName)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CokerDbContext>();

            var origin = context.Request.Headers["Origin"].ToString();
            if (string.IsNullOrEmpty(origin)) return null!;
            var env = _serviceProvider.GetRequiredService<IHostEnvironment>();
            var allowedOrigins = new string[] { origin };
            if (env.IsProduction())
            {
                allowedOrigins = await db.Websites
                .Where(w => w.DefaultUrl != null && w.DefaultUrl.StartsWith("https"))
                .Select(w => w.DefaultUrl.TrimEnd('/'))
                .Distinct()
                .ToArrayAsync();   
            }
            if (allowedOrigins.Any(o => origin.StartsWith(o)))
            {
                return new CorsPolicyBuilder()
                .WithOrigins(origin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .Build();
            }
            return null!;
        }
    }
}
