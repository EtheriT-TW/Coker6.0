using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EtheriT.Coker.Authentication.ExternalProviders;

public static class ExternalProviderAuthenticationExtensions
{
    public static AuthenticationBuilder AddCokerExternalProviders(
        this AuthenticationBuilder authentication,
        IConfiguration configuration,
        string contentRootPath)
    {
        authentication.AddCookie(ExternalAuthenticationDefaults.TemporaryCookieScheme, options =>
        {
            options.Cookie.Name = ExternalAuthenticationDefaults.TemporaryCookieName;
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        });

        var authenticationConfig = configuration.GetSection("Authentication");
        AddLine(authentication, authenticationConfig.GetSection("Line"));
        AddGoogle(authentication, authenticationConfig.GetSection("Google"));
        AddFacebook(authentication, authenticationConfig.GetSection("Facebook"));
        AddApple(authentication, authenticationConfig.GetSection("Apple"), contentRootPath);

        return authentication;
    }

    private static void AddLine(AuthenticationBuilder authentication, IConfigurationSection config)
    {
        if (string.IsNullOrEmpty(config["ChannelId"]) || string.IsNullOrEmpty(config["ChannelSecret"]))
        {
            return;
        }

        authentication.AddLine(options =>
        {
            options.ClientId = config["ChannelId"] ?? "";
            options.ClientSecret = config["ChannelSecret"] ?? "";
            options.CallbackPath = "/SigninLine";
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.SignInScheme = ExternalAuthenticationDefaults.TemporaryCookieScheme;
            options.SaveTokens = true;

            options.Events.OnCreatingTicket = ctx =>
            {
                var idToken = ctx.TokenResponse.Response?.RootElement.GetProperty("id_token").GetString();
                if (!string.IsNullOrEmpty(idToken))
                {
                    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
                    if (jwt.Payload.TryGetValue("email", out var email))
                    {
                        ctx.Identity?.AddClaim(new Claim(ClaimTypes.Email, email?.ToString()!));
                    }

                    if (jwt.Payload.TryGetValue("name", out var name))
                    {
                        ctx.Identity?.AddClaim(new Claim(ClaimTypes.Name, name?.ToString()!));
                    }
                }

                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAuthorizationEndpoint = context =>
            {
                var uri = new UriBuilder(context.RedirectUri);
                var query = QueryHelpers.ParseQuery(uri.Query);
                var updated = query.ToDictionary(item => item.Key, item => item.Value.ToString());
                updated["bot_prompt"] = "normal";
                uri.Query = string.Join("&", updated.Select(item => $"{item.Key}={item.Value}"));
                context.Response.Redirect(uri.ToString());
                return Task.CompletedTask;
            };

            options.CorrelationCookie.SameSite = SameSiteMode.None;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        });
    }

    private static void AddGoogle(AuthenticationBuilder authentication, IConfigurationSection config)
    {
        if (string.IsNullOrEmpty(config["ClientId"]) || string.IsNullOrEmpty(config["ClientSecret"]))
        {
            return;
        }

        authentication.AddGoogle(options =>
        {
            options.ClientId = config["ClientId"] ?? "";
            options.ClientSecret = config["ClientSecret"] ?? "";
            options.CallbackPath = "/signin-google";
            options.SignInScheme = ExternalAuthenticationDefaults.TemporaryCookieScheme;
        });
    }

    private static void AddFacebook(AuthenticationBuilder authentication, IConfigurationSection config)
    {
        if (string.IsNullOrEmpty(config["AppId"]) || string.IsNullOrEmpty(config["AppSecret"]))
        {
            return;
        }

        authentication.AddFacebook(options =>
        {
            options.AppId = config["AppId"] ?? "";
            options.AppSecret = config["AppSecret"] ?? "";
            options.CallbackPath = "/signin-facebook";
            options.SignInScheme = ExternalAuthenticationDefaults.TemporaryCookieScheme;
        });
    }

    private static void AddApple(
        AuthenticationBuilder authentication,
        IConfigurationSection config,
        string contentRootPath)
    {
        var privateKeyPath = config["PrivateKeyPath"];
        if (string.IsNullOrEmpty(config["ClientId"]) ||
            string.IsNullOrEmpty(config["KeyId"]) ||
            string.IsNullOrEmpty(config["TeamId"]) ||
            string.IsNullOrEmpty(privateKeyPath))
        {
            return;
        }

        authentication.AddApple("Apple", options =>
        {
            options.ClientId = config["ClientId"] ?? "";
            options.KeyId = config["KeyId"] ?? "";
            options.TeamId = config["TeamId"] ?? "";
            options.CallbackPath = "/signin-apple";
            options.SignInScheme = ExternalAuthenticationDefaults.TemporaryCookieScheme;
            var fileProvider = new PhysicalFileProvider(contentRootPath);
            options.UsePrivateKey(fileName => fileProvider.GetFileInfo(privateKeyPath));
        });
    }
}
