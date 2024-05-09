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
using SimpleCaptcha;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();

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
    //iis setting
    //options.SuppressXFrameOptionsHeader = false;
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
builder.Services.AddTransient<IMarqueeAppService, MarqueeAppService>();
builder.Services.AddTransient<IOrderAppService, OrderAppService>();
builder.Services.AddTransient<ITokenAppService, TokenAppService>();
builder.Services.AddTransient<IShoppingCartAppService, ShoppingCartAppService>();
builder.Services.AddTransient<IProductAppService, ProductAppService>();
builder.Services.AddTransient<IFreightAppService, FreightAppService>();
builder.Services.AddTransient<IHtmlContentAppService, HtmlContentAppService>();
builder.Services.AddTransient<LoginUserData>();
builder.Services.AddTransient<StringHandler>();
builder.Services.AddTransient<MailAppService>();
builder.Services.AddTransient<ITagAppService, TagAppService>();
builder.Services.AddTransient<IWebMenuApplication, WebMenuApplication>();
builder.Services.AddTransient<IWebsiteApplication, WebsiteApplication>();
builder.Services.AddTransient<IFileUploadAppService, FileUploadAppService>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseMiddleware<CustomBadRequestMiddleware>();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
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
            new CookieOptions { HttpOnly = true });
    }
    /* iis setting
     *context.Response.Headers.Add("Content-Security-Policy",
                            "default-src *; script-src 'self' 'unsafe-inline' 'unsafe-eval' *.google.com *.googletagmanager.com *.googleadservices.com *.facebook.net *.jquery.com *.yimg.com *.google-analytics.com scaleflex.cloudimg.io googleads.g.doubleclick.net d.line-scdn.net cdn.ckeditor.com; style-src 'self' 'unsafe-inline' *.googleapis.com *.google.com cdnjs.cloudflare.com cdn.ckeditor.com; font-src 'self' data: fonts.gstatic.com cdnjs.cloudflare.com; img-src 'self' *.ezsale.tw *.facebook.com *.yahoo.com *.google.com *.google.com.tw *.google-analytics.com *.youtube.com i.ytimg.com ad.doubleclick.net googleads.g.doubleclick.net tr.line.me cdn.ckeditor.com data: blob:; frame-ancestors self *.ezsale.tw");
    */
    return next(context);
});
app.UseCookiePolicy(
    new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.Always,
        HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
        MinimumSameSitePolicy = SameSiteMode.Strict
    }
);

app.UseVirtualDirectory("upload", builder.Configuration.GetValue<string>("VirtualDirectory:upload"));
List<string> childOrgNames = new List<string>();
builder.Configuration.GetSection("WebConfig:childSiteOrgName").Bind(childOrgNames);

List<string> childFilePath = new List<string>();
builder.Configuration.GetSection("WebConfig:childPath").Bind(childFilePath);

if (childOrgNames != null)
{
    for (int i = 0; i < childOrgNames.Count; i++)
    {
        app.UseVirtualDirectory(
            $"upload/{childOrgNames[i]}",
            childFilePath[i]);
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

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
    defaults: new { controller = "Page", action = "Index" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{key?}/{id?}",
    defaults: new { controller = "Home", action = "Index" });

app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
    {
     {
         ".properties",
         "application/octet-stream"
     },{
         ".bcmap",
         "image/svg+xml"
     }
    })
});

//var options = new RewriteOptions()
//        .AddRedirect("^Search/(.*)/(.*)", "Search?id=$&search=$2", 301);
//app.UseRewriter(options);

//app.UseRouting();

app.Run();
