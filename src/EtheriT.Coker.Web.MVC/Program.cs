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
using EtheriT.Coker.Application.Import;

var builder = WebApplication.CreateBuilder(args);
var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews().AddNewtonsoftJson();
builder.Services.AddSingleton<JwtHelpers>();

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
    }); ;

builder.Services.AddAuthorization();

builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IAccountAppService, AccountAppService>();
builder.Services.AddTransient<ITokenAppService, TokenAppService>();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddTransient<IWebsiteApplication, WebsiteApplication>();
builder.Services.AddTransient<IMarqueeAppService, MarqueeAppService>();
builder.Services.AddTransient<IOrderAppService, OrderAppService>();
builder.Services.AddTransient<IMemberAppService, MemberAppService>();
builder.Services.AddTransient<IFreightAppService, FreightAppService>();
builder.Services.AddTransient<IProductAppService, ProductAppService>();
builder.Services.AddTransient<IHtmlContentAppService, HtmlContentAppService>();
builder.Services.AddTransient<ITechnicalCertificateAppService, TechnicalCertificateAppService>();
builder.Services.AddTransient<IWebMenuApplication, WebMenuApplication>();
builder.Services.AddTransient<LoginUserData>();
builder.Services.AddTransient<ImportAppService>();
builder.Services.AddTransient<ISpecificationAppService, SpecificationAppService>();
builder.Services.AddTransient<ITagAppService, TagAppService>();
builder.Services.AddTransient<IFileUploadAppService, FileUploadAppService>();
builder.Services.AddTransient<IObjectTypeAppService, ObjectTypeAppService>();
builder.Services.AddTransient<IArticleAppService, ArticleAppService>();

//多語系
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);//要使用View多國語系的話就加這行程式碼

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.Configure<VirtualDirectory>(builder.Configuration.GetSection("VirtualDirectory"));
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CokerDbContext>(item =>
    item.UseSqlServer(configuration.GetConnectionString("Default"))
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

//設定虛擬目錄
app.UseVirtualDirectory("upload", builder.Configuration.GetValue<string>("VirtualDirectory:upload"));
app.UseVirtualDirectory("shared", builder.Configuration.GetValue<string>("VirtualDirectory:Shared"));

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

app.Run();
