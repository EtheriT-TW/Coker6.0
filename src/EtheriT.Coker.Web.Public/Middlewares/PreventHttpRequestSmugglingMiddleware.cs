namespace EtheriT.Coker.Web.Public.Middlewares
{
    public class PreventHttpRequestSmugglingMiddleware
    {
        private readonly RequestDelegate _next;
        public PreventHttpRequestSmugglingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            // 檢查是否有多個 Content-Length 標頭
            if (context.Request.Headers.ContainsKey("Content-Length"))
            {
                var contentLengthHeaders = context.Request.Headers["Content-Length"];
                if (contentLengthHeaders.Count > 1)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Bad Request: Multiple Content-Length headers detected.");
                    return;
                }
            }

            if (context.Request.Headers.ContainsKey("Content-Length") && context.Request.Headers.ContainsKey("Transfer-Encoding"))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Bad Request: Both Content-Length and Transfer-Encoding headers are present.");
                return;
            }

            // 如果沒有問題，繼續處理請求
            await _next(context);
        }
    }
}
