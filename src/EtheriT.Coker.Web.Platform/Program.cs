using EtheriT.Coker.Authentication.Backoffice;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Security;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews(options =>
        options.Filters.Add(new AuthorizeFilter(BackofficeAuthorizationPolicies.PlatformAccess)))
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = null);
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
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddScoped<IAuthorizationHandler, PlatformAccessHandler>();
builder.Services.AddSingleton<ViteManifestService>();
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
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PlatformHost}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapFallbackToController("Index", "PlatformHost");

app.Run();
