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

var builder = WebApplication.CreateBuilder(args);
var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();

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
                return "Bearer";

            // otherwise always check for cookie auth
            return "Cookies";
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

builder.Services.AddTransient<IAccountAppService, AccountAppService>();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddTransient<MailAppService, MailAppService>();
builder.Services.AddTransient<INewsletterAppService, NewsletterAppService>();
builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IMarqueeAppService, MarqueeAppService>();
builder.Services.AddTransient<IOrderAppService, OrderAppService>();
builder.Services.AddTransient<ITokenAppService, TokenAppService>();
builder.Services.AddTransient<IShoppingCartAppService, ShoppingCartAppService>();
builder.Services.AddTransient<IProductAppService, ProductAppService>();
builder.Services.AddTransient<IFavoritesAppService, FavoritesAppService>();
builder.Services.AddTransient<IFreightAppService, FreightAppService>();
builder.Services.AddTransient<IThirdPartyAppService, ThirdPartyAppService>();
builder.Services.AddTransient<ILinePayAppService, LinePayAppService>();
builder.Services.AddTransient<IPChomePayAppService, PChomePayAppService>();
builder.Services.AddTransient<IHtmlContentAppService, HtmlContentAppService>();
builder.Services.AddTransient<LoginUserData>();
builder.Services.AddTransient<StringHandler>();
builder.Services.AddTransient<MailAppService>();
builder.Services.AddTransient<ITagAppService, TagAppService>();
builder.Services.AddTransient<IWebMenuApplication, WebMenuApplication>();
builder.Services.AddTransient<IWebsiteApplication, WebsiteApplication>();
builder.Services.AddTransient<IFileUploadAppService, FileUploadAppService>();
builder.Services.AddTransient<IAdvertiseAppService, AdvertiseAppService>();
builder.Services.AddTransient<IArticleAppService, ArticleAppService>();
builder.Services.AddTransient<ITechnicalCertificateAppService, TechnicalCertificateAppService>();
builder.Services.AddTransient<ISpecificationAppService, SpecificationAppService>();
builder.Services.AddTransient<IDirectoryAppService, DirectoryAppService>();
builder.Services.AddTransient<ImportAppService, ImportAppService>();
builder.Services.AddTransient<ISpecificationAppService, SpecificationAppService>();
builder.Services.AddTransient<IStoreSetAppService, StoreSetAppService>();
builder.Services.AddTransient<ICustSearchAppService, CustSearchAppService>();
builder.Services.AddTransient<ICaptchaAppService, CaptchaAppService>();
builder.Services.AddTransient<IContactAppService, ContactAppService>();
builder.Services.AddTransient<IRemoteAppService, RemoteAppService>();
builder.Services.AddTransient<IPermissionsAppService, PermissionsAppService>();
builder.Services.AddTransient<IJsonObjectAppService, JsonObjectAppService>();
builder.Services.AddTransient<ISitemap, Sitemap>();
builder.Services.AddTransient<IHtmlProcessor, HtmlProcessor>();
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
    client.BaseAddress = new Uri("https://sandbox-api-pay.line.me");
    //client.BaseAddress = new Uri("https://api-pay.line.me");
});
builder.Services.AddHttpClient("ThirdPartyClient_PCHome", client =>
{
    client.BaseAddress = new Uri("https://sandbox-api.pchomepay.com.tw");
    //client.BaseAddress = new Uri("https://api.pchomepay.com.tw");
});

var app = builder.Build();

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
        //MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Strict
    }
);

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
app.UseHttpsRedirection();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseMiddleware<CustomBadRequestMiddleware>();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    Console.WriteLine("Request path: " + context.Request.Path);
    var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    // 將 nonce 存入 HttpContext.Items
    context.Items["CSPNonce"] = nonce;
    // 添加 CSP(內容限制) header
    context.Response.Headers["Content-Security-Policy"] =
        $"default-src *;" +
        $"script-src 'self' 'nonce-{nonce}' *.google.com *.googletagmanager.com *.googleadservices.com *.facebook.net *.jquery.com *.yimg.com *.google-analytics.com scaleflex.cloudimg.io googleads.g.doubleclick.net d.line-scdn.net cdn.ckeditor.com remotejs.com 'unsafe-eval'; " +
        $"style-src 'self' 'nonce-{nonce}' *.googleapis.com *.google.com cdnjs.cloudflare.com cdn.ckeditor.com; " +
        $"font-src 'self' data: fonts.gstatic.com cdnjs.cloudflare.com; " +
        $"img-src 'self' *.ezsale.tw *.facebook.com *.yahoo.com *.google.com *.google.com.tw *.google-analytics.com *.googletagmanager.com *.youtube.com i.ytimg.com ad.doubleclick.net googleads.g.doubleclick.net tr.line.me cdn.ckeditor.com data: blob:; " +
        $"frame-ancestors 'self' *.ezsale.tw ";
    //cache 限制設定
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
    //Pragma 為http 1.0以下使用，以上已被 Cache-Control取代
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    //防止瀏覽器進行 MIME 嗅探
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    await next();
});
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
