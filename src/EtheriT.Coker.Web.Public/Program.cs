using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Article;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Directory;
using EtheriT.Coker.Application.Freight;
using EtheriT.Coker.Application.HtmlContent;
using EtheriT.Coker.Application.Import;
using EtheriT.Coker.Application.Marquee;
using EtheriT.Coker.Application.Order;
using EtheriT.Coker.Application.Product;
using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.Product;
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
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
    options.HttpsPort = 5001;
});
builder.Services.AddAntiforgery(options =>
{
    // Set Cookie properties using CookieBuilder properties†.
    options.FormFieldName = "AntiforgeryFieldname";
    options.HeaderName = "X-CSRF-TOKEN-HEADERNAME";
    options.SuppressXFrameOptionsHeader = false;
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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

if (builder.Environment.EnvironmentName == "EPZA")
{
    builder.WebHost.UseStaticWebAssets();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
            new CookieOptions { HttpOnly = false });
    }

    return next(context);
});

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

//var options = new RewriteOptions()
//        .AddRedirect("^Search/(.*)/(.*)", "Search?id=$&search=$2", 301);
//app.UseRewriter(options);

//app.UseRouting();

app.Run();
