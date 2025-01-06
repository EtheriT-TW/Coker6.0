using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Resources;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Token;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Net.Http.Headers;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Application.Marquee;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Order;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Member;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Freight;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Product;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.HtmlContent;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Specification;
using EtheriT.Coker.Application.Specification;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Tag;
using EtheriT.Coker.Application.Configuration;
using Microsoft.AspNetCore.Mvc.Razor;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Article;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Advertise;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Directory;
using EtheriT.Coker.Application.Import;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Common;
using Microsoft.Extensions.Options;
using MiniExcelLibs;
using EtheriT.Coker.Application.Search;
using Microsoft.AspNetCore.Antiforgery;
using EtheriT.Coker.Application.Company;
using EtheriT.Coker.Application.AuditLog;
using EtheriT.Coker.Web.MVC.Startup;
using EtheriT.Coker.Application.Newsletter;
using EtheriT.Coker.Application.Permissions;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Application.Remote;
using EtheriT.Coker.Application.JsonObject;
using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.Application.Contact;
using System.Net;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.ThirdParty;
using EtheriT.Coker.Application.Processor;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Web.MVC.Middleware;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.ShoppingCart;
using EtheriT.Coker.Application.Shared.UserHabits;
using EtheriT.Coker.Application.UserHabits;
using Hangfire;
using Hangfire.SqlServer;
using EtheriT.Coker.Application.Filters;
using Hangfire.Dashboard;
using EtheriT.Coker.Application.BackgroundJob;

var builder = WebApplication.CreateBuilder(args);
var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews().AddNewtonsoftJson();
builder.Services.AddSingleton<JwtHelpers>();
builder.Services.AddMemoryCache()
    .AddSimpleCaptcha(builder =>
    {
        builder.UseMemoryStore();
    });

builder.Services
    .AddAuthentication(options =>
    {
        // custom scheme defined in .AddPolicyScheme() below
        options.DefaultScheme = "JWT_OR_COOKIE";
        options.DefaultChallengeScheme = "JWT_OR_COOKIE";
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
            ValidateIssuerSigningKey = false,

            // "1234567890123456" 應該從 IConfiguration 取得
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
        };
    })
    .AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
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
builder.Services.AddScoped<LoginUserData>();
builder.Services.AddScoped<ImportAppService>();
builder.Services.AddScoped<StringHandler>();
builder.Services.AddScoped<NavigationProvider>();
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
builder.Services.AddTransient<IDashboardAuthorizationFilter, HangfireDashboardAuthorizationFilter>();
builder.Services.AddScoped<UserHabitsWorking>();

//多語系
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);//要使用View多國語系的話就加這行程式碼

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.Configure<VirtualDirectory>(builder.Configuration.GetSection("VirtualDirectory"));
// Add services to the container.
builder.Services.AddControllersWithViews();

//item.UseSqlServer(configuration.GetConnectionString("Default"))

if (builder.Environment.EnvironmentName == "EPZA")
{
    builder.WebHost.UseStaticWebAssets();
}

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

// 添加 AntiforgeryDebugMiddleware
app.UseMiddleware<AntiforgeryDebugMiddleware>();

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
        MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Strict
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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 設定 Hangfire 儀表板（可以設置需要權限控制的路徑）
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { app.Services.GetRequiredService<IDashboardAuthorizationFilter>() },  // 使用 DI 解析授權過濾器
    IgnoreAntiforgeryToken = true,
    StatsPollingInterval = 60*1000
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

app.Run();
