using System.Security.Claims;
using AutoMapper;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Directory = System.IO.Directory;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

// A new DI container, in-memory database and file root for EACH scenario.
// No production connection strings, mail transport, HTTP clients, cache or jobs.
public sealed class BackofficeSandboxFactory(ServiceDescriptor[] registrations)
{
    public async Task<BackofficeSandbox> CreateAsync(IMapper mapper, WebsiteLevelEnum level, CancellationToken token)
    {
        var id = Guid.NewGuid().ToString("N");
        var parent = Path.Combine(Path.GetTempPath(), "CokerDiagnostics");
        var folder = Path.Combine(parent, id);
        Directory.CreateDirectory(folder);
        IServiceCollection services = new ServiceCollection();
        foreach (var descriptor in registrations.Where(item => item.ServiceType != typeof(IHostedService))) services.Add(descriptor);
        services.RemoveAll<CokerDbContext>();
        var memoryRoot = new Microsoft.EntityFrameworkCore.Storage.InMemoryDatabaseRoot();
        var memoryOptions = new DbContextOptionsBuilder<CokerDbContext>()
            .UseInMemoryDatabase("CokerDiagnostics-" + id, memoryRoot).Options;
        services.RemoveAll<DbContextOptions<CokerDbContext>>();
        services.AddSingleton(memoryOptions);
        services.AddScoped(_ => new CokerDbContext(memoryOptions));
        services.RemoveAll<IMemoryCache>();
        services.AddMemoryCache();
        services.RemoveAll<IMapper>(); services.AddSingleton(mapper);
        services.RemoveAll<IConfiguration>();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["VirtualDirectory:Upload"] = folder,
            ["SystemLinks:PublicUrl"] = "https://diagnostics.invalid",
            ["WebConfig:SiteId"] = "0"
        }).Build());
        services.RemoveAll<IOptions<VirtualDirectory>>();
        services.AddSingleton(Options.Create(new VirtualDirectory { Upload = folder }));
        services.RemoveAll<IDataProtectionProvider>();
        services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        services.RemoveAll<IHttpClientFactory>();
        services.AddSingleton<IHttpClientFactory>(new SandboxHttpClients());
        services.RemoveAll<IBackgroundJobClient>();
        services.AddSingleton<IBackgroundJobClient>(new SandboxJobs());
        services.RemoveAll<IHttpContextAccessor>();
        services.AddSingleton<IHttpContextAccessor>(new SandboxHttpContextAccessor());
        var environment = new SandboxEnvironment(folder);
        services.RemoveAll<IWebHostEnvironment>(); services.AddSingleton<IWebHostEnvironment>(environment);
        services.RemoveAll<IHostEnvironment>(); services.AddSingleton<IHostEnvironment>(environment);
        var provider = services.BuildServiceProvider();
        var scope = provider.CreateAsyncScope();
        var sandbox = new BackofficeSandbox(provider, scope, folder, parent);
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<CokerDbContext>();
            if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
                throw new InvalidOperationException("The sandbox database must be in memory.");
            await db.Database.EnsureCreatedAsync(token);
            const long fixture = 999000001;
            db.Websites.Add(new Website { Id = fixture, Title = "功能檢測", OrgName = "diagnostics", Locale = "zh-tw", Type = "web", Level = level, ContactMail = "sender@example.invalid", DefaultUrl = "https://example.invalid" });
            db.Users.Add(new User { Id = fixture, Name = "檢測帳號", Account = "diagnostics", Password = "unusable", Email = "tester@example.invalid" });
            db.Roles.Add(new Role { Id = fixture, Name = "檢測管理者", FK_WebsiteId = fixture, Type = RoleTypeEnum.系統維護, IsSuperUser = true });
            db.MappingUserAndRoles.Add(new MappingUserAndRole { UserId = fixture, RoleId = fixture });
            await db.SaveChangesAsync(token);
            scope.ServiceProvider.GetRequiredService<BackgroundOperationContext>().Set(fixture, fixture);
            var http = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext!;
            http.RequestServices = scope.ServiceProvider;
            return sandbox;
        }
        catch { await sandbox.DisposeAsync(); throw; }
    }

    private sealed class SandboxHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "diagnostics") }, "Diagnostics"))
        };
    }

    private sealed class SandboxEnvironment(string folder) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "CokerDiagnostics";
        public string EnvironmentName { get; set; } = "Diagnostics";
        public string ContentRootPath { get; set; } = folder;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = folder;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class SandboxHttpClients : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(new DenyNetwork());
    }
    private sealed class DenyNetwork : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) =>
            throw new InvalidOperationException("Network calls are disabled in the diagnostics sandbox.");
    }
    private sealed class SandboxJobs : IBackgroundJobClient
    {
        public string Create(Job job, IState state) => throw new InvalidOperationException("Background jobs are disabled in the diagnostics sandbox.");
        public bool ChangeState(string jobId, IState state, string? expectedState) => throw new InvalidOperationException("Background jobs are disabled in the diagnostics sandbox.");
    }
}

public sealed class BackofficeSandbox(ServiceProvider provider, AsyncServiceScope scope, string folder, string parent) : IAsyncDisposable
{
    public IServiceProvider Services => scope.ServiceProvider;
    public CokerDbContext Db => Services.GetRequiredService<CokerDbContext>();
    public async ValueTask DisposeAsync()
    {
        await scope.DisposeAsync();
        await provider.DisposeAsync();
        var resolved = Path.GetFullPath(folder);
        var root = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (resolved.StartsWith(root, StringComparison.OrdinalIgnoreCase) && Directory.Exists(resolved))
            Directory.Delete(resolved, true);
    }
}
