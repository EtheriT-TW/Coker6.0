using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Freight;
using EtheriT.Coker.Application.HtmlContent;
using EtheriT.Coker.Application.Marquee;
using EtheriT.Coker.Application.Order;
using EtheriT.Coker.Application.Product;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.ShoppingCart;
using EtheriT.Coker.Application.Tag;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Resources;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var provider = builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();

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
builder.Services.AddTransient<ITagAppService, TagAppService>();
builder.Services.AddTransient<IWebMenuApplication, WebMenuApplication>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseVirtualDirectory("upload", builder.Configuration.GetValue<string>("VirtualDirectory:upload"));

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Page",
    pattern: "{key}/{id?}/{search?}",
    defaults: new { controller = "Page", action = "Index" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{id?}",
    defaults: new { controller = "Home", action = "Index" });

//var options = new RewriteOptions()
//        .AddRedirect("^Search/(.*)/(.*)", "Search?id=$&search=$2", 301);
//app.UseRewriter(options);

//app.UseRouting();

app.Run();
