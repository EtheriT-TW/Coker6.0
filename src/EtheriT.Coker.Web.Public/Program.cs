using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache()
    .AddSimpleCaptcha(builder =>
    {
        builder.UseMemoryStore();
        builder.AddConfiguration(options =>
        {
            options.CodeLength = 4;
            options.ImageWidth = 100;
            options.ImageHeight = 40;
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "Page",
    pattern: "{key}/{id?}/{search?}",
    defaults: new { controller = "Page", action = "Index" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{id?}",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller=Home}/{action=Index}/{id?}");

//var options = new RewriteOptions()
//        .AddRedirect("^Search/(.*)/(.*)", "Search?id=$&search=$2", 301);
//app.UseRewriter(options);

//app.UseRouting();

app.Run();
