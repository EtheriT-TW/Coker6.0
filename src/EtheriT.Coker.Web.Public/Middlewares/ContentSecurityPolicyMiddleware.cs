using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Shared.Processor;

namespace EtheriT.Coker.Web.Public.Middlewares
{
    public class ContentSecurityPolicyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public ContentSecurityPolicyMiddleware(RequestDelegate next,
            IServiceProvider serviceProvider,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _env = env;
            _configuration = configuration;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/File/DecryptFile"))
            {
                await _next(context);
                return;
            }

            var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            using (var scope = _serviceProvider.CreateScope())
            {
                long siteId = _configuration.GetValue<long>("WebConfig:SiteId");
                var backstageUrl = _configuration.GetValue<string>("WebConfig:BackstageUrl") ?? "";
                var backstageHost = "";
                if (Uri.TryCreate(backstageUrl, UriKind.Absolute, out var uri))
                {
                    // 只取 scheme://host(:port) 這一段，避免 path
                    backstageHost = $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? "" : $":{uri.Port}")}";
                }

                var dbContext = scope.ServiceProvider.GetRequiredService<CokerDbContext>();
                var item = dbContext.StoreSetDetail.Where(e => e.FK_WebsiteId == siteId && e.FK_StoreSetId == 2).FirstOrDefault();
                var otherPayElement = dbContext.ThirdPartyKeypairValues.Where(e => e.FK_WebsiteId == siteId && e.FK_ThirdPartyKeypairId == 11).FirstOrDefault();
                string selfInline = $"nonce-{nonce}";
                string connectSrc = $"'self' *";
                context.Items["CSPNonce"] = nonce;
                bool isSitemapRequest = context.Request.Path.HasValue && (
                    context.Request.Path.Value.EndsWith("/sitemap", StringComparison.OrdinalIgnoreCase) ||
                    (
                        (context.Request.Path.Value.EndsWith("/ShoppingCar", StringComparison.OrdinalIgnoreCase) ||
                        context.Request.Path.Value.EndsWith("/Member", StringComparison.OrdinalIgnoreCase)) &&
                        otherPayElement != null && !string.IsNullOrEmpty(otherPayElement.Value)
                    )
                );

                if ((item != null && !string.IsNullOrEmpty(item.value)) || isSitemapRequest)
                {
                    selfInline = $"unsafe-inline";
                }
                if (_env.IsProduction())
                {
                    connectSrc = $"'self' {backstageHost} *.google.com *.google-analytics.com *.googleapis.com *.googleadservices.com https://ad.doubleclick.net https://stats.g.doubleclick.net https://ecpg-stage.ecpay.com.tw https://ecpg.ecpay.com.tw https://remotejs.com wss://remotejs.com";
                }

                var scriptSrc = AppendAdditionalSources(
                    $"'self' '{selfInline}' *.google.com *.googletagmanager.com *.googleadservices.com *.googleapis.com *.facebook.net *.jquery.com *.yimg.com *.google-analytics.com translate.google.com scaleflex.cloudimg.io googleads.g.doubleclick.net d.line-scdn.net cdn.ckeditor.com remotejs.com www.instagram.com platform.twitter.com www.threads.com www.threads.net https://ecpg-stage.ecpay.com.tw https://ecpg.ecpay.com.tw https://applepay.cdn-apple.com logistics-stage.ecpay.com.tw logistics.ecpay.com.tw postgate-stage.ecpay.com.tw postgate.ecpay.com.tw glogistics.ecpay.com.tw https://cdn.jsdelivr.net",
                    "AdditionalScripts");
                var styleSrc = AppendAdditionalSources(
                    $"'self' '{selfInline}' {ExternalStylesheetPolicy.CspSourceExpression}",
                    "AdditionalStyles");
                var fontSrc = AppendAdditionalSources(
                    "'self' data: fonts.gstatic.com cdnjs.cloudflare.com https://ecpg-stage.ecpay.com.tw https://ecpg.ecpay.com.tw https://applepay.cdn-apple.com logistics-stage.ecpay.com.tw logistics.ecpay.com.tw postgate-stage.ecpay.com.tw postgate.ecpay.com.tw",
                    "AdditionalFonts");
                var imageSrc = AppendAdditionalSources(
                    "'self' *.ezsale.tw *.facebook.com https://static.xx.fbcdn.net https://usage.trackjs.com *.yahoo.com *.google.com *.google.com.tw *.google-analytics.com *.googletagmanager.com *.gstatic.com *.googleapis.com *.youtube.com i.ytimg.com *.cdninstagram.com pbs.twimg.com abs.twimg.com ton.twimg.com ad.doubleclick.net googleads.g.doubleclick.net tr.line.me cdn.ckeditor.com i.imgur.com lh3.googleusercontent.com cdn.discordapp.com githubusercontent.com images.unsplash.com cdn.pixabay.com res.cloudinary.com scaleflex.cloudimg.io data: blob: https://ecpg-stage.ecpay.com.tw https://ecpg.ecpay.com.tw logistics-stage.ecpay.com.tw logistics.ecpay.com.tw postgate-stage.ecpay.com.tw postgate.ecpay.com.tw",
                    "AdditionalImages");
                var frameSrc = AppendAdditionalSources(
                    "'self' *.ezsale.tw *.google.com *.google.com.tw *.youtube.com *.youtube-nocookie.com *.facebook.com *.instagram.com *.threads.com *.threads.net platform.twitter.com syndication.twitter.com *.googletagmanager.com *.doubleclick.net v.qq.com https://applepay.cdn-apple.com/",
                    "AdditionalFrames");
                connectSrc = AppendAdditionalSources(connectSrc, "AdditionalConnections");
                connectSrc += " https://syndication.twitter.com https://www.threads.com https://www.threads.net https://graph.facebook.com https://www.facebook.com";
                var frameAncestors = AppendAdditionalSources(
                    "'self' *.ezsale.tw",
                    "AdditionalFrameAncestors");

                // 將 nonce 存入 HttpContext.Items

                // 添加 CSP(內容限制) header
                // google 翻譯 script-src、style-src要加上 'unsafe-inline' 目前還找不到解決方案 
                context.Response.Headers["Content-Security-Policy"] =
                    $"default-src 'self';" +
                    $"script-src {scriptSrc}; " +
                    $"style-src {styleSrc}; " +
                    $"font-src {fontSrc}; " +
                    $"img-src {imageSrc}; " +
                    $"frame-src {frameSrc};" +
                    $"connect-src {connectSrc};" +
                    $"frame-ancestors {frameAncestors};";
                //cache 限制設定
                context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
                //Pragma 為http 1.0以下使用，以上已被 Cache-Control取代
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
                //防止瀏覽器進行 MIME 嗅探
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            }
            var originalBodyStream = context.Response.Body;
            using (var newBodyStream = new MemoryStream())
            {
                bool isSitemapRequest = context.Request.Path.HasValue &&
                        (
                            context.Request.Path.Value.EndsWith("/api/Captcha/index", StringComparison.OrdinalIgnoreCase) ||
                            context.Request.Path.Value.EndsWith("/ShoppingCar", StringComparison.OrdinalIgnoreCase) ||
                            context.Request.Path.Value.EndsWith("/sitemap", StringComparison.OrdinalIgnoreCase)
                        );
                if (isSitemapRequest) await _next(context); // 執行後續的管道（包括 Razor 渲染）
                else
                {
                    context.Response.Body = newBodyStream;

                    await _next(context); // 執行後續的管道（包括 Razor 渲染）

                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    var responseBody = await new StreamReader(newBodyStream).ReadToEndAsync();

                    // 只替換不含 nonce 的 <script> 標籤
                    var modifiedBody = Regex.Replace(
                        responseBody,
                        @"<script(?![^>]*\bnonce=)(?![^>]*\bsrc=)([^>]*)>",
                        $"<script nonce=\"{nonce}\">",
                        RegexOptions.IgnoreCase
                    );

                    context.Response.Body = originalBodyStream;
                    await context.Response.WriteAsync(modifiedBody);
                }
            }
        }

        private string AppendAdditionalSources(string currentSources, string settingName)
        {
            var additionalSources = _configuration
                .GetSection($"ContentSecurityPolicy:{settingName}")
                .GetChildren()
                .Select(item => item.Value?.Trim())
                .Where(IsValidAdditionalSource)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return string.Join(" ", new[] { currentSources }.Concat(additionalSources!));
        }

        private static bool IsValidAdditionalSource(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            // 每一筆只能是一個來源，避免設定值透過分號或空白插入其他 CSP 規則。
            return source.IndexOfAny([';', ',', '\r', '\n', '\t', ' ']) < 0;
        }
    }
}
