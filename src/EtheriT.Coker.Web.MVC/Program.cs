using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using DevExpress.XtraCharts;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Advertise;
using EtheriT.Coker.Application.Article;
using EtheriT.Coker.Application.AuditLog;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.BonusManagement;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Company;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Contact;
using EtheriT.Coker.Application.Directory;
using EtheriT.Coker.Application.FileManagement;
using EtheriT.Coker.Application.Filters;
using EtheriT.Coker.Application.FlowSize;
using EtheriT.Coker.Application.Freight;
using EtheriT.Coker.Application.HtmlContent;
using EtheriT.Coker.Application.Import;
using EtheriT.Coker.Application.JsonObject;
using EtheriT.Coker.Application.Marquee;
using EtheriT.Coker.Application.Member;
using EtheriT.Coker.Application.Newsletter;
using EtheriT.Coker.Application.Order;
using EtheriT.Coker.Application.Permissions;
using EtheriT.Coker.Application.Processor;
using EtheriT.Coker.Application.Product;
using EtheriT.Coker.Application.Remote;
using EtheriT.Coker.Application.Report;
using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion.Auth;
using EtheriT.Coker.Application.Shared.FileManagement;
using EtheriT.Coker.Application.Shared.FlowSize;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Application.Shared.Reporting;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Shared.Specification;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.Shared.UserHabits;
using EtheriT.Coker.Application.ShoppingCart;
using EtheriT.Coker.Application.Specification;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Tag;
using EtheriT.Coker.Application.TechnicalCertificate;
using EtheriT.Coker.Application.Templates;
using EtheriT.Coker.Application.ThirdParty;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Application.UserHabits;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Controllers.DevExpress;
using EtheriT.Coker.Web.MVC.Extensions;
using EtheriT.Coker.Web.MVC.Middleware;
using EtheriT.Coker.Web.MVC.Resources;
using EtheriT.Coker.Web.MVC.Startup;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using MiniExcelLibs;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
var authenticationConfig = builder.Configuration.GetSection("Authentication");

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews().AddNewtonsoftJson();
builder.Services.AddSingleton<JwtHelpers>();
builder.Services.AddCors();
builder.Services.AddMemoryCache()
    .AddSimpleCaptcha(builder =>
    {
        builder.UseMemoryStore();
    });

var OAuth = builder.Services
    .AddAuthentication(options =>
    {
        // custom scheme defined in .AddPolicyScheme() below
        options.DefaultScheme = "JWT_OR_COOKIE";  // 讓 API 可以使用 JWT 或 Cookie
        options.DefaultAuthenticateScheme = "JWT_OR_COOKIE";
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme; // 遇到 401 時優先跳轉登入
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
        options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

            // 一般我們都會驗證 Issuer
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

            // 通常不太需要驗證 Audience
            ValidateAudience = false,
            //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

            // 一般我們都會驗證 Token 的有效期間
            ValidateLifetime = true,

            // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
            ValidateIssuerSigningKey = true,

            // "1234567890123456" 應該從 IConfiguration 取得
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT 驗證失敗: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"JWT 驗證成功: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    })
    .AddPolicyScheme("JWT_OR_COOKIE", "Select JWT or Cookie dynamically", options =>
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
var LineConfig = authenticationConfig.GetSection("Line");
if (!string.IsNullOrEmpty(LineConfig["ChannelId"]) && !string.IsNullOrEmpty(LineConfig["ChannelSecret"])) {
    OAuth.AddLine(options =>
    {
        options.ClientId = LineConfig["ChannelId"] ?? "";
        options.ClientSecret = LineConfig["ChannelSecret"] ?? "";
        options.CallbackPath = "/SigninLine";

        options.Scope.Clear(); // 清掉 LINE 套件預設的 profile
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.SaveTokens = true;

        options.Events.OnCreatingTicket = async ctx =>
        {
            var idToken = ctx.TokenResponse.Response?.RootElement.GetProperty("id_token").GetString();
            if (!string.IsNullOrEmpty(idToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(idToken);

                if (jwt.Payload.TryGetValue("email", out var email))
                {
                    ctx.Identity?.AddClaim(new Claim(ClaimTypes.Email, email?.ToString()!));
                }

                if (jwt.Payload.TryGetValue("name", out var name))
                {
                    ctx.Identity?.AddClaim(new Claim(ClaimTypes.Name, name?.ToString()!));
                }
            }
        };

        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            var uri = new UriBuilder(context.RedirectUri);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            var updated = new Dictionary<string, string>();
            foreach (var kv in query)
                updated[kv.Key] = kv.Value.ToString();

            updated["bot_prompt"] = "normal";

            uri.Query = string.Join("&", updated.Select(kv => $"{kv.Key}={kv.Value}"));
            context.Response.Redirect(uri.ToString());
            return Task.CompletedTask;
        };

        options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;    // 先設好
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    });
}
var GoogleConfig = authenticationConfig.GetSection("Google");
if (!string.IsNullOrEmpty(GoogleConfig["ClientId"]) && !string.IsNullOrEmpty(GoogleConfig["ClientSecret"]))
{
    OAuth.AddGoogle(options =>
    {
        options.ClientId = GoogleConfig["ClientId"] ?? "";
        options.ClientSecret = GoogleConfig["ClientSecret"] ?? "";
        options.CallbackPath = "/signin-google";
    });
}
var FacebookConfig = authenticationConfig.GetSection("Facebook");
if (!string.IsNullOrEmpty(FacebookConfig["AppId"]) && !string.IsNullOrEmpty(FacebookConfig["AppSecret"]))
{
    OAuth.AddFacebook(options =>
    {
        options.AppId = FacebookConfig["AppId"] ?? "";
        options.AppSecret = FacebookConfig["AppSecret"] ?? "";
        options.CallbackPath = "/signin-facebook";
    });
}
var AppleConfig = authenticationConfig.GetSection("Apple");
var privateKeyPath = AppleConfig["PrivateKeyPath"];
if (!string.IsNullOrEmpty(AppleConfig["ClientId"]) && !string.IsNullOrEmpty(AppleConfig["KeyId"]) && !string.IsNullOrEmpty(AppleConfig["TeamId"]) && !string.IsNullOrEmpty(privateKeyPath)) {
    OAuth.AddApple("Apple", options => {
        options.ClientId = AppleConfig["ClientId"] ?? "";
        options.KeyId = AppleConfig["KeyId"] ?? "";
        options.TeamId = AppleConfig["TeamId"] ?? "";
        options.CallbackPath = "/signin-apple";
        var fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
        options.UsePrivateKey(fileName =>
        {
            return fileProvider.GetFileInfo(privateKeyPath);
        });
    });
}

builder.Services.AddAntiforgery(options =>
{
    // input name的名稱
    options.FormFieldName = "AntiforgeryField";
    // 指定header 的名稱
    options.HeaderName = "x-xsrf-token";
    options.Cookie.Name = "cokerAntiforgeryCookie"; // 指定固定的 Cookie 名稱
    options.Cookie.MaxAge = TimeSpan.FromMinutes(30); // 設置 Cookie 的有效期
    options.Cookie.HttpOnly = true;
});

// 添加 Hangfire 服務，並配置使用 SQL Server 存儲
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_110)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(configuration.GetConnectionString("Default"))
);
// 註冊 Hangfire 跟蹤服務
builder.Services.AddHangfireServer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<BackgroundJobService>();
builder.Services.AddScoped<LoginUserData>();
builder.Services.AddScoped<NavigationProvider>();

// 設定 CORS 策略
builder.Services.AddScoped<ICorsPolicyProvider, DynamicCorsPolicyProvider>();

builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IAccountAppService, AccountAppService>();
builder.Services.AddScoped<ITokenAppService, TokenAppService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IWebsiteApplication, WebsiteApplication>();
builder.Services.AddScoped<IMarqueeAppService, MarqueeAppService>();
builder.Services.AddScoped<IOrderAppService, OrderAppService>();
builder.Services.AddScoped<IShoppingCartAppService, ShoppingCartAppService>();
builder.Services.AddScoped<IMemberAppService, MemberAppService>();
builder.Services.AddScoped<IFreightAppService, FreightAppService>();
builder.Services.AddScoped<IProductAppService, ProductAppService>();
builder.Services.AddScoped<IHtmlContentAppService, HtmlContentAppService>();
builder.Services.AddScoped<ITechnicalCertificateAppService, TechnicalCertificateAppService>();
builder.Services.AddScoped<IWebMenuApplication, WebMenuApplication>();
builder.Services.AddScoped<ImportAppService>();
builder.Services.AddScoped<StringHandler>();
builder.Services.AddScoped<MailAppService>();
builder.Services.AddScoped<ISpecificationAppService, SpecificationAppService>();
builder.Services.AddScoped<ITagAppService, TagAppService>();
builder.Services.AddScoped<IFileUploadAppService, FileUploadAppService>();
builder.Services.AddScoped<IObjectTypeAppService, ObjectTypeAppService>();
builder.Services.AddScoped<IArticleAppService, ArticleAppService>();
builder.Services.AddScoped<IAdvertiseAppService, AdvertiseAppService>();
builder.Services.AddScoped<IDirectoryAppService, DirectoryAppService>();
builder.Services.AddScoped<IStoreSetAppService, StoreSetAppService>();
builder.Services.AddScoped<ICustSearchAppService, CustSearchAppService>();
builder.Services.AddScoped<ICompanyAppService, CompanyAppService>();
builder.Services.AddScoped<IAuditLogAppService, AuditLogAppService>();
builder.Services.AddScoped<INewsletterAppService, NewsletterAppService>();
builder.Services.AddScoped<IPermissionsAppService, PermissionsAppService>();
builder.Services.AddScoped<IRemoteAppService, RemoteAppService>();
builder.Services.AddScoped<IJsonObjectAppService, JsonObjectAppService>();
builder.Services.AddScoped<ICaptchaAppService, CaptchaAppService>();
builder.Services.AddScoped<IContactAppService, ContactAppService>();
builder.Services.AddScoped<IThirdPartyAppService, ThirdPartyAppService>();
builder.Services.AddScoped<ILinePayAppService, LinePayAppService>();
builder.Services.AddScoped<IPChomePayAppService, PChomePayAppService>();
builder.Services.AddScoped<IShoppingCartAppService, ShoppingCartAppService>();
builder.Services.AddScoped<IHtmlProcessor, HtmlProcessor>();
builder.Services.AddScoped<IUserHabitsAppService, UserHabitsAppService>();
builder.Services.AddScoped<IFlowSizeAppService, FlowSizeAppService>();
builder.Services.AddScoped<IReportingAppService, ReportingAppService>();
builder.Services.AddTransient<IDashboardAuthorizationFilter, HangfireDashboardAuthorizationFilter>();
builder.Services.AddTransient<ITemplatesApplicationService, TemplatesApplicationService>();
builder.Services.AddScoped<UserHabitsWorking>();
builder.Services.AddScoped<IBonusManagementAppService, BonusManagementAppService>();
builder.Services.AddScoped<IFileManagementAppService, FileManagementAppService>();
builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection("Authentication"));
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddSingleton<IThumbnailGeneratorService, ThumbnailGeneratorService>();

//多語系
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);//要使用View多國語系的話就加這行程式碼

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.Configure<VirtualDirectory>(builder.Configuration.GetSection("VirtualDirectory"));
// Add services to the container.
builder.Services.AddControllersWithViews();

//item.UseSqlServer(configuration.GetConnectionString("Default"))

builder.Services.AddDbContext<CokerDbContext>(options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("Default"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        });
    }
);

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("EtheriT.Coker.Web.MVC", new OpenApiInfo { Title = "EtheriT.Coker.Web.MVC API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

if (builder.Configuration.GetValue<bool>("Verify:HttpOnly"))
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
        options.HttpsPort = 443;
    });
}

//註冊HttpClient
builder.Services.AddHttpClient("ThirdPartyClient_Front", client =>
{
    client.BaseAddress = new Uri("https://defaultFrontApi.com");
});
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
builder.Services.AddHttpClient("ThirdPartyClient_ECPay", client =>
{
    client.BaseAddress = new Uri("https://ecpg-stage.ecpay.com.tw/Merchant");
    //client.BaseAddress = new Uri("https://ecpg.ecpay.com.tw/Merchant");
});
builder.Services.AddDevExpressControls();
// DevExpress Reporting
builder.Services.ConfigureReportingServices(configurator =>
{
    configurator.ConfigureWebDocumentViewer(viewerConfigurator =>
    {
        viewerConfigurator.UseCachedReportSourceBuilder();
    });
    configurator.UseAsyncEngine();
    configurator.UseDevelopmentMode();
});
builder.Services.AddTransient<CustomWebDocumentViewerController>();
builder.Services.AddTransient<CustomReportDesignerController>();
builder.Services.AddTransient<CustomQueryBuilderController>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/EtheriT.Coker.Web.MVC/swagger.json", "EtheriT.Coker.Web.MVC v1");
    });
}

var antiforgery = app.Services.GetRequiredService<IAntiforgery>();
app.UseCookiePolicy(
    new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.Always,
        HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
        MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
        OnAppendCookie = ctx =>
        {
            var isCorrelation = ctx.CookieName.StartsWith(".AspNetCore.Correlation.", StringComparison.Ordinal);
            var isAuth = ctx.CookieName == ".AspNetCore.Cookies";          // ← 登入票證
            if (isCorrelation || isAuth)
            {
                ctx.CookieOptions.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                ctx.CookieOptions.Secure = true; // 符合 Chrome 規範
            }
        }
    }
);

var backgroundJobService = app.Services.GetRequiredService<BackgroundJobService>();
backgroundJobService.InitializeJobs();

/*
app.Use((context, next) =>
{
    var requestPath = context.Request.Path.Value;

    if (string.Equals(requestPath, "/", StringComparison.OrdinalIgnoreCase)
        || string.Equals(requestPath, "/Account", StringComparison.OrdinalIgnoreCase))
    {
        var tokenSet = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokenSet.RequestToken!,
            new CookieOptions { HttpOnly = false });
    }

    return next(context);
});*/

//設定虛擬目錄
app.UseVirtualDirectory("upload", builder.Configuration.GetValue<string>("VirtualDirectory:upload"));
app.UseVirtualDirectory("shared", builder.Configuration.GetValue<string>("VirtualDirectory:Shared"));
app.UseVirtualDirectory("layout", builder.Configuration.GetValue<string>("VirtualDirectory:Layout"));

app.UseHttpsRedirection();

// 設定 MIME 類型映射，包括 AVIF 檔案
var fileProvider = new FileExtensionContentTypeProvider();
fileProvider.Mappings[".avif"] = "image/avif";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = fileProvider
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors();

app.UseDevExpressControls();
// 添加 AntiforgeryDebugMiddleware
app.UseMiddleware<AntiforgeryDebugMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();

// 設定 Hangfire 儀表板（可以設置需要權限控制的路徑）
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { app.Services.GetRequiredService<IDashboardAuthorizationFilter>() },  // 使用 DI 解析授權過濾器
    IgnoreAntiforgeryToken = true,
    StatsPollingInterval = 60 * 1000
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

app.Run();
