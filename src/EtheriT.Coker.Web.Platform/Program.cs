using EtheriT.Coker.Authentication.Backoffice;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Security;
using EtheriT.Coker.Web.Platform.Services;
using EtheriT.Coker.Web.Platform.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews(options =>
    {
        options.Filters.Add(new AuthorizeFilter(BackofficeAuthorizationPolicies.PlatformAccess));
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    })
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = ".Coker6.Platform.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("backoffice-reauthentication", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});
builder.Services.AddDbContext<CokerDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddCokerBackofficeAuthentication(
    builder.Configuration,
    enableJwtBearer: false,
    configureCookie: options =>
    {
        options.LoginPath = "/Home/AccessDenied";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddScoped<IBackofficeSessionValidator, PlatformBackofficeSessionValidator>();
builder.Services.AddSingleton<PlatformReauthenticationTicketService>();
builder.Services.AddScoped<PlatformReauthenticationService>();
builder.Services.AddScoped<IAuthorizationHandler, PlatformAccessHandler>();
builder.Services.AddScoped<PlatformAuditor>();
builder.Services.AddSingleton<PlatformDomainPasswordProtector>();
builder.Services.AddSingleton<ViteManifestService>();
builder.Services.AddSingleton<ProvisioningAgentAuthenticator>();
builder.Services.AddAuthorization(options =>
{
    var platformAccessPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddRequirements(new PlatformAccessRequirement())
        .Build();

    options.AddPolicy(BackofficeAuthorizationPolicies.PlatformAccess, platformAccessPolicy);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseMiddleware<PlatformApiControlMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PlatformHost}/{action=Index}/{id?}")
    .WithStaticAssets();

// 不存在的 /api 路徑回 404；落到 SPA Host 會回 200 + HTML，前端會把 HTML 當成 API 資料
app.MapFallback("/api/{**path}", context =>
{
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    return Task.CompletedTask;
});

app.MapFallbackToController("Index", "PlatformHost");

app.Run();
