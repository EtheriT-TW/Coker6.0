using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Article;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Contact;
using EtheriT.Coker.Application.Directory;
using EtheriT.Coker.Application.Freight;
using EtheriT.Coker.Application.HtmlContent;
using EtheriT.Coker.Application.Import;
using EtheriT.Coker.Application.JsonObject;
using EtheriT.Coker.Application.Marquee;
using EtheriT.Coker.Application.Order;
using EtheriT.Coker.Application.Permissions;
using EtheriT.Coker.Application.Product;
using EtheriT.Coker.Application.Remote;
using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Shared.Specification;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.ShoppingCart;
using EtheriT.Coker.Application.Specification;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Tag;
using EtheriT.Coker.Application.TechnicalCertificate;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Resources;
using EtheriT.Coker.Web.Public.Middlewares;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.CookiePolicy;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using EtheriT.Coker.Application.Advertise;
using EtheriT.Coker.Application.Processor;
using EtheriT.Coker.Application.Shared.Processor;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Net.Http.Headers;
using EtheriT.Coker.Application.Newsletter;
using Microsoft.Extensions.FileProviders;
using EtheriT.Coker.Application.Shared.Favorites;
using EtheriT.Coker.Application.Favorites;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.ThirdParty;
using EtheriT.Coker.Application.Shared.Recipients;
using EtheriT.Coker.Application.Recipients;
using Serilog;
using Serilog.Filters;
using static System.Formats.Asn1.AsnWriter;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);
var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();



builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);

// 配置 JWT Bearer 認證
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "JWT_OR_COOKIE";
        options.DefaultChallengeScheme = "JWT_OR_COOKIE";
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // 是否驗證發行者
            ValidateAudience = true, // 是否驗證接收者
            ValidateLifetime = true, // 是否驗證 Token 的有效期
            ValidateIssuerSigningKey = true, // 是否驗證簽名密鑰
            ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"), // JWT 發行者
            ValidAudience = builder.Configuration.GetValue<string>("JwtSettings:Audience"), // JWT 接收者
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey"))), // 密鑰
            ClockSkew = TimeSpan.Zero // Token 時間允許的偏移量
        };
    }).AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
    {
        // runs on each request
        options.ForwardDefaultSelector = context =>
        {
            // filter by auth type
            string authorization = context.Request.Headers[HeaderNames.Authorization];
            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                return JwtBearerDefaults.AuthenticationScheme;

            // otherwise always check for cookie auth
            return CookieAuthenticationDefaults.AuthenticationScheme;
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});


if (builder.Configuration.GetValue<bool>("Verify:HttpOnly"))
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
        options.HttpsPort = 443;
    });
}
builder.Services.AddAntiforgery(options =>
{
    // Set Cookie properties using CookieBuilder properties†.
    options.FormFieldName = "AntiforgeryFieldname";
    options.HeaderName = "X-CSRF-TOKEN-HEADERNAME";
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    //iis setting
    //options.SuppressXFrameOptionsHeader = false;
});

// Configure Kestrel to allow only HTTP/1.1 and HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(lo =>
    {
        lo.Protocols = HttpProtocols.Http1AndHttp2; // 只允許 HTTP/1.1 和 HTTP/2
    });
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<JwtHelpers>();
builder.Services.AddMemoryCache()
    .AddSimpleCaptcha(builder =>
    {
        builder.UseMemoryStore();
        builder.AddConfiguration(options =>
        {
            options.CodeLength = 4;
            options.ImageWidth = 125;
            options.ImageHeight = 36;
        });
    });

builder.Services.AddDbContext<CokerDbContext>(item =>
    item.UseSqlServer(configuration.GetConnectionString("Default"))
);

builder.Services.AddMvc(options =>
{
    options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
});

builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IAccountAppService, AccountAppService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<MailAppService, MailAppService>();
builder.Services.AddScoped<INewsletterAppService, NewsletterAppService>();
builder.Services.AddScoped<IMarqueeAppService, MarqueeAppService>();
builder.Services.AddScoped<IOrderAppService, OrderAppService>();
builder.Services.AddScoped<IRecipientsAppService, RecipientsAppService>();
builder.Services.AddScoped<ITokenAppService, TokenAppService>();
builder.Services.AddScoped<IShoppingCartAppService, ShoppingCartAppService>();
builder.Services.AddScoped<IProductAppService, ProductAppService>();
builder.Services.AddScoped<IFavoritesAppService, FavoritesAppService>();
builder.Services.AddScoped<IFreightAppService, FreightAppService>();
builder.Services.AddScoped<IThirdPartyAppService, ThirdPartyAppService>();
builder.Services.AddScoped<ILinePayAppService, LinePayAppService>();
builder.Services.AddScoped<IPChomePayAppService, PChomePayAppService>();
builder.Services.AddScoped<IECPayAppService, ECPayAppService>();
builder.Services.AddScoped<IHtmlContentAppService, HtmlContentAppService>();
builder.Services.AddScoped<LoginUserData>();
builder.Services.AddScoped<StringHandler>();
builder.Services.AddScoped<MailAppService>();
builder.Services.AddScoped<ITagAppService, TagAppService>();
builder.Services.AddScoped<IWebMenuApplication, WebMenuApplication>();
builder.Services.AddScoped<IWebsiteApplication, WebsiteApplication>();
builder.Services.AddScoped<IFileUploadAppService, FileUploadAppService>();
builder.Services.AddScoped<IAdvertiseAppService, AdvertiseAppService>();
builder.Services.AddScoped<IArticleAppService, ArticleAppService>();
builder.Services.AddScoped<ITechnicalCertificateAppService, TechnicalCertificateAppService>();
builder.Services.AddScoped<ISpecificationAppService, SpecificationAppService>();
builder.Services.AddScoped<IDirectoryAppService, DirectoryAppService>();
builder.Services.AddScoped<ImportAppService, ImportAppService>();
builder.Services.AddScoped<ISpecificationAppService, SpecificationAppService>();
builder.Services.AddScoped<IStoreSetAppService, StoreSetAppService>();
builder.Services.AddScoped<ICustSearchAppService, CustSearchAppService>();
builder.Services.AddScoped<ICaptchaAppService, CaptchaAppService>();
builder.Services.AddScoped<IContactAppService, ContactAppService>();
builder.Services.AddScoped<IRemoteAppService, RemoteAppService>();
builder.Services.AddScoped<IPermissionsAppService, PermissionsAppService>();
builder.Services.AddScoped<IJsonObjectAppService, JsonObjectAppService>();
builder.Services.AddScoped<ISitemap, Sitemap>();
builder.Services.AddScoped<IHtmlProcessor, HtmlProcessor>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
}

if (builder.Environment.EnvironmentName == "EPZA")
{
    builder.WebHost.UseStaticWebAssets();
}

//註冊HttpClient
builder.Services.AddHttpClient("ThirdPartyClient_Line", client =>
{
    client.BaseAddress = new Uri(configuration.GetValue<string>("ThirdParty:Line:PaymentUrl"));
});
builder.Services.AddHttpClient("ThirdPartyClient_PCHome", client =>
{
    client.BaseAddress = new Uri(configuration.GetValue<string>("ThirdParty:PCHomePay:PaymentUrl"));
});
builder.Services.AddHttpClient("ThirdPartyClient_ECPay", client =>
{
    client.BaseAddress = new Uri(configuration.GetValue<string>("ThirdParty:ECPay:PaymentUrl"));
});

builder.Services.AddResponseCompression(options =>
{
	options.EnableForHttps = true; // 啟用 HTTPS 壓縮
});


// 配置 Serilog
string logPath = $"{configuration.GetValue<string>("VirtualDirectory:upload")}\\logs\\{DateTime.Now.Date.ToString("yyyy-MM-dd")}.txt";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // 設定最低記錄層級
    .WriteTo.Console()          // 在控制台顯示日誌
    .WriteTo.File(logPath) // 輸出到檔案
    .Filter.ByIncludingOnly(logEvent =>
        logEvent.MessageTemplate.Text.Contains("IP:") &&
        logEvent.MessageTemplate.Text.Contains("URL:") &&
        logEvent.MessageTemplate.Text.Contains("User-Agent:") &&
        logEvent.MessageTemplate.Text.Contains("Request Size:") &&
        logEvent.MessageTemplate.Text.Contains("Response Size:"))
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// 使用響應壓縮中間件
app.UseResponseCompression();

// 中間件來記錄流量
app.UseMiddleware<FlowSizeLogMiddleware>();

if (!app.Environment.IsProduction())
{
    IHostApplicationLifetime lifetime = app.Lifetime;
    // 檢查是否是應用程式的啟動階段
    lifetime.ApplicationStarted.Register(() =>
    {
        // 在應用程式啟動時執行一次的程式碼
        Console.WriteLine("應用程式已啟動，執行初始化程式碼...");

        // 在這裡可以做一些初始化的工作，例如註冊服務或設定狀態
    });
}
var antiforgery = app.Services.GetRequiredService<IAntiforgery>();

app.Use((context, next) =>
{
    var requestPath = context.Request.Path.Value;

    if (string.Equals(requestPath, "/", StringComparison.OrdinalIgnoreCase)
        || string.Equals(requestPath, "/home", StringComparison.OrdinalIgnoreCase))
    {
        var tokenSet = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokenSet.RequestToken!,
            new CookieOptions { HttpOnly = true, Secure = true });
    }
    return next(context);
});
app.UseCookiePolicy(
    new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.Always,
        HttpOnly = HttpOnlyPolicy.Always,
        MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None
    }
);
//app.UseMiddleware<CookieHandlingMiddleware>();

//禁用X-HTTP-Method-Override
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("X-HTTP-Method-Override"))
    {
        context.Request.Headers.Remove("X-HTTP-Method-Override");
    }
    await next();
});

// 定義共用的 OnPrepareResponse 委派
static void ConfigureStaticFileHeaders(StaticFileResponseContext ctx)
{
    ctx.Context.Response.Headers["Accept-Ranges"] = "bytes";
    ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000"; // 例如設定快取一年
    ctx.Context.Response.Headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");
    ctx.Context.Response.Headers["Last-Modified"] = File.GetLastWriteTimeUtc(ctx.File.PhysicalPath).ToString("R");
    var etag = Convert.ToBase64String(Encoding.UTF8.GetBytes(ctx.File.PhysicalPath)); // 基於文件路徑生成 ETag
    ctx.Context.Response.Headers["ETag"] = etag;
}

// 新增一個 Middleware 處理 favicon.ico 的請求
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/favicon.ico")
    {
        // 直接提供 /upload/favicon.ico 檔案
        var filePath = Path.Combine(builder.Configuration.GetValue<string>("VirtualDirectory:upload"), "favicon.ico");

        if (File.Exists(filePath))
        {
            context.Response.ContentType = "image/x-icon"; // 設定正確的 Content-Type
            await context.Response.SendFileAsync(filePath); // 直接提供檔案
            return; // 停止後續處理
        }
    }
    await next(); // 否則繼續處理其他請求
});

app.UseDefaultFiles();
//wwwroot資料夾下的文件
app.UseStaticFiles(new StaticFileOptions()
{
    OnPrepareResponse = ConfigureStaticFileHeaders
});
//upload資料夾下的靜態文件
//其他靜態文件
var fileProvider = new FileExtensionContentTypeProvider();
fileProvider.Mappings[".properties"] = "application/octet-stream";
fileProvider.Mappings[".bcmap"] = "image/svg+xml";
fileProvider.Mappings[".ftl"] = "application/l10n";
fileProvider.Mappings[".dwg"] = "image/vnd.dwg";

//app.UseVirtualDirectory("upload", builder.Configuration.GetValue<string>("VirtualDirectory:upload"));
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(builder.Configuration.GetValue<string>("VirtualDirectory:upload")),
    RequestPath = "/upload",
    OnPrepareResponse = ConfigureStaticFileHeaders,
    ContentTypeProvider = fileProvider
});
//子站靜態文件
List<string> childOrgNames = new List<string>();
builder.Configuration.GetSection("WebConfig:childSiteOrgName").Bind(childOrgNames);

List<string> childFilePath = new List<string>();
builder.Configuration.GetSection("WebConfig:childPath").Bind(childFilePath);

if (childOrgNames != null)
{
    for (int i = 0; i < childOrgNames.Count; i++)
    {
        //app.UseVirtualDirectory($"upload/{childOrgNames[i]}",childFilePath[i]);
        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = new PhysicalFileProvider(childFilePath[i]),
            RequestPath = $"/upload/{childOrgNames[i]}",
            OnPrepareResponse = ConfigureStaticFileHeaders,
            ContentTypeProvider = fileProvider
        });
    }
}

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = fileProvider,
    OnPrepareResponse = ConfigureStaticFileHeaders
});

app.UseMiddleware<PreventHttpRequestSmugglingMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseMiddleware<CustomBadRequestMiddleware>();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
}
app.UseHsts();
app.UseHttpsRedirection();
app.UseMiddleware<ContentSecurityPolicyMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Sitemap",
    pattern: "Sitemap",
    defaults: new { controller = "Sitemap", action = "Index" }
);

app.MapControllerRoute(
    name: "Verify",
    pattern: ".well-known/{option}/{key}",
    defaults: new { controller = "Verify", action = "Index" }
);

app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Page",
    pattern: "{website}/{key}/{option?}/{id?}/{search?}",
    defaults: new { controller = "Page", action = "Index" },
    constraints: new { website = new NotEqual(new List<string> { "upload", "css", "js", "images", "Shared", "lib" }) }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{key?}/{id?}",
    defaults: new { controller = "Page", action = "Index" });

if (app.Environment.IsProduction())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.IsHttps)
        {
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    long siteId = builder.Configuration.GetValue<long>("WebConfig:SiteId");
                    var dbContext = scope.ServiceProvider.GetRequiredService<CokerDbContext>();
                    var website = dbContext.Websites.Where(e => e.Id == siteId && !e.IsDeleted).FirstOrDefault();
                    if (website != null && !string.IsNullOrEmpty(website.DefaultUrl))
                    {
                        var currentHost = context.Request.Host.Host;
                        var mainDomain = website.DefaultUrl.Replace("http://", "").Replace("https://", ""); // 主網域

                        if (!currentHost.Equals(mainDomain, StringComparison.OrdinalIgnoreCase))
                        {
                            var newUrl = $"https://{mainDomain}{context.Request.Path}{context.Request.QueryString}";
                            context.Response.Redirect(newUrl, true); // true 表示 301轉址 
                            return;
                        }

                        await next();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    });
}

app.UseRouting();

app.Run();
