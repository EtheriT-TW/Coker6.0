using EtheriT.Coker.Authentication.Backoffice;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AuthorizeFilter(BackofficeAuthorizationPolicies.PlatformAccess)));
builder.Services.AddDbContext<CokerDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddCokerBackofficeAuthentication(
    builder.Configuration,
    enableJwtBearer: false,
    configureCookie: options =>
    {
        options.LoginPath = "/Home/AccessDenied";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

builder.Services.AddScoped<IAuthorizationHandler, PlatformAccessHandler>();
builder.Services.AddAuthorization(options =>
{
    var platformAccessPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddRequirements(new PlatformAccessRequirement())
        .Build();

    options.AddPolicy(BackofficeAuthorizationPolicies.PlatformAccess, platformAccessPolicy);
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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
