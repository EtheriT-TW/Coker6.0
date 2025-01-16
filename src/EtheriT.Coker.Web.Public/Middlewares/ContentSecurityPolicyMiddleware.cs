using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
        public async Task InvokeAsync(HttpContext context) {
            var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            using (var scope = _serviceProvider.CreateScope()) {
                long siteId = _configuration.GetValue<long>("WebConfig:SiteId");
                var dbContext = scope.ServiceProvider.GetRequiredService<CokerDbContext>();
                var item = dbContext.StoreSetDetail.Where(e => e.FK_WebsiteId == siteId && e.FK_StoreSetId == 2).FirstOrDefault();
                string selfInline = $"nonce-{nonce}";
                string connectSrc = "*;";
                context.Items["CSPNonce"] = nonce;
                bool isSitemapRequest = context.Request.Path.HasValue &&
                        context.Request.Path.Value.ToLowerInvariant().EndsWith("/sitemap", StringComparison.OrdinalIgnoreCase);

                if ((item != null && !string.IsNullOrEmpty(item.value))|| isSitemapRequest) {
                    selfInline = $"unsafe-inline";
                }
                if (_env.IsProduction()) {
                    connectSrc = "'self' *.google.com *.google-analytics.com *.googleapis.com;";
                }
                
                // 將 nonce 存入 HttpContext.Items
                
                // 添加 CSP(內容限制) header
                // google 翻譯 script-src、style-src要加上 'unsafe-inline' 目前還找不到解決方案 
                context.Response.Headers["Content-Security-Policy"] =
                    $"default-src 'self';" +
                    $"script-src 'self' '{selfInline}' *.google.com *.googletagmanager.com *.googleadservices.com *.googleapis.com *.facebook.net *.jquery.com *.yimg.com *.google-analytics.com scaleflex.cloudimg.io googleads.g.doubleclick.net d.line-scdn.net cdn.ckeditor.com remotejs.com www.instagram.com; " +
                    $"style-src 'self' '{selfInline}' *.googleapis.com  *.google.com *.gstatic.com cdnjs.cloudflare.com cdn.ckeditor.com; " +
                    $"font-src 'self' data: fonts.gstatic.com cdnjs.cloudflare.com; " +
                    $"img-src 'self' *.ezsale.tw *.facebook.com *.yahoo.com *.google.com *.google.com.tw *.google-analytics.com *.googletagmanager.com *.gstatic.com *.googleapis.com *.youtube.com i.ytimg.com ad.doubleclick.net googleads.g.doubleclick.net tr.line.me cdn.ckeditor.com data: blob:; " +
                    $"frame-src 'self' *.ezsale.tw *.google.com *.youtube.com *.youtube-nocookie.com *.facebook.com *.instagram.com *.googletagmanager.com *.doubleclick.net;" +
                    $"connect-src {connectSrc}" +
                    $"frame-ancestors 'self' *.ezsale.tw;";
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
                        context.Request.Path.Value.ToLowerInvariant().EndsWith("/api/Captcha/index", StringComparison.OrdinalIgnoreCase);
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
                        @"<script(?![^>]*\bnonce=)(?![^>]*\bsrc=)[^>]*>",
                        $"<script nonce=\"{nonce}\">",
                        RegexOptions.IgnoreCase
                    );

                    context.Response.Body = originalBodyStream;
                    await context.Response.WriteAsync(modifiedBody);
                }
            }
        }
    }
}
