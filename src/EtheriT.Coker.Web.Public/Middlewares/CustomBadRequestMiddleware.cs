

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace EtheriT.Coker.Web.Public.Middlewares
{
    public class CustomBadRequestMiddleware
    {
        private readonly RequestDelegate _next;
        public CustomBadRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
            if (context.Response.StatusCode == 400)
            {
                var json = new
                {
                    context.Response.StatusCode,
                    Message = "Bad Request Error."
                };
                await context.Response.WriteAsync(JsonConvert.SerializeObject(json));
            }
        }
    }
}
