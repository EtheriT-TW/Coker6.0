using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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
    defaults:new { controller = "Page", action = "Index" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{id?}",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller=Home}/{action=Index}/{id?}");

//var options = new RewriteOptions()
//        .AddRewrite(@"^(\w+)", "Page/Index/$1", skipRemainingRules: true)
//        .AddRewrite(@"^(\w+)/(\d+)", "Page/Index/$1/$2", skipRemainingRules: true)
//        .AddRewrite(@"^(\w+)/(\d+)/(\d+)", "Page/Index/$1/$2/$3", skipRemainingRules: true)
//        .AddRewrite(@"^(\w+)/(\d+)/(\d+)/(\w+)", "Page/Index/$1/$2/$3/$4", skipRemainingRules: true);

//app.UseRewriter(options);

app.Run();
